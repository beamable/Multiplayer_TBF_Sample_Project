using System;
using System.Collections.Generic;
using Beamable.Platform.SDK;
using Beamable.Platform.SDK.Announcements;
using Beamable.Platform.SDK.Auth;
using Beamable.Platform.SDK.Calendars;
using Beamable.Platform.SDK.Chat;
using Beamable.Platform.SDK.Commerce;
using Beamable.Platform.SDK.CloudSaving;
using Beamable.Platform.SDK.Inventory;
using Beamable.Platform.SDK.Leaderboard;
using Beamable.Platform.SDK.Matchmaking;
using Beamable.Platform.SDK.Payments;
using Beamable.Platform.SDK.Sim;
using Beamable.Platform.SDK.Stats;
using Beamable.Platform.SDK.Tournaments;
using Beamable;
using Beamable.Common;
using Beamable.Content;

namespace Packages.DisruptorEngine.Runtime.Tests.DisruptorEngine
{
   public class MockBeamableApi : IBeamableAPI
   {
      public User User { get; set; }
      public AccessToken Token { get; }
      public AnnouncementsService AnnouncementService { get; set; }
      public MockAuthService MockAuthService { get; set; } = new MockAuthService();
      public IAuthService AuthService => MockAuthService;
      public CalendarsService CalendarsService { get; set; }
      public ChatService ChatService { get; set; }

      public CloudSavingService CloudSavingService { get; set; }
      public ContentService ContentService { get; set; }
      public GameRelayService GameRelayService { get; set; }
      public InventoryService InventoryService { get; set; }
      public LeaderboardService LeaderboardService { get; set; }
      public PlatformRequester Requester { get; set; }
      public StatsService Stats { get; }
      public CommerceService Commerce { get; }
      public MatchmakingService Matchmaking { get; }
      public Promise<PaymentDelegate> PaymentDelegate { get; }
      public ConnectivityService ConnectivityService { get;  }
      public ITournamentService Tournaments { get; }

      public event Action<User> OnUserChanged;
      public event Action<bool> OnConnectivityChanged;

      public Func<TokenResponse, Promise<Unit>> ApplyTokenDelegate;
      public Func<Promise<ISet<UserBundle>>> GetDeviceUsersDelegate;

      public void UpdateUserData(User user)
      {
         User = user;
         TriggerOnUserChanged(user);
      }

      public void TriggerOnUserChanged(User user)
      {
         OnUserChanged?.Invoke(user);
      }

      public void TriggerOnConnectivityChanged(bool isSuccessful)
      {
         OnConnectivityChanged?.Invoke(isSuccessful);
      }

      public Promise<ISet<UserBundle>> GetDeviceUsers()
      {
         return GetDeviceUsersDelegate();
      }

      public void RemoveDeviceUser(TokenResponse token)
      {
         throw new NotImplementedException();
      }

      public Promise<Unit> ApplyToken(TokenResponse response)
      {
         var promise = ApplyTokenDelegate(response);
         TriggerOnUserChanged(null);
         return promise;
      }
   }
}