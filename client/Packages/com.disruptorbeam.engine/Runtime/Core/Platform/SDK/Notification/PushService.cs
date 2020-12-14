using System;
using Beamable.Common;
using Beamable.Common.Api;

namespace Beamable.Api.Notification
{
   public class PushService
   {
      private PlatformRequester _requester;
      public PushService (PlatformRequester requester)
      {
         _requester = requester;
      }

      public Promise<EmptyResponse> Register (string provider, string token)
      {
         return _requester.Request<EmptyResponse>(Method.POST, "/basic/push/register", new PushRegisterRequest(provider, token));
      }
   }

   [Serializable]
   public class PushRegisterRequest
   {
      public string provider;
      public string token;

      public PushRegisterRequest (string provider, string token)
      {
         this.provider = provider;
         this.token = token;
      }
   }
}