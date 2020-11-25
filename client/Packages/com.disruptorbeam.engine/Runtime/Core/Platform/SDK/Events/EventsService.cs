using System;
using System.Collections.Generic;
using Beamable.Api;
using Beamable.Common;

namespace Beamable.Platform.SDK.Events
{
   public class EventsService : PlatformSubscribable<EventsGetResponse,EventsGetResponse>
   {
      public EventsService (PlatformService platform, PlatformRequester requester) : base(platform, requester,  "event-players")
      {
         platform.Notification.Subscribe("event.phase", _ => Refresh());
      }

      public Promise<EventClaimResponse> Claim(string eventId)
      {
         return requester.Request<EventClaimResponse>(
            Method.POST,
            String.Format("/object/event-players/{0}/claim?eventId={1}", platform.User.id, eventId)
         ).Then(claimRsp => { Refresh(); });
      }

      public void ForceRefresh()
      {
         Refresh();
      }

      protected override void OnRefresh(EventsGetResponse data)
      {
         data.Init();
         Notify(data);
      }
   }

   [Serializable]
   public class EventsGetResponse
   {
      public List<EventView> running;
      public List<EventView> done;

      internal void Init()
      {
         foreach (var view in running)
         {
            view.Init();
         }
         foreach (var view in done)
         {
            view.Init();
         }
      }
   }

   [Serializable]
   public class EventClaimResponse
   {
      public EventView view;
      public string gameRspJson;
   }

   [Serializable]
   public class EventView
   {
      public string id;
      public string name;
      public string leaderboardId;
      public double score;
      public long rank;
      public long secondsRemaining;
      public DateTime endTime;
      public List<EventReward> scoreRewards;
      public List<EventReward> rankRewards;

      public EventPhase currentPhase;
      public List<EventPhase> allPhases;

      internal void Init()
      {
         endTime = DateTime.UtcNow.AddSeconds(secondsRemaining);
      }
   }

   [Serializable]
   public class EventReward
   {
      public List<EventObtain> obtain;
      public double min;
      public double max;
      public bool earned;
      public bool claimed;
   }

   [Serializable]
   public class EventObtain
   {
      public string symbol;
      public int count;
   }

   [Serializable]
   public class EventPhase
   {
      public string name;
      public long durationSeconds;
      public List<EventRule> rules;
   }

   [Serializable]
   public class EventRule
   {
      public string rule;
      public string value;
   }
}