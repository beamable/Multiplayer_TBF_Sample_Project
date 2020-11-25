using System;
using System.Collections.Generic;
using Beamable.Api;
using Beamable.Common;

namespace Beamable.Platform.SDK.Announcements
{
   public class AnnouncementsService : PlatformSubscribable<AnnouncementQueryResponse, AnnouncementQueryResponse>
   {
      public AnnouncementsService(PlatformService platform, PlatformRequester requester) : base(platform, requester, "announcements")
      {
      }

      protected override void OnRefresh(AnnouncementQueryResponse data)
      {
         foreach (var announcement in data.announcements)
         {
            announcement.Init();
         }
         Notify(data);
      }

      public Promise<EmptyResponse> MarkRead(string id)
      {
         return MarkRead(new List<string> {id});
      }

      public Promise<EmptyResponse> MarkRead(List<string> ids)
      {
         return requester.Request<EmptyResponse>(
            Method.PUT,
            String.Format("/object/announcements/{0}/read", platform.User.id),
            new AnnouncementRequest(ids)
         ).Map(rsp =>
         {
            var data = GetLatest();
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
               Notify(data);
            }

            return rsp;
         });
      }

      public Promise<EmptyResponse> MarkDeleted(string id)
      {
         return MarkDeleted(new List<string> {id});
      }

      public Promise<EmptyResponse> MarkDeleted(List<string> ids)
      {
         return requester.Request<EmptyResponse>(
            Method.DELETE,
            String.Format("/object/announcements/{0}", platform.User.id),
            new AnnouncementRequest(ids)
         ).Map(rsp =>
         {
            var data = GetLatest();
            if (data != null)
            {
               data.announcements.RemoveAll((next) => ids.Contains(next.id));
               Notify(data);
            }

            return rsp;
         });
      }

      public Promise<EmptyResponse> Claim(string id)
      {
         return Claim(new List<string> {id});
      }

      public Promise<EmptyResponse> Claim(List<string> ids)
      {
         return requester.Request<EmptyResponse>(
            Method.POST,
            String.Format("/object/announcements/{0}/claim", platform.User.id),
            new AnnouncementRequest(ids)
         ).Map(rsp =>
         {
            var data = GetLatest();
            if (data != null)
            {
               var announcements = data.announcements.FindAll((next) => ids.Contains(next.id));
               if (announcements != null)
               {
                  foreach (var announcement in announcements)
                  {
                     announcement.isRead = true;
                     announcement.isClaimed = true;
                  }
               }
               Notify(data);
            }

            return rsp;
         });
      }
   }



   [Serializable]
   public class AnnouncementQueryResponse
   {
      public List<AnnouncementView> announcements;
   }

   [Serializable]
   public class AnnouncementView : CometClientData
   {
      public string id;
      public string channel;
      public string startDate;
      public string endDate;
      public long secondsRemaining;
      public DateTime endDateTime;
      public string title;
      public string summary;
      public string body;
      public List<AnnouncementAttachment> attachments;
      public bool isRead;
      public bool isClaimed;

      public bool HasClaimsAvailable()
      {
         return !isClaimed && attachments.Count > 0;
      }

      internal void Init()
      {
         endDateTime = DateTime.UtcNow.AddSeconds(secondsRemaining);
      }
   }

   [Serializable]
   public class AnnouncementAttachment
   {
      public string symbol;
      public int count;
   }

   [Serializable]
   public class AnnouncementRequest
   {
      public List<string> announcements;

      public AnnouncementRequest(List<string> announcements)
      {
         this.announcements = announcements;
      }
   }
}