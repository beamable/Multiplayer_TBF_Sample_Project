using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Announcements;

namespace Beamable.Api.Announcements
{
   public class AnnouncementsSubscription : PlatformSubscribable<AnnouncementQueryResponse, AnnouncementQueryResponse>
   {
      public AnnouncementsSubscription(PlatformService platform, IBeamableRequester requester, string service) : base(platform, requester, service)
      {
      }

      protected override void OnRefresh(AnnouncementQueryResponse data)
      {
         foreach (var announcement in data.announcements)
         {
            announcement.endDateTime = DateTime.UtcNow.AddSeconds(announcement.secondsRemaining);
         }
         Notify(data);
      }
   }

   public class AnnouncementsService : AbsAnnouncementsApi , IHasPlatformSubscriber<AnnouncementsSubscription, AnnouncementQueryResponse, AnnouncementQueryResponse>
   {
      public AnnouncementsSubscription Subscribable { get; }

      public AnnouncementsService(PlatformService platform, IBeamableRequester requester) : base(requester, platform)
      {
         Subscribable = new AnnouncementsSubscription(platform, requester, "announcements");
      }

      public override Promise<EmptyResponse> Claim(List<string> ids)
      {
         return base.Claim(ids).Then(_ =>
         {
            var data = Subscribable.GetLatest();
            if (data == null) return;

            var announcements = data.announcements.FindAll((next) => ids.Contains(next.id));
            if (announcements != null)
            {
               foreach (var announcement in announcements)
               {
                  announcement.isRead = true;
                  announcement.isClaimed = true;
               }
            }
            Subscribable.Notify(data);
         });
      }

      public override Promise<EmptyResponse> MarkDeleted(List<string> ids)
      {
         return base.MarkDeleted(ids).Then(_ =>
         {
            var data = Subscribable.GetLatest();
            if (data != null)
            {
               data.announcements.RemoveAll((next) => ids.Contains(next.id));
               Subscribable.Notify(data);
            }
         });
      }

      public override Promise<EmptyResponse> MarkRead(List<string> ids)
      {
         return base.MarkRead(ids).Then(_ =>
         {
            var data = Subscribable.GetLatest();
            if (data != null)
            {
               var announcements = data.announcements.FindAll((next) => ids.Contains(next.id));
               if (announcements != null)
               {
                  foreach (var announcement in announcements)
                  {
                     announcement.isRead = true;
                  }
               }
               Subscribable.Notify(data);
            }
         });
      }

      public override Promise<AnnouncementQueryResponse> GetCurrent(string scope = "") =>
         Subscribable.GetCurrent(scope);

   }

}