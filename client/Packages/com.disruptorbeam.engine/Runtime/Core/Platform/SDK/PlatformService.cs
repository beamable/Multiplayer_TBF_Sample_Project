using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Beamable.Common;
using Beamable.Config;
using Beamable.Coroutines;
using Beamable.Platform.SDK.Analytics;
using Beamable.Platform.SDK.Announcements;
using Beamable.Platform.SDK.Auth;
using Beamable.Platform.SDK.Calendars;
using Beamable.Platform.SDK.Chat;
using Beamable.Platform.SDK.CloudSaving;
using Beamable.Platform.SDK.Commerce;
using Beamable.Platform.SDK.Entitlements;
using Beamable.Platform.SDK.Events;
using Beamable.Platform.SDK.Groups;
using Beamable.Platform.SDK.Inventory;
using Beamable.Platform.SDK.Leaderboard;
using Beamable.Platform.SDK.Mail;
using Beamable.Platform.SDK.Matchmaking;
using Beamable.Platform.SDK.Notification;
using Beamable.Platform.SDK.Payments;
using Beamable.Platform.SDK.QA;
using Beamable.Platform.SDK.Sessions;
using Beamable.Platform.SDK.Sim;
using Beamable.Platform.SDK.Social;
using Beamable.Platform.SDK.Stats;
using Beamable.Platform.SDK.Tag;
using Beamable.Platform.SDK.Tournaments;
using Beamable.Service;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Beamable.Platform.SDK
{
   public enum Method
   {
      GET = 1,
      POST = 2,
      PUT = 3,
      DELETE = 4
   }

   [EditorServiceResolver(typeof(PlatformEditorServiceResolver))]
   public class PlatformService : IDisposable
   {
      private const string acceptHeader = "application/json";
      private const int HeartbeatInterval = 30;
      private const int MaxInitRetries = 4;

      private delegate Promise<Unit> InitStep();

      // Initialization Control
      public Promise<Unit> OnReady;
      private InitStep[] _initSteps;
      private int _nextInitStep = 0;
      public event Action OnShutdown;
      public event Action OnReloadUser;

      // Required runtime singletons
      private static AccessTokenStorage _accessTokenStorage = new AccessTokenStorage();

      // API Services
      public AnnouncementsService Announcements;
      public AuthService Auth;
      public CalendarsService Calendars;
      public readonly ChatService Chat;
      public CloudSavingService CloudSaving;
      public ConnectivityService ConnectivityService;
      public CommerceService Commerce;
      public EntitlementService Entitlements;
      public EventsService Events;
      public GameRelayService GameRelay;
      public GroupsService Groups;
      public Heartbeat Heartbeat;
      public InventoryService Inventory;
      public LeaderboardService Leaderboard;
      public MailService Mail;
      public MatchmakingService Matchmaking;
      public NotificationService Notification;
      public PaymentService Payments;
      public TournamentService Tournaments;

      public PaymentDelegate PaymentDelegate =>
         ServiceManager.Exists<PaymentDelegate>() ? ServiceManager.Resolve<PaymentDelegate>() : null;
      public Promise<PaymentDelegate> InitializedPaymentDelegate = new Promise<PaymentDelegate>();
      public readonly PubnubNotificationService PubnubNotificationService;
      public PushService Push;
      public QAService QA;
      public SessionService Session;
      public SocialListService Social;
      public StatsService Stats;
      public TagService Tag;

      // High order functionality
      private User _user = new User();
      public AnalyticsTracker Analytics;
      public readonly ChatProvider ChatProvider;
      public EntitlementSubscriber EntitlementSubscriber;
      public PubnubSubscriptionManager PubnubSubscriptionManager;

      // Configuration values
      public bool DebugMode;
      private bool _withLocalNote;
      protected string platform
      {
         get => _requester.Host;
         set => _requester.Host = value;
      }
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

      public string Shard
      {
         get => _requester.Shard;
         set => _requester.Shard = value;
      }

      public event Action TimeOverrideChanged;

      public string TimeOverride
      {
         get => _requester.TimeOverride;
         set
         {
            if (value == null)
            {
               _requester.TimeOverride = null;
               TimeOverrideChanged();
               return;
            }

            var date = DateTime.Parse(value, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var str = date.ToString("yyyy-MM-ddTHH:mm:ssZ");
            _requester.TimeOverride = str;
            TimeOverrideChanged();
         }
      }

      // System references
      private GameObject _gameObject;
      private PlatformRequester _requester;

      public User User => _user;
      public PlatformRequester Requester => _requester;

      public AccessToken AccessToken => _requester.Token;
      public PlatformService() : this(false)
      {
      }

      public PlatformService(bool debugMode, bool withLocalNote = true)
      {
         DebugMode = debugMode;
         _withLocalNote = withLocalNote;

         _gameObject = new GameObject("PlatformService");
         Object.DontDestroyOnLoad(_gameObject);

         // Configure initialization
         OnReady = new Promise<Unit>();
         _initSteps = new InitStep[]
         {
            InitStepLoadToken,
            InitStepRefreshAccount,
            InitStepGetAccount,
            InitStepStartSession,
            InitStepStartAuxiliary
         };

            // Attach child services
         ConnectivityService = new ConnectivityService(ServiceManager.Resolve<CoroutineService>());
         _requester = new PlatformRequester("", _accessTokenStorage, ConnectivityService);
         if (_gameObject != null)
            Notification = _gameObject.AddComponent<NotificationService>();
         Analytics = new AnalyticsTracker(this, _requester, ServiceManager.Resolve<CoroutineService>(), 30, 10);
         Announcements = new AnnouncementsService(this, _requester);
         Auth = new AuthService(_requester);
         Calendars = new CalendarsService(this, _requester);
         Chat = new ChatService(this, _requester);
         if (_gameObject != null)
            ChatProvider = _gameObject.AddComponent<PubNubChatProvider>();
         CloudSaving = new CloudSavingService(this,_requester,ServiceManager.Resolve<CoroutineService>());
         Commerce = new CommerceService(this, _requester);
         Entitlements = new EntitlementService(_requester);
         EntitlementSubscriber = new EntitlementSubscriber(this);
         Events = new EventsService(this, _requester);
         GameRelay = new GameRelayService(this, _requester);
         Groups = new GroupsService(this, _requester);
         Inventory = new InventoryService(this, _requester);
         Leaderboard = new LeaderboardService(this, _requester);
         Mail = new MailService(this, _requester);
         Matchmaking = new MatchmakingService(_requester);
         Payments = new PaymentService(this, _requester);
         PubnubNotificationService = new PubnubNotificationService(_requester);
         if (_gameObject != null)
         {
            PubnubSubscriptionManager = _gameObject.AddComponent<PubnubSubscriptionManager>();
            PubnubSubscriptionManager.Initialize(this);
         }
         Push = new PushService(_requester);
         QA = new QAService(_requester);
         Session = new SessionService(this, _requester);
         Social = new SocialListService(this, _requester);
         Stats = new StatsService(this, _requester);
         Tournaments = new TournamentService(Stats, _requester);
         Tag = new TagService(this, _requester);

      }

      public void Dispose()
      {
         OnShutdown?.Invoke();
         _requester?.Dispose();

         if (ApplicationLifetime.isQuitting)
         {
            return;
         }

         if (_gameObject != null)
            Object.Destroy(_gameObject);
      }

      private void ContinueInitialize(Promise<Unit> initResult)
      {
         if (_nextInitStep >= _initSteps.Length)
         {
            OnReady.CompleteSuccess(Promise<Unit>.Unit);
            initResult.CompleteSuccess(Promise<Unit>.Unit);
            return;
         }

         var coroutineService = ServiceManager.Resolve<CoroutineService>();
         coroutineService.StartNew("Platform", RetryInitializeStep(initResult));
      }

      private IEnumerator RetryInitializeStep(Promise<Unit> initResult)
      {
         int tries = 0;
         bool done = false;
         Exception lastError = null;
         bool skipped = false;
         while (!done && (tries < MaxInitRetries))
         {
            var stepDone = false;
            var promise = _initSteps[_nextInitStep]();
            promise.Then(result =>
            {
               stepDone = true;
               done = true;
            });
            promise.Error(err =>
            {
                if (err is NoConnectivityException)
                {
                    Debug.LogWarning(err.Message);
                    skipped = true;
                }
               lastError = err;
               stepDone = true;
               tries++;
            });

            // Wait for the outstanding promise to resolve
            while (!stepDone)
            {
               yield return Yielders.EndOfFrame;
            }
         }

        if (done && !skipped)
        {
            _nextInitStep += 1;
            ContinueInitialize(initResult);
        }
        else if (skipped && _nextInitStep > Array.IndexOf(_initSteps, InitStepGetAccount))
        {
            OnReady.CompleteSuccess(Promise<Unit>.Unit);
            initResult.CompleteSuccess(Promise<Unit>.Unit);
        }
        else
        {
          initResult.CompleteError(lastError);
        }
      }

        private void RetryInitializationOnInternetReconnect(bool tryToRestart)
        {

            if (tryToRestart)
            {
                OnReady = new Promise<Unit>();
                Promise<Unit> initResult = new Promise<Unit>();
                ContinueInitialize(initResult);
            }
       }

        public Promise<Unit> Initialize(string language)
      {
         Promise<Unit> initResult = new Promise<Unit>();

         // Pull out config values
         platform = ConfigDatabase.GetString("platform");
         Cid = ConfigDatabase.GetString("cid");
         Pid = ConfigDatabase.GetString("pid");
         _requester.Language = language;
         ConnectivityService.OnConnectivityChanged += RetryInitializationOnInternetReconnect;
         ContinueInitialize(initResult);

         return initResult;
      }

      private Promise<Unit> InitStepLoadToken()
      {
         return _accessTokenStorage.LoadTokenForRealm(Cid, Pid).Map(token =>
         {
            _requester.Token = token;
            return Promise<Unit>.Unit;
         });
      }

      private Promise<Unit> InitStepRefreshAccount()
      {
         // Create a new account
         if (_requester.Token == null)
         {
            return Auth.CreateUser().Map(rsp =>
            {
               SaveToken(rsp);
               return Promise<Unit>.Unit;
            });
         }

         // Refresh token
         if (_requester.Token.IsExpired)
         {
            return Auth.LoginRefreshToken(_requester.Token.RefreshToken).Map(rsp =>
            {
               SaveToken(rsp);
               return Promise<Unit>.Unit;
            });
         }

         // Ready
         return Promise<Unit>.Successful(Promise<Unit>.Unit);
      }

      private Promise<Unit> InitStepGetAccount()
      {
         return ReloadUser().Map(rsp => Promise<Unit>.Unit);
      }

      public Promise<Unit> StartNewSession()
      {
         return AdvertisingIdentifier.AdvertisingIdentifier.GetIdentifier()
            .FlatMap(id => Session.StartSession(id, _requester.Language)).Map(_ => PromiseBase.Unit);
      }

      private Promise<Unit> InitStepStartSession()
      {
         return StartNewSession();
      }

      private Promise<Unit> InitStepStartAuxiliary()
      {
        //If you lose internet in the middle of these warming up, we may not recover properly.
         PubnubSubscriptionManager.SubscribeToProvider();
         PaymentDelegate?.Initialize().Then(_ =>
         {
            InitializedPaymentDelegate.CompleteSuccess(PaymentDelegate);
         });
         if (_withLocalNote)
            Notification.RegisterForNotifications(this);
         Heartbeat = new Heartbeat(this, ServiceManager.Resolve<CoroutineService>(), HeartbeatInterval);
         Heartbeat.Start();
         return Promise<Unit>.Successful(Promise<Unit>.Unit);
      }

      public Promise<ISet<UserBundle>> GetDeviceUsers()
      {
         var promises = Array.ConvertAll(_accessTokenStorage.RetrieveDeviceRefreshTokens(),
            token => Auth.GetUser(token).Map(user => new UserBundle
            {
               User = user,
               Token = token
            }));

         return Promise.Sequence(promises).Map(userBundles => (new HashSet<UserBundle>(userBundles) as ISet<UserBundle>));
      }

      public void RemoveDeviceUsers(TokenResponse token)
      {
         _accessTokenStorage.RemoveDeviceRefreshToken(token);
      }

      public Promise<User> ReloadUser()
      {
         return Auth.GetUser().Map(user =>
         {
            _user = user;
            OnReloadUser?.Invoke();
            return user;
         });
      }

      public void SetUser(User user)
      {
         _user = user;
      }

      public Promise<Unit> SaveToken(TokenResponse rsp)
      {
         ClearToken();
         _requester.Token = new AccessToken(_accessTokenStorage, Cid, Pid, rsp.access_token, rsp.refresh_token, rsp.expires_in);
         return _requester.Token.Save();
      }

      public void ClearToken()
      {
         _requester.DeleteToken();
      }

      public void ClearDeviceUsers()
      {
         _accessTokenStorage.ClearDeviceRefreshTokens();
      }
   }

   public class PlatformEditorServiceResolver : IServiceResolver<PlatformService>
   {
      private static PlatformService instance;

      public bool CanResolve()
      {
         return !ApplicationLifetime.isQuitting;
      }

      public bool Exists()
      {
         return (instance != null) && !ApplicationLifetime.isQuitting;
      }

      public PlatformService Resolve()
      {
         if (instance == null)
         {
            if (ApplicationLifetime.isQuitting)
            {
               return null;
            }

            ConfigDatabase.Init();

            instance = new PlatformService(true);
            instance.Initialize("en");
         }

         return instance;
      }

      public void OnTeardown()
      {
         if (ApplicationLifetime.isQuitting)
         {
            return;
         }

         instance = null;
      }
   }
}