using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Mail;
using Beamable.Serialization;

namespace Beamable.Api.Mail
{

   public class MailSubscription : PlatformSubscribable<MailQueryResponse, MailQueryResponse>
   {
      public MailSubscription(PlatformService platform, IBeamableRequester requester) : base(platform, requester, AbsMailApi.SERVICE_NAME)
      {
      }

      protected override void OnRefresh(MailQueryResponse data)
      {
         Notify(data);
      }
   }

   public class MailService : AbsMailApi, IHasPlatformSubscriber<MailSubscription, MailQueryResponse, MailQueryResponse>
   {
      public MailSubscription Subscribable { get; }

      public MailService (PlatformService platform, IBeamableRequester requester) : base(requester, platform)
      {
         Subscribable = new MailSubscription(platform, requester);
      }

      public override Promise<MailQueryResponse> GetCurrent(string scope = "") => Subscribable.GetCurrent(scope);
   }

}