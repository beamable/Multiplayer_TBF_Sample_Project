using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Events;

namespace Beamable.Api.Events
{

   public class EventSubscription : PlatformSubscribable<EventsGetResponse, EventsGetResponse>
   {
      public EventSubscription(PlatformService platform, IBeamableRequester requester) : base(platform, requester, AbsEventsApi.SERVICE_NAME)
      {
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

   public class EventsService : AbsEventsApi, IHasPlatformSubscriber<EventSubscription, EventsGetResponse, EventsGetResponse>
   {
      public EventSubscription Subscribable { get; }

      public EventsService(PlatformService platform, IBeamableRequester requester) : base(requester, platform)
      {
         Subscribable = new EventSubscription(platform, requester);
      }

      public override Promise<EventClaimResponse> Claim(string eventId)
      {
         return base.Claim(eventId).Then(_ => Subscribable.ForceRefresh());
      }

      public override Promise<EventsGetResponse> GetCurrent(string scope = "") => Subscribable.GetCurrent(scope);
   }

}