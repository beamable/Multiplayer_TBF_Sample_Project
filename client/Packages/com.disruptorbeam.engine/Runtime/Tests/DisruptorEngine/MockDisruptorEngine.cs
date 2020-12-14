using System;
using System.Collections.Generic;
using Beamable.Api.Calendars;
using Beamable.Api.Chat;
using Beamable.Api.Commerce;
using Beamable.Api.CloudSaving;
using Beamable.Api.Inventory;
using Beamable.Api.Leaderboard;
using Beamable.Api.Matchmaking;
using Beamable.Api.Payments;
using Beamable.Api.Sim;
using Beamable.Api.Stats;
using Beamable;
using Beamable.Api;
using Beamable.Api.Announcements;
using Beamable.Api.Auth;
using Beamable.Api.Connectivity;
using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Common.Api.Tournaments;
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
      public ITournamentApi Tournaments { get; }

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