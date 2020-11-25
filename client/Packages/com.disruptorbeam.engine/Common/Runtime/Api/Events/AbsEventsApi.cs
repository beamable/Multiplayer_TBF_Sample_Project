using System;

namespace Beamable.Common.Api.Events
{
   public abstract class AbsEventsApi : IEventsApi
   {
      public const string SERVICE_NAME = "event-players";

      public IBeamableRequester Requester { get; }
      public IUserContext Ctx { get; }

      protected AbsEventsApi(IBeamableRequester requester, IUserContext ctx)
      {
         Requester = requester;
         Ctx = ctx;
      }

      public virtual Promise<EventClaimResponse> Claim(string eventId)
      {
         return Requester.Request<EventClaimResponse>(
            Method.POST,
            String.Format("/object/event-players/{0}/claim?eventId={1}", Ctx.UserId, eventId)
         );
      }

      public abstract Promise<EventsGetResponse> GetCurrent(string scope = "");
   }
}