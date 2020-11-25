using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Common.Api.Social
{
   public class SocialApi : ISocialApi
   {
      public IBeamableRequester Requester { get; }
      public IUserContext Ctx { get; }

      protected Promise<SocialList> _socialList;

      public SocialApi(IBeamableRequester requester, IUserContext ctx)
      {
         Requester = requester;
         Ctx = ctx;
      }

      public Promise<SocialList> Get()
      {
         if (_socialList == null)
         {
            return RefreshSocialList();
         }

         return _socialList;
      }

      public virtual Promise<PlayerBlockStatus> BlockPlayer(long gamerTag)
      {
         return Requester.Request<PlayerBlockStatus>(
            Method.POST,
            string.Format("/social/blocked/{0}", gamerTag));
      }

      public virtual Promise<PlayerBlockStatus> UnblockPlayer(long gamerTag)
      {
         return Requester.Request<PlayerBlockStatus>(
            Method.DELETE,
            string.Format("/social/blocked/{0}", gamerTag));
      }

      public virtual Promise<EmptyResponse> SendFriendRequest (long gamerTag)
      {
         return Requester.Request<EmptyResponse>(
            Method.POST,
            string.Format("/social/friend/invite?gt={0}", gamerTag)
         );
      }

      public virtual Promise<EmptyResponse> RemoveFriend (long gamerTag)
      {
         return Requester.Request<EmptyResponse>(
            Method.DELETE,
            string.Format("/social/friend/{0}", gamerTag)
         );
      }

      public virtual Promise<SocialList> RefreshSocialList()
      {
         _socialList = Requester.Request<SocialList>(
            Method.GET,
            "/social/my"
         );
         return _socialList;
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