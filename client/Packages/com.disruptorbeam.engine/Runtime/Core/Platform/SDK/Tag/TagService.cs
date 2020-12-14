using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Common.Api;

namespace Beamable.Api.Tag
{
   public class TagService
   {
      private PlatformService _platform;
      private PlatformRequester _requester;
      public TagService (PlatformService platform, PlatformRequester requester)
      {
         _platform = platform;
         _requester = requester;
      }

      public Promise<EmptyResponse> UpdateAlias (string alias)
      {
         return _requester.Request<EmptyResponse>(
            Method.PUT,
            String.Format("/tag/my?alias={0}", alias)
         ).Map(rsp =>
         {
            // Wipe my game stats since they have changed
            _platform.Stats.GetCache("game.public.player.").Remove(_platform.User.id);

            return rsp;
         });
      }
   }

   [Serializable]
   public class AliasResponse {
      public List<TagAlias> tags;

      public Dictionary<long, string> ToDictionary () {
         Dictionary<long, string> result = new Dictionary<long, string>();
         for (int i=0; i<tags.Count; i++) {
            var next = tags[i];
            result[next.gamerTag] = next.alias;
         }
         return result;
      }
   }

   [Serializable]
   public class TagAlias {
      public long gamerTag;
      public string alias;
   }
}