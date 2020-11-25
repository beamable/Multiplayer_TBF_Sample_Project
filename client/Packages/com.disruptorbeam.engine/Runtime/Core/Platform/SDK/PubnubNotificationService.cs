using System;
using Beamable.Common;

namespace Beamable.Platform.SDK
{
   public class PubnubNotificationService
   {
      private PlatformRequester _requester;

      public PubnubNotificationService (PlatformRequester requester)
      {
         _requester = requester;
      }

      public Promise<SubscriberDetailsResponse> GetSubscriberDetails ()
      {
         return _requester.Request<SubscriberDetailsResponse>(Method.GET, "/basic/chat/subscriberDetails");
      }
   }

   [Serializable]
   public class SubscriberDetailsResponse
   {
      public string subscribeKey;
      public string gameNotificationChannel;
      public string gameGlobalNotificationChannel;
      public string playerChannel;
      public string playerForRealmChannel;
      public string customChannelPrefix;
      public string authenticationKey;
   }
}