using System;
using System.Collections.Generic;

namespace Beamable.Common.Api.Events
{
   public interface IEventsApi : ISupportsGet<EventsGetResponse>
   {
      Promise<EventClaimResponse> Claim(string eventId);
   }

   [Serializable]
   public class EventsGetResponse
   {
      public List<EventView> running;
      public List<EventView> done;

      public void Init()
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

      public void Init()
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