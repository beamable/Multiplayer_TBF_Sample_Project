using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Calendars;

namespace Beamable.Api.Calendars
{

   public class CalendarsSubscription : PlatformSubscribable<CalendarQueryResponse, CalendarView>
   {
      public CalendarsSubscription(PlatformService platform, IBeamableRequester requester, string service) : base(platform, requester, service)
      {
      }

      public void ForceRefresh(string scope)
      {
         Refresh(scope);
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

   public class CalendarsService : AbsCalendarApi, IHasPlatformSubscriber<CalendarsSubscription, CalendarQueryResponse, CalendarView>
   {

      public CalendarsSubscription Subscribable { get; }

      public CalendarsService (PlatformService platform, IBeamableRequester requester) : base(requester, platform)
      {
         Subscribable = new CalendarsSubscription(platform, requester, SERVICE_NAME);
      }

      public override Promise<EmptyResponse> Claim(string calendarId)
      {
         return base.Claim(calendarId).Then(claimRsp => { Subscribable.ForceRefresh(calendarId); });
      }

      public override Promise<CalendarView> GetCurrent(string scope = "") => Subscribable.GetCurrent(scope);

//      public Promise<EmptyResponse> Claim(string calendarId)
//      {
//         return requester.Request<EmptyResponse>(
//            Method.POST,
//            $"/object/calendars/{platform.User.id}/claim?id={calendarId}"
//         ).Then(claimRsp => { Refresh(calendarId); });
//      }

//      protected override void OnRefresh(CalendarQueryResponse data)
//      {
//         data.Init();
//
//         foreach (var calendar in data.calendars)
//         {
//            // Schedule the next callback
//            var seconds = long.MaxValue;
//            if (calendar.nextClaimSeconds != 0 && calendar.nextClaimSeconds < seconds)
//            {
//               seconds = calendar.nextClaimSeconds;
//            }
//
//            if (calendar.remainingSeconds != 0 && calendar.remainingSeconds < seconds)
//            {
//               seconds = calendar.remainingSeconds;
//            }
//
//            if (seconds > 0)
//            {
//               ScheduleRefresh(seconds, calendar.id);
//            }
//
//            Notify(calendar.id, calendar);
//         }
//      }
   }

}