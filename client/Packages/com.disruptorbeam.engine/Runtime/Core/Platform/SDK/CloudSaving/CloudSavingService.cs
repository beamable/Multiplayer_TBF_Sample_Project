using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Concurrent;
using Beamable.Api;
using Beamable.Common;
using Beamable.Coroutines;
using Beamable.Spew;

namespace Beamable.Platform.SDK.CloudSaving
{
   public class CloudSavingService : PlatformSubscribable<ManifestResponse, ManifestResponse>
   {
      private const string ServiceName = "cloudsaving";
      private ManifestResponse _localManifest;
      private PlatformService _platform;
      private PlatformRequester _requester;
      private WaitForSecondsRealtime _delay;
      private CoroutineService _coroutineService;
      private ConcurrentDictionary<string, string> _pendingUploads = new ConcurrentDictionary<string, string>();
      private ConcurrentDictionary<string, string> _previouslyDownloaded = new ConcurrentDictionary<string, string>();

      private IEnumerator _fileWatchingRoutine;

      public Action<ManifestResponse> UpdateReceived;
      public Action<CloudSavingError> OnError;

      private ConnectivityService _connectivityService;

      private LocalCloudDataPath localCloudDataPath => new LocalCloudDataPath(_platform);

      public string LocalCloudDataFullPath => localCloudDataPath.dataPath;
      private string _manifestPath => Path.Combine(localCloudDataPath.manifestPath, "cloudDataManifest.json");

      public CloudSavingService(PlatformService platform, PlatformRequester requester,
         CoroutineService coroutineService) : base(platform, requester, ServiceName)
      {
         _platform = platform;
         _requester = requester;
         _coroutineService = coroutineService;
         _connectivityService = new ConnectivityService(_coroutineService);
      }

      public void Init(object user = null)
      {
         // TODO: This really needs to be pulled in from the CloudSavingConfiguration object.
         Initialize(10);
      }

      private Promise<Unit> Initialize(int pollingIntervalSecs)
      {
         _delay = new WaitForSecondsRealtime(pollingIntervalSecs);

         StopFileWatcherRoutine();
         LoadLocalManifest();

         return EnsureRemoteManifest()
            .FlatMap(SyncRemoteContent)
            .Then(_ =>
            {
               _fileWatchingRoutine = StartFileSystemWatchingCoroutine();
               _platform.Notification.Subscribe(
                  "cloudsaving.datareplaced",
                  OnReplacedNtf
               );
            }).Map(_ => PromiseBase.Unit);
      }

      private Promise<Unit> SyncRemoteContent(ManifestResponse manifestResponse)
      {
         // Replacement is essentially a "force sync". Therefore we want to ensure that we
         // blow away everything.
         if (manifestResponse.replacement)
         {
            DeleteLocalUserData();
            Directory.CreateDirectory(LocalCloudDataFullPath);
         }

         // This method will also commit a new remote manifest AND store the manifest locally.
         return DownloadUserData(manifestResponse);
      }

      private IEnumerator StartFileSystemWatchingCoroutine()
      {
         var routine = WatchDirectoryForChanges();
         _coroutineService.StartCoroutine(routine);
         return routine;
      }

      private IEnumerator WatchDirectoryForChanges()
      {
         while (true)
         {
            yield return _delay;

            if (!_connectivityService.HasConnectivity)
            {
               continue;
            }

            _pendingUploads.Clear();

            try
            {
               foreach (var filepath in Directory.GetFiles(LocalCloudDataFullPath, "*.*", SearchOption.AllDirectories))
               {
                  SetPendingUploads(filepath);
               }
            }
            catch (DirectoryNotFoundException e)
            {
               // The local folder was not created for this player at the time that this coroutine ran.
               // This could be because the initialization code hasn't finished by the time it
               // gets here. The next time the coroutine runs it should pick up the file changes.
               PlatformLogger.Log(e.Message);
            }

            if (_pendingUploads.Count > 0)
            {
               yield return UploadUserData();
            }
         }
      }

      private void InvokeError(string reason, Exception inner)
      {
         OnError?.Invoke(new CloudSavingError(reason, inner));
      }

      private Action<Exception> ProvideErrorCallback(string methodName)
      {
         return (ex) => { InvokeError($"{methodName} Failed: {ex?.Message ?? "unknown reason"}", ex); };
      }

      public Promise<ManifestResponse> EnsureRemoteManifest()
      {
         return FetchUserManifest().RecoverWith(error =>
         {
            if (error is PlatformRequesterException notFound && notFound.Status == 404)
            {
               return CreateEmptyManifest();
            }

            throw error;
         });
      }

      private void StopFileWatcherRoutine()
      {
         if (_fileWatchingRoutine != null)
         {
            _coroutineService.StopCoroutine(_fileWatchingRoutine);
         }
      }

      private void LoadLocalManifest()
      {
         Directory.CreateDirectory(LocalCloudDataFullPath);

         if (File.Exists(_manifestPath))
         {
            _localManifest = _localManifest == null
               ? JsonUtility.FromJson<ManifestResponse>(File.ReadAllText(_manifestPath))
               : _localManifest;
         }
      }

      private Promise<ManifestResponse> UploadUserData()
      {
         var (upload, uploadMap) = GenerateUploadObjectRequestWithMapping();
         //TODO: We may want to handle this better, so we don't create empty manifests
         if (upload.request.Count <= 0)
         {
            return Promise<ManifestResponse>.Failed(new Exception("Upload is empty"));
         }

         return HandleRequest(upload,
               uploadMap,
               Method.PUT,
               "/data/uploadURL"
            ).FlatMap(_ => CommitManifest(upload))
            .RecoverWith(_ => UploadUserData())
            .Error(ProvideErrorCallback(nameof(UploadUserData)));
      }

      private (UploadManifestRequest, List<KeyValuePair<string, string>>) GenerateUploadObjectRequestWithMapping()
      {
         var uploadRequest = new List<ManifestEntry>();
         var uploadMap = new List<KeyValuePair<string, string>>();
         var allFilesAndDirectories = new List<string>();

         if (_pendingUploads != null && _pendingUploads.Count > 0)
         {
            foreach (var item in _pendingUploads)
            {
               allFilesAndDirectories.Add(Path.Combine(LocalCloudDataFullPath, item.Key));
            }
         }

         foreach (var fullPathToFile in allFilesAndDirectories)
         {
            var objectKey = NormalizeS3Path(fullPathToFile, LocalCloudDataFullPath);
            uploadMap.Add(new KeyValuePair<string, string>(objectKey, fullPathToFile));
            var contentInfo = new FileInfo(fullPathToFile);
            var contentLength = contentInfo.Length;
            var lastModified = long.Parse(contentInfo.LastWriteTime.ToString("yyyyMMddHHmmss"));

            var uploadObjectRequest = new ManifestEntry(objectKey,
               (int)contentLength,
               GenerateChecksum(fullPathToFile),
               null,
               _platform.User.id,
               lastModified);

            _previouslyDownloaded[fullPathToFile] = GenerateChecksum(fullPathToFile);

            uploadRequest.Add(uploadObjectRequest);
         }

         return (new UploadManifestRequest(uploadRequest), uploadMap);
      }

      private Promise<Unit> WriteManifestToDisk(ManifestResponse response)
      {
         try
         {
            Directory.CreateDirectory(Path.GetDirectoryName(_manifestPath));
            _localManifest = response;
            File.WriteAllText(_manifestPath, JsonUtility.ToJson(_localManifest, true));
            return Promise<Unit>.Successful(PromiseBase.Unit);
         }
         catch (Exception ex)
         {
            return Promise<Unit>.Failed(ex).Error(ProvideErrorCallback(nameof(WriteManifestToDisk)));
         }
      }

      private Promise<Unit> DownloadUserData(ManifestResponse manifestResponse)
      {
         var downloadRequest = GenerateDownloadRequest(manifestResponse);
         var downloadMap = GenerateDownloadMap(manifestResponse);
         return HandleRequest(new GetS3DownloadURLsRequest(downloadRequest),
               downloadMap,
               Method.GET,
               "/data/downloadURL"
            )
            .Error(ProvideErrorCallback(nameof(DownloadUserData)))
            .Map(__ =>
            {
               if (manifestResponse.replacement)
               {
                  var upload = new UploadManifestRequest(new List<ManifestEntry>());
                  foreach (var r in manifestResponse.manifest)
                  {
                     upload.request.Add(new ManifestEntry(r.key,
                        r.size,
                        r.eTag,
                        null,
                        _platform.User.id,
                        r.lastModified)
                     );
                  }

                  CommitManifest(upload).Then(_ =>
                  {
                     // We want to ensure that we explicitly invoke the event with the ORIGINAL manifest.
                     UpdateReceived?.Invoke(manifestResponse);
                  });
               }
               else
               {
                  if (downloadRequest.Count > 0)
                  {
                     WriteManifestToDisk(manifestResponse).Then(_ =>
                     {
                        UpdateReceived?.Invoke(manifestResponse);
                     });
                  }
               }
               return PromiseBase.Unit;
            });
      }

      private List<GetS3ObjectRequest> GenerateDownloadRequest(ManifestResponse manifestResponse)
      {
         var force = manifestResponse.replacement;
         var downloadRequest = new List<GetS3ObjectRequest>();
         if (force)
         {
            foreach (var responseObj in manifestResponse.manifest)
            {
               _previouslyDownloaded[responseObj.key] = responseObj.eTag;
            }
         }
         else
         {
            // TODO: Maybe DiffManifest needs to be split out?
            _previouslyDownloaded = DiffManifest(manifestResponse);
         }

         foreach (var s3Object in _previouslyDownloaded)
         {
            downloadRequest.Add(new GetS3ObjectRequest(s3Object.Value));
         }

         return downloadRequest;
      }
      private List<KeyValuePair<string, string>> GenerateDownloadMap(ManifestResponse manifestResponse)
      {
         var downloadMap = new List<KeyValuePair<string, string>>();
         foreach (var s3Object in manifestResponse.manifest)
         {
            var objectFullName = Path.Combine(LocalCloudDataFullPath, s3Object.key);
            downloadMap.Add(new KeyValuePair<string, string>(s3Object.eTag, objectFullName));
         }
         return downloadMap;
      }

      private Promise<ManifestResponse> CreateEmptyManifest()
      {
         var emptyManifest = new UploadManifestRequest(new List<ManifestEntry>());
         return CommitManifest(emptyManifest);
      }

      private ConcurrentDictionary<string, string> DiffManifest(ManifestResponse response)
      {
         var newManifestDict = new ConcurrentDictionary<string, string>();

         if (_localManifest != null)
         {
            foreach (var s3Object in _localManifest.manifest)
            {
               _previouslyDownloaded[s3Object.key] = s3Object.eTag;
            }

            foreach (var s3Object in response.manifest)
            {
               if (!_previouslyDownloaded.ContainsKey(s3Object.key) ||
                   !_previouslyDownloaded[s3Object.key].Equals(s3Object.eTag))
               {
                  newManifestDict[s3Object.key] = s3Object.eTag;
               }
            }
         }
         else
         {
            foreach (var s3Object in response.manifest)
            {
               newManifestDict[s3Object.key] = s3Object.eTag;
            }
         }

         WriteManifestToDisk(response);

         return newManifestDict;
      }

      private Promise<List<Unit>> HandleRequest<T>(T request, List<KeyValuePair<string, string>> map, Method method,
         string endpoint)
      {
         var promiseList = new HashSet<Promise<Unit>>();
         var failedPromiseList = new HashSet<Promise<Unit>>();
         return GetPresignedURL(request, endpoint).FlatMap(presignedURLS =>
         {
            foreach (var response in presignedURLS.response)
            {
               foreach (var filepath in map)
               {
                  if (filepath.Key == response.objectKey)
                  {
                     promiseList.Add(GetObjectFromS3(filepath.Value, response, method));
                  }
               }
            }

            return Promise.Sequence(promiseList.ToList());
         });
      }

      private Promise<Unit> GetObjectFromS3(string fullPathToFile, PreSignedURL url, Method method)
      {
         return MakeRequestToS3(
            BuildS3Request(method, url.url, fullPathToFile)
         ).Map(_ => PromiseBase.Unit);
      }

      private Promise<EmptyResponse> MakeRequestToS3(UnityWebRequest request)
      {
         var result = new Promise<EmptyResponse>();
         var op = request.SendWebRequest();
         op.completed += _ => HandleResponse(result, request);
         return result;
      }

      private void HandleResponse(Promise<EmptyResponse> promise, UnityWebRequest request)
      {
         request.Dispose();
         promise.CompleteSuccess(new EmptyResponse());
      }

      private UnityWebRequest BuildS3Request(Method method, string uri, string fileName)
      {
         UnityWebRequest request;

         if (fileName != null && method == Method.GET)
         {
            request = new UnityWebRequest(uri)
            {
               downloadHandler = new DownloadHandlerFile(fileName),

               method = method.ToString()
            };
         }
         else
         {
            var upload = new UploadHandlerRaw(File.ReadAllBytes(fileName))
            {
               contentType = "application/octet-stream"
            };
            request = new UnityWebRequest(uri)
            {
               uploadHandler = upload,
               method = method.ToString()
            };
         }

         return request;
      }

      private Promise<ManifestResponse> FetchUserManifest()
      {
         var manifestRequest = new FetchManifestRequest(_platform.User.id);
         return _requester.Request<ManifestResponse>(
               Method.GET,
               string.Format($"/basic/cloudsaving?playerId={manifestRequest.playerId}"),
               null)
            .Error(ProvideErrorCallback(nameof(FetchUserManifest)));
      }

      private Promise<URLResponse> GetPresignedURL<T>(T request, string endpoint)
      {
         return _requester.Request<URLResponse>(
               Method.POST,
               string.Format($"/basic/cloudsaving{endpoint}"),
               request)
            .Error(ProvideErrorCallback(nameof(GetPresignedURL)));
      }

      private Promise<ManifestResponse> CommitManifest(UploadManifestRequest request)
      {
         return _requester.Request<ManifestResponse>(
            Method.PUT,
            string.Format($"/basic/cloudsaving/data/commitManifest"),
            request
         ).Then(res =>
            {
               res.replacement = false;
               WriteManifestToDisk(res);
            }
         ).Error(ProvideErrorCallback(nameof(CommitManifest)));
      }

      private string NormalizeS3Path(string key, string path)
      {
         return key.Remove(0, path.Length + 1).Replace(@"\", "/");
      }


      private void SetPendingUploads(string filePath)
      {
         var checksumEqual = _previouslyDownloaded.ContainsKey(filePath) && _previouslyDownloaded[filePath].Equals(GenerateChecksum(filePath));
         var missingKey = !_previouslyDownloaded.ContainsKey(filePath);
         var fileLengthNotZero = new FileInfo(filePath).Length > 0;
         if ((!checksumEqual || missingKey) && fileLengthNotZero)
         {
            _pendingUploads[filePath] = GenerateChecksum(filePath);
         }
      }

      private void DeleteLocalUserData()
      {
         _pendingUploads.Clear();
         _localManifest = null;
         _previouslyDownloaded.Clear();

         if (File.Exists(_manifestPath))
         {
            File.Delete(_manifestPath);
         }

         if (Directory.Exists(LocalCloudDataFullPath))
         {
            Directory.Delete(LocalCloudDataFullPath, true);
         }
      }

      protected override string CreateRefreshUrl(string scope = null)
      {
         return "/basic/cloudsaving";
      }

      private static string GenerateChecksum(string content)
      {
         try
         {
            using (var md5 = MD5.Create())
            {
               using (var stream = File.OpenRead(content))
               {
                  return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
               }
            }
         }
         catch
         {
            //Waiting for release.
            return GenerateChecksum(content);
         }
      }

      private void OnReplacedNtf(object _)
      {
         EnsureRemoteManifest()
             .FlatMap(SyncRemoteContent);
      }

      protected override Promise<ManifestResponse> ExecuteRequest(PlatformRequester requester, string url)
      {
         return base.ExecuteRequest(requester, url).RecoverWith(err =>
         {
            if (err is PlatformRequesterException notFound && notFound.Status == 404)
            {
               return CreateEmptyManifest();
            }

            return Promise<ManifestResponse>.Failed(err);
         });
      }

      protected override void OnRefresh(ManifestResponse data)
      {
         if (JsonUtility.ToJson(_localManifest) != JsonUtility.ToJson(data))
         {
            SyncRemoteContent(data);
         }
      }
   }


   [Serializable]
   public class S3Object
   {
      public string bucketName;
      public string key;
      public long size;
      public DateTime lastModified;
      public Owner owner;
      public string storageClass;
      public string eTag;
   }

   [Serializable]
   public class Owner
   {
      public string displayName;
      public string id;
   }

   [Serializable]
   public class PreSignedURL
   {
      public string objectKey;
      public string url;
   }

   [Serializable]
   public class URLResponse
   {
      public List<PreSignedURL> response;

      public URLResponse(List<PreSignedURL> response)
      {
         this.response = response;
      }
   }

   [Serializable]
   public class UploadManifestRequest
   {
      public List<ManifestEntry> request;

      public UploadManifestRequest(List<ManifestEntry> request)
      {
         this.request = request;
      }
   }

   [Serializable]
   public class ManifestEntry
   {
      public string objectKey;
      public int sizeInBytes;
      public string checksum;
      public List<MetadataPair> metadata;
      public long playerId;
      public long lastModified;

      public ManifestEntry(string objectKey, int sizeInBytes, string checksum, List<MetadataPair> metadata,
         long playerId, long lastModified)
      {
         this.objectKey = objectKey;
         this.sizeInBytes = sizeInBytes;
         this.checksum = checksum;
         this.metadata = metadata;
         this.playerId = playerId;
         this.lastModified = lastModified;
      }
   }

   [Serializable]
   public class MetadataPair
   {
      public string key;
      public string value;

      public MetadataPair(string key, string value)
      {
         this.key = key;
         this.value = value;
      }
   }

   [Serializable]
   public class FetchManifestRequest
   {
      public long playerId;

      public FetchManifestRequest(long playerId)
      {
         this.playerId = playerId;
      }
   }

   [Serializable]
   public class GetS3ObjectRequest
   {
      public string objectKey;

      public GetS3ObjectRequest(string objectKey)
      {
         this.objectKey = objectKey;
      }
   }

   [Serializable]
   public class GetS3DownloadURLsRequest
   {
      public List<GetS3ObjectRequest> request;

      public GetS3DownloadURLsRequest(List<GetS3ObjectRequest> request)
      {
         this.request = request;
      }
   }

   [Serializable]
   public class ManifestResponse
   {
      public string id;
      public List<CloudSavingManifestEntry> manifest;
      public bool replacement = false;
   }

   [Serializable]
   public class CloudSavingManifestEntry
   {
      public string bucketName;
      public string key;
      public int size;
      public long lastModified;
      public string eTag;

      public CloudSavingManifestEntry(string bucketName, string key, int size, long lastModified, string eTag)
      {
         this.bucketName = bucketName;
         this.key = key;
         this.size = size;
         this.lastModified = lastModified;
         this.eTag = eTag;
      }
   }

   [Serializable]
   public class LocalCloudDataPath
   {
      public string root;
      public string prefix;
      public string dataPath;
      public string manifestPath;

      private const string _root = "beamable";
      private const string _cloudSavingDir = "cloudsaving";

      public LocalCloudDataPath(PlatformService platformService)
      {
         platformService.OnReady.Then(plat =>
         {
            root = Application.persistentDataPath;
            prefix = Path.Combine(
               _root,
               _cloudSavingDir,
               platformService.Cid,
               platformService.Pid,
               platformService.User.id.ToString()
            );
            dataPath = Path.Combine(root, prefix, "data");
            manifestPath = Path.Combine(root, prefix);
         });
      }
   }
}