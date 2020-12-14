using System;
using System.Text;
using Beamable.Api.Caches;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using Beamable.Api.Auth;
using Beamable.Api.Connectivity;
using Beamable.Common.Pooling;
using Beamable.Serialization;
using Beamable.Spew;
using UnityEngine;
using UnityEngine.Networking;

namespace Beamable.Api
{

   public class PlatformRequester : IBeamableRequester
   {
      private const string ACCEPT_HEADER = "application/json";
      private AccessTokenStorage _accessTokenStorage;
      private ConnectivityService _connectivityService;
      private bool _disposed;
      private bool internetConnectivity;
      public string Host { get; set; }
      public string Cid { get; set; }
      public string Pid { get; set; }

      public IAccessToken AccessToken => Token;
      public AccessToken Token { get; set; }
      public string Shard { get; set; }
      public string Language { get; set; }
      public string TimeOverride { get; set; }
      public IAuthApi AuthService { private get; set; }

      public PlatformRequester(string host, AccessTokenStorage accessTokenStorage, ConnectivityService connectivityService)
      {
         Host = host;
         _accessTokenStorage = accessTokenStorage;
         _connectivityService = connectivityService;
      }

      public IBeamableRequester WithAccessToken(TokenResponse token)
      {
         var requester = new PlatformRequester(Host, _accessTokenStorage, _connectivityService);
         requester.Cid = Cid;
         requester.Pid = Pid;
         requester.Shard = Shard;
         requester.Language = Language;
         requester.TimeOverride = TimeOverride;
         requester.AuthService = AuthService;
         requester.Token = new AccessToken(_accessTokenStorage, Cid, Pid, token.access_token, token.refresh_token, token.expires_in);
         return requester;
      }

      public string EscapeURL(string url)
      {
         return UnityWebRequest.EscapeURL(url);
      }

      public void DeleteToken()
      {
         Token?.Delete();
         Token = null;
      }

      public void Dispose()
      {
         _disposed = true;
      }

      public UnityWebRequest BuildWebRequest(Method method, string uri, string contentType, byte[] body)
      {
         var address = uri.Contains("://") ? uri : $"{Host}{uri}";

         // Prepare the request
         var request = new UnityWebRequest(address)
         {
            downloadHandler = new DownloadHandlerBuffer(), method = method.ToString()
         };

         // Set the body
         if (body != null)
         {
            var upload = new UploadHandlerRaw(body) {contentType = contentType};
            request.uploadHandler = upload;
         }

         return request;
      }

      public Promise<T> RequestForm<T>(string uri, WWWForm form, bool includeAuthHeader = true)
      {
         return RequestForm<T>(uri, form, Method.POST, includeAuthHeader);
      }

      public Promise<T> RequestForm<T>(string uri, WWWForm form, Method method, bool includeAuthHeader = true)
      {
         return MakeRequestWithTokenRefresh<T>(method, uri, "application/x-www-form-urlencoded", form.data,
            includeAuthHeader);
      }

      public Promise<T> Request<T>(Method method, string uri, object body = null, bool includeAuthHeader = true, Func<string, T> parser=null, bool useCache=false)
      {
         string contentType = null;
         byte[] bodyBytes = null;

         if (body != null)
         {
            bodyBytes = body is string json ? Encoding.UTF8.GetBytes(json) : Encoding.UTF8.GetBytes(JsonUtility.ToJson(body));
            contentType = "application/json";
         }

         return MakeRequestWithTokenRefresh<T>(method, uri, contentType, bodyBytes, includeAuthHeader, parser, useCache);
      }

      public Promise<T> RequestJson<T>(Method method, string uri, JsonSerializable.ISerializable body,
         bool includeAuthHeader = true)
      {
         const string contentType = "application/json";
         var jsonFields = JsonSerializable.Serialize(body);

         using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
         {
            var json = Serialization.SmallerJSON.Json.Serialize(jsonFields, pooledBuilder.Builder);
            var bodyBytes = Encoding.UTF8.GetBytes(json);
            return MakeRequestWithTokenRefresh<T>(method, uri, contentType, bodyBytes, includeAuthHeader);
         }
      }

      private Promise<T> MakeRequestWithTokenRefresh<T>(
         Method method,
         string uri,
         string contentType,
         byte[] body,
         bool includeAuthHeader,
         Func<string, T> parser=null,
         bool useCache=false)
      {
        internetConnectivity = _connectivityService != null ? _connectivityService.HasConnectivity : true;

        if (internetConnectivity)
        {
           return MakeRequest<T>(method, uri, contentType, body, includeAuthHeader, parser)
              .RecoverWith(error =>
              {
                 var httpNoInternet = error is NoConnectivityException ||
                                      error is PlatformRequesterException noInternet && noInternet.Status == 0;

                 if (httpNoInternet)
                 {
                    _connectivityService?.SetHasInternet(false);
                 }

                 if (useCache && httpNoInternet && Application.isPlaying)
                 {
                    return OfflineCache.Get<T>(uri, Token);
                 }

                 // if we get a 401 InvalidTokenError, let's refresh the token and retry the request.
                 if (error is PlatformRequesterException code && code?.Error?.error == "InvalidTokenError")
                 {
                    return AuthService.LoginRefreshToken(Token.RefreshToken)
                       .Map(rsp =>
                       {
                          Token = new AccessToken(_accessTokenStorage, Cid, Pid, rsp.access_token, rsp.refresh_token,
                             rsp.expires_in);
                          Token.Save();
                          return PromiseBase.Unit;
                       })
                       .FlatMap(_ => MakeRequest(method, uri, contentType, body, includeAuthHeader, parser));
                 }

                 return Promise<T>.Failed(error);
                 //The uri + Token.RefreshToken.ToString() wont work properly for anything with a body in the request
              }).Then(_response =>
              {
                 if (useCache && Token != null && Application.isPlaying)
                 {
                    OfflineCache.Set<T>(uri, _response, Token);
                 }
              });
        }
        else if (!internetConnectivity && useCache && Application.isPlaying)
        {
            return OfflineCache.Get<T>(uri, Token);
        }
        else
        {
           return Promise<T>.Failed(new NoConnectivityException(uri + " should not be cached and requires internet connectivity."));
        }
      }



      private Promise<T> MakeRequest<T>(
         Method method,
         string uri,
         string contentType,
         byte[] body,
         bool includeAuthHeader,
         Func<string, T> parser=null)
      {
         var result = new Promise<T>();
         var request = BuildWebRequest(method, uri, contentType, body, includeAuthHeader);
         var op = request.SendWebRequest();
         op.completed += _ => HandleResponse<T>(result, request, parser);
         return result;
      }

      private UnityWebRequest BuildWebRequest(Method method, string uri, string contentType, byte[] body,
         bool includeAuthHeader)
      {
         PlatformLogger.Log($"PLATFORM REQUEST: {Host}{uri}");

         // Prepare the request
         UnityWebRequest request = BuildWebRequest(method, uri, contentType, body);
         request.SetRequestHeader("Accept", ACCEPT_HEADER);
         if (Cid != "")
         {
            request.SetRequestHeader("X-KS-CLIENTID", Cid);
            request.SetRequestHeader("X-KS-PROJECTID", Pid);
         }

         if (includeAuthHeader)
         {
            var authHeader = GenerateAuthorizationHeader();
            if (authHeader != null)
            {
               request.SetRequestHeader("Authorization", authHeader);
            }
         }

         if (Shard != null)
         {
            request.SetRequestHeader("X-KS-SHARD", Shard);
         }

         if (TimeOverride != null)
         {
            request.SetRequestHeader("X-KS-TIME", TimeOverride);
         }

         if (Language != null)
         {
            request.SetRequestHeader("Accept-Language", Language);
         }

         return request;
      }

      private void HandleResponse<T>(Promise<T> promise, UnityWebRequest request, Func<string, T> parser=null)
      {
         // swallow any responses if already disposed
         if (_disposed)
         {
            PlatformLogger.Log("PLATFORM REQUESTER: Disposed, Ignoring Response");
            return;
         }

         if (request.responseCode >= 300 || request.isNetworkError)
         {
            // Handle errors
            var payload = request.downloadHandler.text;

            PlatformError platformError = null;
            try
            {
               platformError = JsonUtility.FromJson<PlatformError>(payload);
            }
            catch (Exception)
            {
               // Swallow the exception and let the error be null
            }

            promise.CompleteError(new PlatformRequesterException(platformError, request));

         }
         else
         {
            // Parse JSON object and resolve promise
            PlatformLogger.Log($"PLATFORM RESPONSE: {request.downloadHandler.text}");

            try
            {
               T result = parser == null ?
                  JsonUtility.FromJson<T>(request.downloadHandler.text) :
                  parser(request.downloadHandler.text);
               promise.CompleteSuccess(result);
            }
            catch (Exception ex)
            {
               promise.CompleteError(ex);
            }
         }
      }

      private string GenerateAuthorizationHeader()
      {
         return Token != null ? $"Bearer {Token.Token}" : null;
      }
   }

   [Serializable]
   public class PlatformError
   {
      public long status;
      public string service;
      public string error;
      public string message;
   }

   public class PlatformRequesterException : Exception, IRequestErrorWithStatus
   {
      public PlatformError Error { get; }
      public UnityWebRequest Request { get; }
      public long Status => Request.responseCode;
      public PlatformRequesterException(PlatformError error, UnityWebRequest request)
      : base(GenerateMessage(request))
      {
         Error = error;
         Request = request;
      }
      static string GenerateMessage(UnityWebRequest request)
      {
         return $"HTTP Error. method=[{request.method}] uri=[{request.uri}] code=[{request.responseCode}] payload=[{request.downloadHandler.text}]";
      }
   }
    public class NoConnectivityException : Exception
    {
        public NoConnectivityException(string message) : base(message) { }
    }
}