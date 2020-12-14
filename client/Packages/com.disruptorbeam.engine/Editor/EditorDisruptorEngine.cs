using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Beamable.Api;
using Beamable.Api.Auth;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using Beamable.Editor.Content;
using Beamable.Editor.Environment;
using Beamable.Config;
using Beamable.Platform.SDK;
using Beamable.Platform.SDK.Auth;
using Beamable.Service;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.Editor
{
   public class EditorAPI
   {
      private static Promise<EditorAPI> _instance;
      public PlatformRequester Requester => _requester;
      public static Promise<EditorAPI> Instance
      {
         get
         {
            if (_instance == null)
            {
               var de = new EditorAPI();
               _instance = de.Initialize().Error(err =>
               {
                  Debug.LogError(err);
                  de.Logout();
                  _instance = null;
               });
            }

            return _instance;
         }
      }

      // Services
      private AccessTokenStorage _accessTokenStorage;
      private PlatformRequester _requester;
      public AuthService AuthService;
      public ContentIO ContentIO;
      public ContentPublisher ContentPublisher;
      public event Action<User> OnUserChange;

      // Info
      public string Cid
      {
         get => _requester.Cid;
         set => _requester.Cid = value;
      }

      public string Pid
      {
         get => _requester.Pid;
         set => _requester.Pid = value;
      }

      public string Host => _requester.Host;

      public User User;
      public AccessToken Token => _requester.Token;

      private Promise<EditorAPI> Initialize()
      {
         if (!Application.isPlaying)
         {
            PromiseExtensions.RegisterUncaughtPromiseHandler();
         }

         // Register services
         BeamableEnvironment.ReloadEnvironment();
         _accessTokenStorage = new AccessTokenStorage("editor.");
         _requester = new PlatformRequester(BeamableEnvironment.ApiUrl, _accessTokenStorage, null);
         AuthService = new AuthService(_requester);
         ContentIO = new ContentIO(_requester);
         ContentPublisher = new ContentPublisher(_requester, ContentIO);

         if (!ConfigDatabase.HasConfigFile(ConfigDatabase.GetConfigFileName()))
         {
            ApplyConfig("", "", BeamableEnvironment.ApiUrl);
            return Promise<EditorAPI>.Successful(this);
         }

         ConfigDatabase.Init();
         ApplyConfig(
            ConfigDatabase.GetString("cid"),
            ConfigDatabase.GetString("pid"),
            ConfigDatabase.GetString("platform")
         );

         return _accessTokenStorage.LoadTokenForRealm(Cid, Pid).FlatMap(token =>
         {
            if (token == null)
               return Promise<EditorAPI>.Successful(this);
            return InitializeWithToken(token).Error(err => { Logout(); });
         });
      }

      public void Logout()
      {
         _requester.DeleteToken();
         User = null;
         OnUserChange?.Invoke(null);
         BeamableEnvironment.ReloadEnvironment();
      }

      public bool HasDependencies()
      {
         var hasAddressables = null != AddressableAssetSettingsDefaultObject.GetSettings(false);
         var hasTextmeshPro = TextMeshProImporter.EssentialsLoaded;

         return hasAddressables && hasTextmeshPro;
      }

      public Promise<Unit> CreateDependencies()
      {
         // import addressables...
         AddressableAssetSettingsDefaultObject.GetSettings(true);

         return TextMeshProImporter.ImportEssentials().Then(_ =>
         {
            AssetDatabase.Refresh();
         });
      }

      public void SaveConfig(string cid, string pid, string host = null)
      {
         if (string.IsNullOrEmpty(host))
         {
            host = BeamableEnvironment.ApiUrl;
         }

         var config = new ConfigData()
         {
            cid = cid,
            pid = pid,
            platform = host,
            socket = host
         };
         var asJson = JsonUtility.ToJson(config, true);
         Directory.CreateDirectory("Assets/DisruptorEngine/Resources/");
         string path = "Assets/DisruptorEngine/Resources/config-defaults.txt";
         File.WriteAllText(path, asJson);
         AssetDatabase.Refresh();
         ApplyConfig(cid, pid, host);
      }

      public Promise<EditorAPI> ApplyToken(TokenResponse tokenResponse)
      {
         var token = new AccessToken(_accessTokenStorage, Cid, Pid, tokenResponse.access_token,
            tokenResponse.refresh_token, tokenResponse.expires_in);
         return token.Save().FlatMap(_ => InitializeWithToken(token));
      }

      public Promise<string> GetRealmSecret()
      {
         // TODO this will only work if the current user is an admin.

         return _requester.Request<CustomerResponse>(Method.GET, "/basic/realms/customer").Map(resp =>
         {
            var matchingProject = resp.customer.projects.FirstOrDefault(p => p.name.Equals(Pid));
            return matchingProject?.secret ?? "";
         });
      }

      private void ApplyConfig(string cid, string pid, string host)
      {
         Cid = cid;
         Pid = pid;
         _requester.Host = host;
      }


      private Promise<EditorAPI> InitializeWithToken(AccessToken token)
      {
         _requester.Token = token;
         // TODO: This call may fail because we're getting a customer scoped token now..
         return AuthService.GetUserForEditor().Map(user =>
         {
            User = user;
            OnUserChange?.Invoke(user);
            return this;
         });
      }


   }

   [System.Serializable]
   public class ConfigData
   {
      public string cid, pid, platform;
      public string socket;
   }

   [System.Serializable]
   public class CustomerResponse
   {
      public CustomerDTO customer;
   }

   [System.Serializable]
   public class CustomerDTO
   {
      public List<ProjectDTO> projects;
   }

   [System.Serializable]
   public class ProjectDTO
   {
      public string name;
      public string secret;
   }
}