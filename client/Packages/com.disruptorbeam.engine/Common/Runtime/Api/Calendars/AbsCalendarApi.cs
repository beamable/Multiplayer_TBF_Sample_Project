namespace Beamable.Common.Api.Calendars
{
   public abstract class AbsCalendarApi : ICalendarApi
   {
      protected const string SERVICE_NAME = "calendars";
      public IBeamableRequester Requester { get; }
      public IUserContext Ctx { get; }

      public AbsCalendarApi(IBeamableRequester requester, IUserContext ctx)
      {
         Requester = requester;
         Ctx = ctx;
      }

      public virtual Promise<EmptyResponse> Claim(string calendarId)
      {
         return Requester.Request<EmptyResponse>(
            Method.POST,
            $"/object/calendars/{Ctx.UserId}/claim?id={calendarId}"
         );
      }

      public abstract Promise<CalendarView> GetCurrent(string scope = "");
   }
}