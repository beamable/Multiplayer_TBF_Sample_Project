using System;
using System.Collections.Generic;
namespace Beamable.Common.Api.Groups
{
   public class GroupsApi : IGroupsApi
   {
      public IUserContext Ctx { get; }
      public IBeamableRequester Requester { get; }

      public GroupsApi (IUserContext ctx, IBeamableRequester requester)
      {
         Ctx = ctx;
         Requester = requester;
      }

      public Promise<GroupUser> GetUser (long gamerTag) {
         return Requester.Request<GroupUser>(
            Method.GET,
            String.Format("/object/group-users/{0}", gamerTag)
         );
      }

      public Promise<Group> GetGroup (long groupId) {
         return Requester.Request<Group>(
            Method.GET,
            String.Format("/object/groups/{0}", groupId)
         );
      }

      public Promise<EmptyResponse> DisbandGroup (long group) {
         return Requester.Request<EmptyResponse>(
            Method.DELETE,
            String.Format("/object/groups/{0}", group)
         );
      }

      public Promise<GroupMembershipResponse> LeaveGroup (long group) {
          return Requester.Request<GroupMembershipResponse>(
            Method.DELETE,
            String.Format("/object/group-users/{0}/join", Ctx.UserId),
            new GroupMembershipRequest(group)
         );
      }

      public Promise<GroupMembershipResponse> JoinGroup (long group) {
          return Requester.Request<GroupMembershipResponse>(
            Method.POST,
            String.Format("/object/group-users/{0}/join", Ctx.UserId),
            new GroupMembershipRequest(group)
         );
      }

      public Promise<EmptyResponse> Petition (long group) {
          return Requester.Request<EmptyResponse>(
            Method.POST,
            String.Format("/object/groups/{0}/petition", group),
            ""
         );
      }

      public Promise<GroupSearchResponse> GetRecommendations () {
         return Requester.Request<GroupSearchResponse>(
            Method.GET,
            String.Format("/object/group-users/{0}/recommended", Ctx.UserId)
         );
      }

      public Promise<GroupSearchResponse> Search (
         string name = null,
         List<string> enrollmentTypes = null,
         bool? hasSlots = null,
         long? scoreMin = null,
         long? scoreMax = null,
         string sortField = null,
         int? sortValue = null,
         int? offset = null,
         int? limit = null
      )
      {
         string args = "";

         if (!string.IsNullOrEmpty(name)) { args = AddQuery(args, "name", name); }
         if (offset.HasValue) { args = AddQuery(args, "offset", offset.ToString()); }
         if (limit.HasValue) { args = AddQuery(args, "limit", limit.ToString()); }
         if (enrollmentTypes != null) { args = AddQuery(args, "enrollmentTypes", string.Join(",", enrollmentTypes.ToArray())); }
         if (hasSlots.HasValue) { args = AddQuery(args, "hasSlots", hasSlots.Value.ToString()); }
         if (scoreMin.HasValue) { args = AddQuery(args, "scoreMin", scoreMin.Value.ToString()); }
         if (scoreMax.HasValue) { args = AddQuery(args, "scoreMax", scoreMax.Value.ToString()); }
         if (!string.IsNullOrEmpty(sortField)) { args = AddQuery(args, "sortField", sortField); }
         if (sortValue.HasValue) { args = AddQuery(args, "sortValue", sortValue.Value.ToString()); }

         return Requester.Request<GroupSearchResponse>(
            Method.GET,
            String.Format("/object/group-users/{0}/search?{1}", Ctx.UserId, args)
         );
      }

      public Promise<GroupCreateResponse> CreateGroup (GroupCreateRequest request) {
         return Requester.Request<GroupCreateResponse>(
            Method.POST,
            String.Format("/object/group-users/{0}/group", Ctx.UserId),
            request
         );
      }

      public Promise<AvailabilityResponse> CheckAvailability (string name, string tag) {
         string query = "";
         if (name != null) {
            query += "name=" + name;
         }
         if (tag != null) {
            if (name != null) { query += "&"; }
            query += "tag=" + tag;
         }
         return Requester.Request<AvailabilityResponse>(
            Method.GET,
            String.Format("/object/group-users/{0}/availability?{1}", Ctx.UserId, query)
         );
      }

      public Promise<EmptyResponse> SetMotd (long group, string motd) {
         return Requester.Request<EmptyResponse>(
            Method.PUT,
            String.Format("/object/groups/{0}", group),
            new UpdateGroupMotd(motd)
         );
      }

      public Promise<EmptyResponse> SetSlogan (long group, string slogan) {
         return Requester.Request<EmptyResponse>(
            Method.PUT,
            String.Format("/object/groups/{0}", group),
            new UpdateGroupSlogan(slogan)
         );
      }

      public Promise<EmptyResponse> SetEnrollmentType (long group, string enrollmentType) {
         return Requester.Request<EmptyResponse>(
            Method.PUT,
            String.Format("/object/groups/{0}", group),
            new UpdateGroupEnrollmentType(enrollmentType)
         );
      }

      public Promise<EmptyResponse> SetRequirement (long group, long requirement) {
         return Requester.Request<EmptyResponse>(
         Method.PUT,
         String.Format("/object/groups/{0}", group),
         new UpdateGroupRequirement(requirement)
         );
      }

      public Promise<GroupMembershipResponse> Kick (long group, long gamerTag) {
         return Requester.Request<GroupMembershipResponse>(
            Method.DELETE,
            String.Format("/object/groups/{0}/member", group),
            new KickRequest(gamerTag)
         );
      }

      public Promise<EmptyResponse> SetRole (long group, long gamerTag, string role) {
         return Requester.Request<EmptyResponse>(
            Method.PUT,
            String.Format("/object/groups/{0}/role", group),
            new RoleChangeRequest(gamerTag, role)
         );
      }

      public string AddQuery(string query, string key, string value)
      {
         if (query.Length == 0)
         {
            return key + "=" + value;
         }
         else
         {
            return query + "&" + key + "=" + value;
         }
      }
   }

   [Serializable]
   public class GroupUser {
      public long gamerTag;
      public GroupMemberships member;
      public long updated;
   }

   [Serializable]
   public class GroupMemberships
   {
      public List<GroupMembership> guild;
   }

   [Serializable]
   public class GroupMembership
   {
      public long id;
      public List<long> subGroups;
      public long joined;
   }

   [Serializable]
   public class Group {
      public long id;
      public string name;
      public string tag;
      public string slogan;
      public string motd;
      public string enrollmentType;
      public long requirement;
      public int maxSize;
      public List<Member> members;
      public List<SubGroup> subGroups;

      public long created;
      public int freeSlots;

      public bool canDisband;
      public bool canUpdateEnrollment;
      public bool canUpdateMOTD;
      public bool canUpdateSlogan;
   }

   [Serializable]
   public class Member {
      public long gamerTag;
      public string role;

      public bool canKick;
      public bool canPromote;
      public bool canDemote;
   }

   [Serializable]
   public class SubGroup {
      public string name;
      public long requirement;
      public List<Member> members;
   }

   [Serializable]
   public class GroupMembershipRequest {
      public long group;

      public GroupMembershipRequest(long group) {
        this.group = group;
      }
   }

   [Serializable]
   public class GroupMembershipResponse {
       public bool member;
   }

   [Serializable]
   public class GroupCreateRequest {
      public string name;
      public string tag;
      public string enrollmentType;
      public long requirement;
      public int maxSize;

      public GroupCreateRequest (string name, string tag, string enrollmentType, long requirement, int maxSize) {
         this.name = name;
         this.tag = tag;
         this.enrollmentType = enrollmentType;
         this.requirement = requirement;
         this.maxSize = maxSize;
      }
   }

   [Serializable]
   public class GroupCreateResponse {
       public long group;
   }

   [Serializable]
   public class GroupSearchResponse {
      public List<Group> groups;
   }

   [Serializable]
   public class AvailabilityResponse {
      public bool name;
      public bool tag;
   }

   [Serializable]
   public class UpdateGroupMotd {
      public string motd;
      public UpdateGroupMotd(string motd) { this.motd = motd; }
   }

   [Serializable]
   public class UpdateGroupSlogan {
      public string slogan;
      public UpdateGroupSlogan(string slogan) { this.slogan = slogan; }
   }

   [Serializable]
   public class UpdateGroupEnrollmentType {
      public string enrollmentType;
      public UpdateGroupEnrollmentType(string enrollmentType) { this.enrollmentType = enrollmentType; }
   }

   [Serializable]
   public class UpdateGroupRequirement {
      public long requirement;
      public UpdateGroupRequirement(long requirement) { this.requirement = requirement; }
   }

   [Serializable]
   public class KickRequest {
      public long gamerTag;
      public KickRequest(long gamerTag) {
         this.gamerTag = gamerTag;
      }
   }

   [Serializable]
   public class RoleChangeRequest {
      public long gamerTag;
      public string role;
      public RoleChangeRequest(long gamerTag, string role) {
         this.gamerTag = gamerTag;
         this.role = role;
      }
   }
}