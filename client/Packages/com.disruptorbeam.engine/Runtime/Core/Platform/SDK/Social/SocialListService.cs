using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Social;
using UnityEngine;

namespace Beamable.Api.Social
{
   public class SocialListService : SocialApi
   {
      public SocialListService(PlatformService platform, IBeamableRequester requester) : base(requester, platform)
      {
         // Invalidate the cache any time your social list changes
         platform.Notification.Subscribe("SOCIAL.UPDATE", _ => InvalidateCache());
      }

      public override Promise<PlayerBlockStatus> BlockPlayer(long gamerTag)
      {
         return base.BlockPlayer(gamerTag).Then(_ => InvalidateCache());
      }

      public Promise<PlayerBlockStatus> UnblockPlayer(long gamerTag)
      {
         return base.UnblockPlayer(gamerTag).Then(_ => InvalidateCache());
      }

      private void InvalidateCache()
      {
         _socialList = null;
      }
   }

}