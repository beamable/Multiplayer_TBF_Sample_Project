using System;
using System.Collections.Generic;

namespace Beamable.Common.Api.Calendars
{
   public interface ICalendarApi : ISupportsGet<CalendarView>
   {
      Promise<EmptyResponse> Claim(string calendarId);
   }


   [Serializable]
   public class CalendarQueryResponse
   {
      public List<CalendarView> calendars;

      public void Init()
      {
         // Set the absolute timestamps for when state changes
         foreach (var calendar in calendars)
         {
            calendar.Init();
         }
      }
   }

   [Serializable]
   public class CalendarView
   {
      public string id;
      public List<RewardCalendarDay> days;
      public int nextIndex;
      public long remainingSeconds;
      public long nextClaimSeconds;
      public DateTime nextClaimTime;
      public DateTime endTime;

      public void Init()
      {
         nextClaimTime = DateTime.UtcNow.AddSeconds(nextClaimSeconds);
         endTime = DateTime.UtcNow.AddSeconds(remainingSeconds);
      }
   }

   [Serializable]
   public class RewardCalendarDay
   {
      public List<RewardCalendarObtain> obtain;
   }

   [Serializable]
   public class RewardCalendarObtain
   {
      public string symbol;
      public string specialization;
      public string action;
      public int quantity;
   }
}