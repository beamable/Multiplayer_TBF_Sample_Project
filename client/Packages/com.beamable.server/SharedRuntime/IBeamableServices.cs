
using Beamable.Common.Api.Leaderboards;
using Beamable.Common.Content;
using Beamable.Server.Api;
using Beamable.Server.Api.Announcements;
using Beamable.Server.Api.Calendars;
using Beamable.Server.Api.Events;
using Beamable.Server.Api.Groups;
using Beamable.Server.Api.Inventory;
using Beamable.Server.Api.Leaderboards;
using Beamable.Server.Api.Mail;
using Beamable.Server.Api.Social;
using Beamable.Server.Api.Stats;
using Beamable.Server.Api.Tournament;

namespace Beamable.Server
{
   public interface IBeamableServices
   {
      IMicroserviceAuthApi Auth { get; }
      IMicroserviceStatsApi Stats { get; }
      IContentService Content { get; }
      IMicroserviceInventoryApi Inventory { get; }
      IMicroserviceLeaderboardsApi Leaderboards { get; }
      IMicroserviceAnnouncementsApi Announcements { get; }
      IMicroserviceCalendarsApi Calendars { get; }
      IMicroserviceEventsApi Events { get; }
      IMicroserviceGroupsApi Groups { get; }
      IMicroserviceMailApi Mail { get; }
      IMicroserviceSocialApi Social { get; }
      IMicroserviceTournamentApi Tournament { get; }
   }
}