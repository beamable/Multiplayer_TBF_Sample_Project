using System;
using System.Collections.Generic;
using Beamable.Common.Pooling;
using Beamable.Serialization.SmallerJSON;

namespace Beamable.Common.Api.Mail
{
   public abstract class AbsMailApi : IMailApi
   {
      public IBeamableRequester Requester { get; }
      public IUserContext Ctx { get; }
      public const string SERVICE_NAME = "mail";

      public AbsMailApi(IBeamableRequester requester, IUserContext ctx)
      {
         Requester = requester;
         Ctx = ctx;
      }

      public Promise<SearchMailResponse> SearchMail(SearchMailRequest request)
      {
         var url = $"/object/mail/{Ctx.UserId}/search";

         using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
         {
            var dict = request.Serialize();
            var json = Json.Serialize(dict, pooledBuilder.Builder);
            return Requester.Request<SearchMailResponse>(Method.POST, url, json);
         }
      }

      public Promise<ListMailResponse> GetMail (string category, long startId = 0, long limit = 100)
      {
         var key = "search";
         var req = new SearchMailRequest(
            new SearchMailRequestClause()
            {
               name = key,
               categories = new[]{category},
               states = new[] {"Read", "Unread"},
               limit = limit,
               start = startId > 0 ? (long?)(startId) : null
            }
         );
         return SearchMail(req).Map(res =>
         {
            var content = res.results.Find(set => set.name == key)?.content;
            return new ListMailResponse()
            {
               result = content
            };
         });
      }

      public Promise<EmptyResponse> Update (MailUpdateRequest updates) {
         return Requester.Request<EmptyResponse>(
            Method.PUT,
            $"/object/mail/{Ctx.UserId}/bulk",
            updates
         );
      }

      public abstract Promise<MailQueryResponse> GetCurrent(string scope = "");
   }


   [Serializable]
   public class MailQueryResponse
   {
      public int unreadCount;
   }

   [Serializable]
   public class ListMailResponse {
      public List<MailMessage> result;
   }

   [Serializable]
   public class SearchMailRequest //: JsonSerializable.ISerializable
   {
      public SearchMailRequestClause[] clauses;

      public SearchMailRequest(params SearchMailRequestClause[] clauses)
      {
         this.clauses = clauses;
      }

      public ArrayDict Serialize()
      {
         var serializedClauses = new ArrayDict[clauses.Length];
         for (var i = 0; i < serializedClauses.Length; i++)
         {
            serializedClauses[i] = clauses[i].Serialize();
         }
         return new ArrayDict
         {
            { nameof(clauses), serializedClauses}
         };
      }

//      public void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         s.SerializeArray(nameof(clauses), ref clauses);
//      }
   }

   [Serializable]
   public class SearchMailRequestClause //: JsonSerializable.ISerializable
   {
      public string name;
      public bool onlyCount;
      public string[] categories;
      public string[] states;
      public long? forSender;
      public long? limit;
      public long? start;

      public ArrayDict Serialize()
      {
         var dict = new ArrayDict();

         dict.Add(nameof(name), name);
         dict.Add(nameof(onlyCount), onlyCount);

         if (categories != null)
         {
            dict.Add(nameof(categories), categories);
         }

         if (states != null)
         {
            dict.Add(nameof(states), states);
         }

         if (limit.HasValue)
         {
            dict.Add(nameof(limit), limit.Value);
         }

         if (forSender.HasValue)
         {
            dict.Add(nameof(forSender), forSender.Value);
         }

         if (start.HasValue)
         {
            dict.Add(nameof(start), start.Value);
         }

         return dict;
      }
//
//      public void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         s.Serialize(nameof(name), ref name);
//         s.Serialize(nameof(onlyCount), ref onlyCount);
//
//         if (categories != null)
//         {
//            s.SerializeArray(nameof(categories), ref categories);
//         }
//
//         if (states != null)
//         {
//            s.SerializeArray(nameof(states), ref states);
//         }
//
//         if (limit.HasValue)
//         {
//            var limitValue = limit.Value;
//            s.Serialize(nameof(limit), ref limitValue);
//         }
//
//         if (forSender.HasValue)
//         {
//            var senderValue = forSender.Value;
//            s.Serialize(nameof(forSender), ref senderValue);
//         }
//
//         if (start.HasValue)
//         {
//            var startValue = start.Value;
//            s.Serialize(nameof(start), ref startValue);
//         }
//      }
   }


   [Serializable]
   public class SearchMailResponse
   {
      public List<SearchMailResponseClause> results;
   }

   [Serializable]
   public class SearchMailResponseClause
   {
      public int count;
      public string name;
      public List<MailMessage> content;
   }

   [Serializable]
   public class MailMessage {
      public long id;
      public long sent;
      public long receiverGamerTag;
      public long senderGamerTag;
      public string category;
      public string subject;
      public string body;
      public string state;
      public string expires;
      public List<MailAttachment> attachments;

      public MailState MailState
      {
         get { return (MailState)Enum.Parse(typeof(MailState), state); }
      }

      public bool HasNewAttachments
      {
         get
         {
            foreach (var attachment in attachments)
            {
               if (attachment.MailAttachmentState == MailAttachmentState.New)
               {
                  return true;
               }
            }
            return false;
         }
      }
   }

   [Serializable]
   public class MailAttachment {
      public long id;
      public MailAttachmentEntitlement wrapped;
      public string state;
      public long target;

      public MailAttachmentState MailAttachmentState
      {
         get { return (MailAttachmentState)Enum.Parse(typeof(MailAttachmentState), state); }
      }
   }

   [Serializable]
   public class MailAttachmentEntitlement {
      public string symbol;
      public string specialization;
      public string action;
      public int quantity;
   }

   [Serializable]
   public class MailCounts {
      public long sent;
      public MailStateCounts received;
   }

   [Serializable]
   public class MailStateCounts {
      public long all;
      public long unread;
      public long read;
      public long deleted;
   }

   [Serializable]
   public class MailGetCountsResponse {
      public MailCounts total;
   }

   [Serializable]
   public class MailUpdate
   {
      public long mailId;
      public string state;
      public string expires;
      public bool acceptAttachments;
      public MailUpdate (long mailId, MailState state, bool acceptAttachments, string expires)
      {
         this.mailId = mailId;
         this.state = state.ToString();
         this.acceptAttachments = acceptAttachments;
         this.expires = expires;
      }
   }

   [Serializable]
   public class MailUpdateEntry {
      public long id;
      public MailUpdate update;
   }

   [Serializable]
   public class MailUpdateRequest {
      public List<MailUpdateEntry> updateMailRequests = new List<MailUpdateEntry>();

      public MailUpdateRequest Add (long id, MailState state, bool acceptAttachments, string expires) {
         MailUpdateEntry entry = new MailUpdateEntry();
         entry.id = id;
         entry.update = new MailUpdate(id, state, acceptAttachments, expires);
         updateMailRequests.Add(entry);
         return this;
      }
   }

   [Serializable]
   public class MailReceivedRequest
   {
      public string[] categories;
      public string[] states;
      public long limit;
   }

   [Serializable]
   public class MailCountRequest
   {
      public string[] categories;
   }

   public enum MailState {
      Read,
      Unread,
      Deleted
   }

   public enum MailAttachmentState
   {
      New,
      Accepted
   }
}