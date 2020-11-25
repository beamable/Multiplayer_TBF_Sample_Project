using System;

namespace Beamable.Server
{
   public class RequestContext
   {
      public string Cid { get; }
      public string Pid { get; }
      public long Id { get; }
      public int Status { get; }
      public long UserId { get; }
      public string Path { get; }
      public string Method { get; }
      public string Body { get; }

      public RequestContext(string cid, string pid, long id, int status, long userId, string path, string method, string body)
      {
         Cid = cid;
         Pid = pid;
         Id = id;
         UserId = userId;
         Path = path;
         Method = method;
         Status = status;
         Body = body;
      }

   }
}