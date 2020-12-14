using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Common.Api;

namespace Beamable.Api.Entitlements
{
   public class EntitlementService
   {
      private PlatformRequester _requester;
      public EntitlementService (PlatformRequester requester)
      {
         _requester = requester;
      }

      public Promise<EntitlementResponse> Get (string symbol, string state)
      {
         return _requester.Request<EntitlementResponse>(
            Method.GET,
            string.Format("/entitlement/v2/my?symbol={0}&state={1}", symbol, state)
         );
      }
   }

   [Serializable]
   public class EntitlementResponse
   {
      public List<Entitlement> entitlements;
   }

   [Serializable]
   public class Entitlement
   {
      public string uuid;
      public long gamerTag;
      public string symbol;
      public string specialized;
      public string state;
   }
}