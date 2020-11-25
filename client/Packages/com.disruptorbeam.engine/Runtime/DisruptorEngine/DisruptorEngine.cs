using System;
using System.Collections;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Content;
using Beamable.Config;
using Beamable.Coroutines;
using Beamable.Platform.SDK;
using Beamable.Platform.SDK.Announcements;
using Beamable.Platform.SDK.Auth;
using Beamable.Platform.SDK.Chat;
using Beamable.Platform.SDK.Commerce;
using Beamable.Platform.SDK.Inventory;
using Beamable.Platform.SDK.Leaderboard;
using Beamable.Platform.SDK.Matchmaking;
using Beamable.Platform.SDK.Notification;
using Beamable.Platform.SDK.Payments;
using Beamable.Platform.SDK.Sim;
using Beamable.Platform.SDK.Stats;
using Beamable.Platform.SDK.CloudSaving;
using Beamable.Service;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Beamable.Platform.SDK.Caches;
using Beamable.Platform.SDK.Calendars;
using Beamable.Platform.SDK.Tournaments;
using Beamable.Modules.Tournaments;

namespace Beamable
{
   public interface IBeamableAPI
   {
      User User { get; }
      AccessToken Token { get; }

      event Action<User> OnUserChanged;

      AnnouncementsService AnnouncementService { get; }
      IAuthService AuthService { get; }
      CalendarsService CalendarsService { get; }
      ChatService ChatService { get; }
      CloudSavingService CloudSavingService { get; }
      ContentService ContentService { get; }
      GameRelayService GameRelayService { get; }
      InventoryService InventoryService { get; }
      LeaderboardService LeaderboardService { get;  }
      PlatformRequester Requester { get; }
      StatsService Stats { get; }
      CommerceService Commerce { get; }
      MatchmakingService Matchmaking { get;  }
      Promise<PaymentDelegate> PaymentDelegate { get;  }
      ConnectivityService ConnectivityService { get;  }
      ITournamentService Tournaments { get; }

      void UpdateUserData(User user);
      Promise<ISet<UserBundle>> GetDeviceUsers();
      void RemoveDeviceUser(TokenResponse token);
      Promise<Unit> ApplyToken(TokenResponse response);
   }

   public class API : IBeamableAPI
   {

      private static Promise<IBeamableAPI> _instance;

      public static Promise<IBeamableAPI> Instance
      {
         get
         {
            if (_instance != null)
            {
               return _instance;
            }

            _instance = new API().Initialize();
            return _instance;
         }

         // SHOULD ONLY BE USED BY LOCAL TEST CODE.
         #if UNITY_EDITOR
         set => _instance = value;
         #endif
      }

      private PlatformService _platform;
      private GameObject _gameObject;

      public AnnouncementsService AnnouncementService => _platform.Announcements;
      public ContentService ContentService { get; private set; }
      public GameRelayService GameRelayService => _platform.GameRelay;
      public InventoryService InventoryService => _platform.Inventory;
      public LeaderboardService LeaderboardService => _platform.Leaderboard;
      public IAuthService AuthService => _platform.Auth;
      public StatsService Stats => _platform.Stats;
      public CalendarsService CalendarsService => _platform.Calendars;
      public ChatService ChatService => _platform.Chat;

      public ITournamentService Tournaments => _platform.Tournaments;

      public CloudSavingService CloudSavingService => _platform.CloudSaving;
      public CommerceService Commerce => _platform.Commerce;
      public PlatformRequester Requester => _platform.Requester;
      public NotificationService NotificationService => _platform.Notification;
      public MatchmakingService Matchmaking => _platform.Matchmaking;
      public Promise<PaymentDelegate> PaymentDelegate => _platform.InitializedPaymentDelegate;
      public User User => _platform.User;
      public AccessToken Token => _platform.AccessToken;
      public ConnectivityService ConnectivityService => _platform.ConnectivityService;

      public event Action<User> OnUserChanged;

      private Promise<IBeamableAPI> Initialize()
      {
         if (Application.isPlaying)
         {
            PromiseExtensions.RegisterUncaughtPromiseHandler();
         }

         // Build default game object
         _gameObject = new GameObject("DisruptorEngine");
         Object.DontDestroyOnLoad(_gameObject);

        // Initialize platform
        ConfigDatabase.Init();
        //Flush cache that wasn't created with this version of the game.
        OfflineCache.FlushInvalidCache();
        // Register services
        ServiceManager.DisableEditorResolvers();
        var coroutineService = MonoBehaviourServiceContainer<CoroutineService>.CreateComponent(_gameObject);

        ServiceManager.Provide(coroutineService);
        ServiceManager.ProvideWithDefaultContainer(new ConnectivityService(coroutineService.Resolve()));
        _platform = new PlatformService(debugMode: false, withLocalNote: false);
        ServiceManager.ProvideWithDefaultContainer(_platform);
        ServiceManager.Provide(new DisruptorEngineResolver(this));
        ContentService = new ContentService(_platform, _platform.Requester);
        OnUserChanged += CloudSavingService.Init;
         return ServiceManager.Resolve<PlatformService>().Initialize("en").Map(_ => this as IBeamableAPI);
      }

      IEnumerator DeferPromise(Action action)
      {
         yield return Yielders.EndOfFrame;
         action();
      }

      public Promise<Unit> ApplyToken (TokenResponse tokenResponse)
      {
         return _platform.SaveToken(tokenResponse)
            .FlatMap(_ => _platform.ReloadUser())
            .FlatMap(user =>_platform.StartNewSession().Map(_ => user))
            .Then(user => OnUserChanged?.Invoke(user))
            .Map(_ => PromiseBase.Unit);
      }

      public Promise<ISet<UserBundle>> GetDeviceUsers()
      {
         return _platform.GetDeviceUsers();
      }

      public void RemoveDeviceUser(TokenResponse token)
      {
         _platform.RemoveDeviceUsers(token);
      }

      public void UpdateUserData(User user)
      {
         _platform.SetUser(user);
         OnUserChanged?.Invoke(user);
      }

      public void TearDown()
      {
         var monoBehaviour = _gameObject.AddComponent<CoroutineBehaviour>();
         monoBehaviour.StartCoroutine(TearDownCoroutine());
      }

      private IEnumerator TearDownCoroutine()
      {
         // Create new empty scene so existing scenes can be unloaded and leave this one standing
         SceneManager.CreateScene(Guid.NewGuid().ToString());

         // Shut down all scenes
         int count = SceneManager.sceneCount;
         var ops = new List<AsyncOperation>();
         for (int i = 0; i < count; i++)
         {
            var scene = SceneManager.GetSceneAt(i);
            ops.Add(SceneManager.UnloadSceneAsync(scene));
         }
         for (int i=0; i<count; i++)
         {
            yield return ops[i];
         }

         // Reboot
         _instance = null;
         Object.Destroy(_gameObject);
         SceneManager.LoadScene(0, LoadSceneMode.Single);
      }

      class CoroutineBehaviour : MonoBehaviour
      {

      }

   }

   public class DisruptorEngineResolver : IServiceResolver<API>
   {
      private API _app;

      public DisruptorEngineResolver(API app)
      {
         _app = app;
      }

      public bool CanResolve()
      {
         return _app != null;
      }

      public bool Exists()
      {
         return _app != null;
      }

      public API Resolve()
      {
         return _app;
      }

      public void OnTeardown()
      {
         _app.TearDown();
         _app = null;
         ServiceManager.Remove(this);
      }
   }

}