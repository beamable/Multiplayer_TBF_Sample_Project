using System;
using System.Collections.Generic;
using Beamable.Common;

namespace Beamable.Platform.SDK.QA
{
   public class QAService
   {
      private PlatformRequester _requester;
      public QAService (PlatformRequester requester)
      {
         _requester = requester;
      }

      public Promise<SubscriberDetails> GetSubscriberDetails () {
         return _requester.Request<SubscriberDetails>(
            Method.GET,
            "/basic/qa/subscriberDetails"
         );
      }

      public Promise<EmptyResponse> Publish (string message, List<string> parameters) {
         return _requester.Request<EmptyResponse>(
            Method.POST,
            "/basic/qa/publish",
            new PublishRequest(message, parameters)
         );
      }
   }

   [Serializable]
   public class SubscriberDetails {
      public string subscribeKey;
      public string gameNotificationChannel;
      public string gameGlobalNotificationChannel;
      public string playerChannel;
      public string playerForRealmChannel;
      public string customChannelPrefix;
      public string authneticationKey;
   }

   [Serializable]
   public class PublishRequest {
      public string message;
      public List<string> @params;

      public PublishRequest(string message, List<string> parameters) {
         this.message = message;
         this.@params = parameters;
      }
   }
}