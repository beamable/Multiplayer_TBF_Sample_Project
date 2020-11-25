using System;
using System.Collections.Generic;
using Beamable.Api;
using Beamable.Common;

namespace Beamable.Platform.SDK.Calendars
{
   public class CalendarsService : PlatformSubscribable<CalendarQueryResponse, CalendarView>
   {
      public CalendarsService (PlatformService platform, PlatformRequester requester) : base(platform, requester, "calendars")
      {
      }

      public Promise<EmptyResponse> Claim(string calendarId)
      {
         return requester.Request<EmptyResponse>(
            Method.POST,
            $"/object/calendars/{platform.User.id}/claim?id={calendarId}"
         ).Then(claimRsp => { Refresh(calendarId); });
      }

      protected override void OnRefresh(CalendarQueryResponse data)
      {
         data.Init();

         foreach (var calendar in data.calendars)
         {
            // Schedule the next callback
            var seconds = long.MaxValue;
            if (calendar.nextClaimSeconds != 0 && calendar.nextClaimSeconds < seconds)
            {
               seconds = calendar.nextClaimSeconds;
            }

            if (calendar.remainingSeconds != 0 && calendar.remainingSeconds < seconds)
            {
               seconds = calendar.remainingSeconds;
            }

            if (seconds > 0)
            {
               ScheduleRefresh(seconds, calendar.id);
            }

            Notify(calendar.id, calendar);
         }
      }
   }

   [Serializable]
   public class CalendarQueryResponse
   {
      public List<CalendarView> calendars;

      internal void Init()
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

      internal void Init()
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