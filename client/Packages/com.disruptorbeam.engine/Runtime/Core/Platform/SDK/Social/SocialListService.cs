using System;
using System.Collections.Generic;
using Beamable.Common;
using UnityEngine;

namespace Beamable.Platform.SDK.Social
{
   public class SocialListService
   {
      private PlatformRequester _requester;
      private Promise<SocialList> _socialList;

      public SocialListService(PlatformService platform, PlatformRequester requester)
      {
         _requester = requester;

         // Invalidate the cache any time your social list changes
         platform.Notification.Subscribe("SOCIAL.UPDATE", _ => InvalidateCache());
      }

      public Promise<SocialList> Get()
      {
         if (_socialList == null)
         {
            return RefreshSocialList();
         }

         return _socialList;
      }

      public Promise<PlayerBlockStatus> BlockPlayer(long gamerTag)
      {
         return _requester.Request<PlayerBlockStatus>(
            Method.POST,
            string.Format("/social/blocked/{0}", gamerTag)
         ).Then(_ => InvalidateCache());
      }

      public Promise<PlayerBlockStatus> UnblockPlayer(long gamerTag)
      {
         return _requester.Request<PlayerBlockStatus>(
            Method.DELETE,
            string.Format("/social/blocked/{0}", gamerTag)
         ).Then(_ => InvalidateCache());
      }

      public Promise<EmptyResponse> SendFriendRequest (long gamerTag)
      {
         return _requester.Request<EmptyResponse>(
            Method.POST,
            string.Format("/social/friend/invite?gt={0}", gamerTag)
         );
      }

      public Promise<EmptyResponse> RemoveFriend (long gamerTag)
      {
         return _requester.Request<EmptyResponse>(
            Method.DELETE,
            string.Format("/social/friend/{0}", gamerTag)
         );
      }

      private Promise<SocialList> RefreshSocialList()
      {
         _socialList = _requester.Request<SocialList>(
            Method.GET,
            "/social/my"
         );
         return _socialList;
      }

      private void InvalidateCache()
      {
         _socialList = null;
      }
   }

   [Serializable]
   public class SocialList
   {
      public List<long> friend;
      public List<long> block;

      public bool IsBlocked(long dbid)
      {
         return IsIn(dbid, block);
      }

      public bool IsFriend(long dbid)
      {
         return IsIn(dbid, friend);
      }

      private static bool IsIn(long dbid, IList<long> list)
      {
         if (list == null)
         {
            return false;
         }

         for (var i = 0; i < list.Count; i++)
         {
            if (list[i] == dbid)
            {
               return true;
            }
         }

         return false;
      }
   }

   [Serializable]
   public class PlayerBlockStatus
   {
      [SerializeField]
      private long playerB = 0;
      public bool isBlocked;

      public long Player => playerB;
   }
}