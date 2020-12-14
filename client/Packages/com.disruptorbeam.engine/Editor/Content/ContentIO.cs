using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Content;
using Beamable.Content.Serialization;
using Beamable.Content.Validation;
using Beamable.Editor.Content.UI;
using Beamable.Platform.SDK;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace Beamable.Editor.Content
{
   public delegate void ContentSelectionEvent<in TContent>(TContent content) where TContent : ContentObject, new();

   public interface IContentIOEventGroup<out TContent> where TContent : ContentObject, new()
   {
   }

   public class ContentIOEventGroup<TContent> : IContentIOEventGroup<TContent> where TContent : ContentObject, new()
   {
      public event ContentSelectionEvent<TContent> OnCreated = (c) => { };
      public event ContentSelectionEvent<TContent> OnDeleted = (c) => { };

      public void NotifyCreation(TContent content)
      {
         OnCreated?.Invoke(content);
      }

      public void NotifyDeletion(TContent content)
      {
         OnDeleted?.Invoke(content);
      }

   }

   public class ContentIOAssetProcessor : UnityEditor.AssetModificationProcessor
   {
      private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
      {
         if (AssetDatabase.IsValidFolder(assetPath))
         {
            var subAssets = AssetDatabase.FindAssets("", new string[] { assetPath });
            foreach (var sub in subAssets)
            {
               var subPath = AssetDatabase.GUIDToAssetPath(sub);
               OnWillDeleteAsset(subPath, options);
            }
            return AssetDeleteResult.DidNotDelete;

         }

         var asset = AssetDatabase.LoadAssetAtPath<ContentObject>(assetPath);
         if (asset == null) return AssetDeleteResult.DidNotDelete;

         var method = typeof(ContentIO).GetMethod(nameof(ContentIO.NotifyDeleted), BindingFlags.NonPublic | BindingFlags.Static);
         var genMethod = method.MakeGenericMethod(asset.GetType());
         genMethod.Invoke(null, new object[] { asset });

         return AssetDeleteResult.DidNotDelete;
      }


      private static string[] OnWillSaveAssets(string[] paths)
      {
         foreach (var path in paths)
         {
            var asset = AssetDatabase.LoadAssetAtPath<ContentObject>(path);
            if (asset == null) continue;

            var method = typeof(ContentIO).GetMethod(nameof(ContentIO.NotifyCreated), BindingFlags.NonPublic | BindingFlags.Static);
            var genMethod = method.MakeGenericMethod(asset.GetType());

            var rootPath = $"{ContentConstants.DATA_DIR}/{asset.ContentType}/";
            var subPath = path.Substring(rootPath.Length);
            var qualifiedName = Path.GetFileNameWithoutExtension(subPath.Replace(Path.DirectorySeparatorChar, '.'));

            asset.SetContentName(qualifiedName);

            genMethod.Invoke(null, new object[] { asset });
         }

         return paths;
      }
   }

   public interface IContentIO
   {
      Promise<Manifest> FetchManifest();
      IEnumerable<ContentObject> FindAll(ContentQuery query=null);
      string Checksum(IContentObject content);
   }

   /// <summary>
   /// The purpose of this class is to
   /// 1. scrape local editor directory for content assets
   /// 2. handle the upload of the assets to Platform
   /// 3. create new editor-side-not-yet-deployed content
   /// </summary>
   public class ContentIO : IContentIO
   {
      private readonly IBeamableRequester _requester;
      private Promise<Manifest> _manifestPromise;
      private ContentObject _lastSelected;
      private static readonly Dictionary<Type, IContentIOEventGroup<ContentObject>> ContentEventGroups = new Dictionary<Type, IContentIOEventGroup<ContentObject>>();

      public Promise<Manifest> OnManifest => _manifestPromise ?? FetchManifest();

      public event ContentDelegate OnSelectionChanged;

      public static event IContentDelegate OnContentCreated, OnContentDeleted;
      public static event IContentRenamedDelegate OnContentRenamed;

      private ValidationContext ValidationContext { get; } = new ValidationContext { AllContentIds = new HashSet<string>()};

      public ContentIO(IBeamableRequester requester)
      {
         _requester = requester;
         Selection.selectionChanged += SelectionChanged;

         OnContentCreated += Internal_HandleContentCreated;
         OnContentDeleted += Internal_HandleContentDeleted;
      }

      private void Internal_HandleContentCreated(IContentObject content)
      {
         ValidationContext.AllContentIds.Add(content.Id);
      }
      private void Internal_HandleContentDeleted(IContentObject content)
      {
         ValidationContext.AllContentIds.Remove(content.Id);
      }


      private void SelectionChanged()
      {
         var activeContent = Selection.activeObject as ContentObject;

         if (_lastSelected != null && _lastSelected != activeContent)
         {
            // selection has been lost! Save and broadcast an update!
            _lastSelected.BroadcastUpdate();
         }

         if (_lastSelected != activeContent && activeContent != null)
         {
            OnSelectionChanged?.Invoke(activeContent);
         }

         _lastSelected = activeContent;
      }

      public IValidationContext GetValidationContext()
      {
         return ValidationContext;
      }

      public Promise<Manifest> FetchManifest()
      {
         var manifestUrl = "/basic/content/manifest";

         _manifestPromise = new Promise<Manifest>();
         var webRequest = _requester.Request<ContentManifest>(Method.GET, manifestUrl, useCache: true);
         webRequest.Error(error =>
         {
            // Nullref check added for offline stability - srivello
            if (error is PlatformRequesterException err && err?.Error?.status == 404)
            {
               // create a blank in-memory copy of the manifest for usage now. This is the same as assuming no manifest.
               _manifestPromise.CompleteSuccess(new Manifest(new List<ContentManifestReference>()));
            }
            else
            {
               _manifestPromise.CompleteError(error);
            }
         }).Then(source =>
         {
            _manifestPromise.CompleteSuccess(new Manifest(source));
         });

         return _manifestPromise;
      }

      public Promise<ContentStatus> GetStatus(ContentObject content)
      {
         return OnManifest.Map(manifest =>
         {
            var data = manifest.Get(content.Id);
            if (data == null)
            {
               return ContentStatus.NEW;
            }

            var checksumsMatch = data.checksum.Equals(Checksum(content));
            if (checksumsMatch && data.tags.SequenceEqual(content.Tags))
            {
               return ContentStatus.CURRENT;
            }

            return ContentStatus.MODIFIED;
         });
      }

      public void Select(ContentObject content)
      {
         var actual = Find(content);
         Selection.SetActiveObjectWithContext(actual, actual);
      }

      private ContentObject Find(ContentObject content)
      {
         var assetGuids = AssetDatabase.FindAssets($"t:{content.GetType().Name}", new[] { ContentConstants.DATA_DIR });
         foreach (var guid in assetGuids)
         {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ContentObject asset = (ContentObject)AssetDatabase.LoadAssetAtPath(assetPath,content.GetType());
            if (asset.Id == content.Id)
               return asset;
         }
         throw new Exception("No matching content.");
      }
      public IEnumerable<TContent> FindAllContent<TContent>(ContentQuery query=null, bool inherit = true) where TContent : ContentObject, new()
      {
         if (query == null) query = ContentQuery.Unit;

         var assetGuids = AssetDatabase.FindAssets($"t:{typeof(TContent).Name}", new[] { ContentConstants.DATA_DIR });
         var contentType = ContentRegistry.TypeToName(typeof(TContent));

         foreach (var guid in assetGuids)
         {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<TContent>(path);

            if (!query.Accept(asset))
               continue;
            if (asset == null || (!inherit && asset.ContentType != contentType))
               continue;


            var name = Path.GetFileNameWithoutExtension(path);
            asset.SetContentName(name);
            yield return asset;
         }
      }


      public string[] FindAllDirectoriesForSubtypes<TContent>() where TContent : ContentObject, new()
      {
         return FindAllSubtypes<TContent>().Select(contentType =>
         {
            string contentTypeName = ContentObject.GetContentTypeName(contentType);
            var dir = $"{ContentConstants.DATA_DIR}/{contentTypeName}";
            return dir;
         }).ToArray();
      }

      public Type[] FindAllSubtypes<TContent>() where TContent : ContentObject, new()
      {
         return GetContentTypes().Where(contentType =>
         {
            return typeof(TContent).IsAssignableFrom(contentType);
         }).ToArray();
      }

      public IEnumerable<ContentObject> FindAll(ContentQuery query=null)
      {
         var contentArray = new List<ContentObject>();
         var contentIdHashSet = new HashSet<string>();
         foreach (var contentType in GetContentTypes())
         {
            foreach (var content in FindAllContentByType(contentType, query))
            {
               if (!contentIdHashSet.Contains(content.Id))
               {
                  contentIdHashSet.Add(content.Id);
                  contentArray.Add(content);
               }
            }
         }
         return contentArray;
      }

      public LocalContentManifest BuildLocalManifest()
      {
         var localManifest = new LocalContentManifest();
         ValidationContext.AllContentIds.Clear();
         foreach (var contentType in ContentRegistry.GetContentTypes()) // TODO check heirarchy types.
         {
            var assetGuids = AssetDatabase.FindAssets($"t:{contentType.Name}", new []{ContentConstants.DATA_DIR});

            foreach (var assetGuid in assetGuids)
            {
               var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
               var rawAsset =  AssetDatabase.LoadAssetAtPath(assetPath, typeof(IContentObject));
               var content = rawAsset as IContentObject;

               if (content == null || rawAsset.GetType() != contentType) continue;

               var entry = new LocalContentManifestEntry
               {
                  ContentType = contentType,
                  Content = content,
                  AssetPath = assetPath
               };
               ValidationContext.AllContentIds.Add(content.Id);
               if (!localManifest.Content.ContainsKey(content.Id))
               {
                  localManifest.Content.Add(content.Id, entry);
               }
            }
         }

         ContentObject.ValidationContext = ValidationContext;
         return localManifest;
      }

      public IEnumerable<Type> GetContentTypes()
      {
         return ContentRegistry.GetContentTypes();
      }

      public IEnumerable<string> GetContentClassIds()
      {
         return ContentRegistry.GetContentClassIds();
      }

      public ContentObject LoadContent(LocalContentManifestEntry manifestEntry)
      {
         return AssetDatabase.LoadAssetAtPath<ContentObject>(manifestEntry.AssetPath);
      }
      public ContentObject LoadContent(string assetPath)
      {
         return AssetDatabase.LoadAssetAtPath<ContentObject>(assetPath);
      }


      public IEnumerable<ContentObject> FindAllContentByType(Type type, ContentQuery query=null, bool inherit = true)
      {
         var methodName = nameof(FindAllContent);
         var method = typeof(ContentIO).GetMethod(methodName).MakeGenericMethod(type);
         var content = method.Invoke(this,new object[] { query, inherit }) as IEnumerable<ContentObject>;
         return content;
      }

      public void EnsureAllDefaultContent()
      {
         foreach (var contentType in GetContentTypes())
         {
            EnsureDefaultContentByType(contentType);
         }
      }

      public void EnsureDefaultContentByType(Type type)
      {
         var methodName = nameof(EnsureDefaultContent);
         var method = typeof(ContentIO).GetMethod(methodName).MakeGenericMethod(type);
         method.Invoke(this, null);
      }

      public void EnsureDefaultContent<TContent>() where TContent : ContentObject
      {
         string typeName = ContentObject.GetContentType<TContent>();
         var dir = $"{ContentConstants.DATA_DIR}/{typeName}";
         EnsureDefaultAssets<TContent>();
         if (!Directory.Exists(dir))
         {
            Directory.CreateDirectory(dir);
            var defaultDir = $"{ContentConstants.DEFAULT_DATA_DIR}/{typeName}";
            if (Directory.Exists(defaultDir))
            {
               string[] files = Directory.GetFiles(defaultDir);
               foreach (var src in files)
               {
                  if (!Path.GetExtension(src).Equals(".asset"))
                     continue;

                  var filename = Path.GetFileName(src);
                  var dest = Path.Combine(dir, filename);
                  File.Copy(src, dest, true);

               }
               AssetDatabase.ImportAsset(dir, ImportAssetOptions.ImportRecursive);
            }
         }
      }

      public string GetAssetPathByType(Type contentType, IContentObject content)
      {
         foreach (var nextContentType in ContentRegistry.GetContentTypes()) // TODO check heirarchy types.
         {
            var assetGuids = AssetDatabase.FindAssets($"t:{nextContentType.Name}", new[] { ContentConstants.DATA_DIR });

            foreach (var assetGuid in assetGuids)
            {
               var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
               var rawAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(IContentObject));
               var nextContent = rawAsset as IContentObject;

               if (nextContent == null || rawAsset.GetType() != nextContentType) continue;

               if (nextContentType == contentType &&
                  nextContent == content)
               {
                  return assetPath;
               }
            }
         }
         return "";
      }

      public void EnsureDefaultAssetsByType(Type type)
      {
         var methodName = nameof(EnsureDefaultAssets);
         var method = typeof(ContentIO).GetMethod(methodName).MakeGenericMethod(type);
         method.Invoke(this, null);
      }

      public void EnsureDefaultAssets<TContent>() where TContent : ContentObject
      {
         string contentType = ContentObject.GetContentType<TContent>();
         var assetDir = $"{ContentConstants.ASSET_DIR}/{contentType}";
         var defaultDir = $"{ContentConstants.DEFAULT_ASSET_DIR}/{contentType}";
         if (Directory.Exists(assetDir) || !Directory.Exists(defaultDir))
         {
            return;
         }

         Directory.CreateDirectory(assetDir);
         string[] files = Directory.GetFiles(defaultDir);
         var addedEntries = new List<AddressableAssetEntry>();

         var addressableAssetSettings = AddressableAssetSettingsDefaultObject.GetSettings(false);
         if (addressableAssetSettings == null)
         {
            throw new Exception("Addressables was not configured");
         }

         AssetDatabase.Refresh();

         var filesToMarkAddressable = new List<string>();

         foreach (var src in files)
         {
            var filename = Path.GetFileName(src);
            var dest = Path.Combine(assetDir, filename);
            File.Copy(src, dest, true);

            if (src.EndsWith("meta"))
               // we don't need to mark a meta file as addressable... silly...
               continue;
            filesToMarkAddressable.Add(dest);
         }

         // mark all files as addressable after copy completes...
         CommitAssetDatabase();
         foreach (var file in filesToMarkAddressable)
         {
            var guid = AssetDatabase.AssetPathToGUID(file);
            var entry = addressableAssetSettings.CreateOrMoveEntry(guid, addressableAssetSettings.DefaultGroup); // TODO make we make a DEDefaultContentGroup ?
            addedEntries.Add(entry);
         }

         addressableAssetSettings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, addedEntries, true);
         CommitAssetDatabase();
      }

      public void Create<TContent>(TContent content, string assetPath=null)
         where TContent : ContentObject, new()
      {
         if (string.IsNullOrEmpty(assetPath))
         {
            var newNameAsPath = content.Id.Replace('.', Path.DirectorySeparatorChar);
            assetPath = $"{ContentConstants.DATA_DIR}/{newNameAsPath}.asset";
         }
         var directory = Path.GetDirectoryName(assetPath);
         Directory.CreateDirectory(directory);
         AssetDatabase.CreateAsset(content, assetPath);

         NotifyCreated(content);
      }

      /// <summary>
      /// Delete a <see cref="TContent"/> item of type <see cref="Type"/>
      /// </summary>
      /// <param name="type"></param>
      /// <param name="content"></param>
      public void DeleteByType(Type type, IContentObject content)
      {
         var methodName = nameof(Delete);
         var method = typeof(ContentIO).GetMethod(methodName).MakeGenericMethod(type);
         method.Invoke(this, new object[] { content } );
      }

      /// <summary>
      /// Delete a <see cref="TContent"/> item of type <see cref="TContent"/>
      /// </summary>
      /// <typeparam name="TContent"></typeparam>
      /// <param name="content"></param>
      public void Delete<TContent>(TContent content)
         where TContent : ContentObject, new()
      {
         var contentName = content.ContentName;
         var contentSystemType = content.GetType();

         var path = AssetDatabase.GetAssetPath(content);
         if (string.IsNullOrEmpty(path))
         {
            var newNameAsPath = contentName.Replace('.', Path.DirectorySeparatorChar);
            path = $"{ContentConstants.DATA_DIR}/{content.ContentType}/{newNameAsPath}.asset";
         }
         NotifyDeleted(content);

         AssetDatabase.DeleteAsset(path);
         File.Delete(path);
      }


      struct ValidationChecksum
      {
         public Guid ValidationId;
         public string Checksum;
      }

      private static Dictionary<string, ValidationChecksum> _checksumTable = new Dictionary<string, ValidationChecksum>();
      private static Dictionary<Guid, string> _validationIdToChecksumTable = new Dictionary<Guid, string>();
      public static string ComputeChecksum(IContentObject content)
      {

         if (content is ContentObject contentObj && contentObj && _checksumTable.TryGetValue(contentObj.Id, out var existing) && existing.ValidationId == contentObj.ValidationGuid)
         {
            return existing.Checksum;
         }

         using (var md5 = MD5.Create())
         {
            var json = ClientContentSerializer.SerializeProperties(content);
            var bytes = Encoding.ASCII.GetBytes(json);
            var hash = md5.ComputeHash(bytes);
            var checksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            if (content is ContentObject contentObj2 && contentObj2)
            {
               _checksumTable[contentObj2.Id] = new ValidationChecksum
               {
                  ValidationId = contentObj2.ValidationGuid,
                  Checksum = checksum
               };
            }

            return checksum;
         }
      }

      public string Checksum(IContentObject content)
      {
         return ComputeChecksum(content);
      }
      public string Serialize<TContent>(TContent content)
         where TContent : ContentObject, new()
      {
         return ClientContentSerializer.SerializeContent(content);

      }


      public void Rename(string existingAssetPath, string nextAssetpath, ContentObject content)
      {
         var newDirectory = Path.GetDirectoryName(existingAssetPath);
         Directory.CreateDirectory(newDirectory);
         var nextName = Path.GetFileNameWithoutExtension(nextAssetpath);

         var oldId = content.Id;
         content.name = nextName;
         content.SetContentName(nextName);
         NotifyRenamed(oldId, content, nextAssetpath);
         content.BroadcastUpdate();
         var result = AssetDatabase.MoveAsset(existingAssetPath, nextAssetpath);
         if (!string.IsNullOrEmpty(result))
         {
            throw new Exception(result);
         }

         EditorUtility.SetDirty(content);
         AssetDatabase.ForceReserializeAssets(new[] { nextAssetpath },
            ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
      }
//
//      /// <summary>
//      /// Rename a <see cref="IContentObject"/> instance of type <see cref="Type"/>
//      /// </summary>
//      /// <param name="type"></param>
//      /// <param name="content"></param>
//      /// <param name="newName"></param>
//      public void RenameByType(Type type, IContentObject content, string newName)
//      {
//         var methodName = nameof(Rename);
//         var method = typeof(ContentIO).GetMethod(methodName).MakeGenericMethod(type);
//         method.Invoke(this, new object[] { content, newName });
//      }

      /// <summary>
      /// Rename a <see cref="TContent"/> instance of type <see cref="TContent"/>
      /// </summary>
      /// <typeparam name="TContent"></typeparam>
      /// <param name="content"></param>
      /// <param name="newName"></param>
      public void Rename<TContent>(TContent content, string newName)
         where TContent : ContentObject, new()
      {
         var existingName = content.ContentName;
         var typeName = content.ContentType;


         var existingNameAsPath = existingName.Replace('.', Path.DirectorySeparatorChar);
         var existingPath = $"{ContentConstants.DATA_DIR}/{typeName}/{existingNameAsPath}.asset";
         var existingDirectory = Path.GetDirectoryName(existingPath);

         var newNameAsPath = newName.Replace('.', Path.DirectorySeparatorChar);
         var newPath = $"{ContentConstants.DATA_DIR}/{typeName}/{newNameAsPath}.asset";
         var newDirectory = Path.GetDirectoryName(newPath);

         if (existingPath == newPath)
         {
            return;
         }


         AssetDatabase.ForceReserializeAssets(new[] { newDirectory, existingDirectory },
            ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);

         var fileExists = File.Exists(existingPath);
         if (!fileExists)
         {
            throw new NotImplementedException();
         }

         Directory.CreateDirectory(newDirectory);
         AssetDatabase.Refresh();
         AssetDatabase.ForceReserializeAssets(new[] { newDirectory },
            ForceReserializeAssetsOptions.ReserializeMetadata);

         var oldId = content.Id;
         content.name = newName;
         content.SetContentName(newName);
         NotifyRenamed(oldId, content, newPath);
         content.BroadcastUpdate();

         var result = AssetDatabase.MoveAsset(existingPath, newPath);
         if (!string.IsNullOrEmpty(result))
         {
            throw new Exception(result);
         }

         EditorUtility.SetDirty(content);
         AssetDatabase.ForceReserializeAssets(new[] { newPath },
            ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);

      }


      public static ContentIOEventGroup<TContent> GetEventGroup<TContent>() where TContent : ContentObject, new()
      {
         var type = typeof(TContent);
         if (!ContentEventGroups.ContainsKey(type))
         {
            var group = new ContentIOEventGroup<TContent>();
            ContentEventGroups.Add(type, group);
         }

         var output = ContentEventGroups[type];
         var specialOutput = output as ContentIOEventGroup<TContent>;
         return specialOutput;
      }

      private void CommitAssetDatabase()
      {
         // TODO: Revisit this code. It was written this way, because that was the way I could make sure the assets always received the changes
         AssetDatabase.Refresh();
         AssetDatabase.SaveAssets();
         AssetDatabase.Refresh();
      }


      internal static void NotifyRenamed<TContent>(string oldId, TContent content, string nextAssetPath)
         where TContent : ContentObject, new()
      {
         OnContentRenamed?.Invoke(oldId, content, nextAssetPath);
      }
      internal static void NotifyCreated<TContent>(TContent content)
         where TContent : ContentObject, new()
      {
         OnContentCreated?.Invoke(content);

         GetEventGroup<TContent>().NotifyCreation(content);
      }
      internal static void NotifyDeleted<TContent>(TContent content)
         where TContent : ContentObject, new()
      {
         OnContentDeleted?.Invoke(content);
         GetEventGroup<TContent>().NotifyDeletion(content);
      }

   }
}