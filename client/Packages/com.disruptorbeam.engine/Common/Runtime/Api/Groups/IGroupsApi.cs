using System.Collections.Generic;

namespace Beamable.Common.Api.Groups
{
   public interface IGroupsApi
   {
      Promise<GroupUser> GetUser (long gamerTag);
      Promise<Group> GetGroup (long groupId);
      Promise<EmptyResponse> DisbandGroup (long group);
      Promise<GroupMembershipResponse> LeaveGroup (long group);
      Promise<GroupMembershipResponse> JoinGroup (long group);
      Promise<EmptyResponse> Petition (long group);
      Promise<GroupSearchResponse> GetRecommendations ();

      Promise<GroupSearchResponse> Search (
         string name = null,
         List<string> enrollmentTypes = null,
         bool? hasSlots = null,
         long? scoreMin = null,
         long? scoreMax = null,
         string sortField = null,
         int? sortValue = null,
         int? offset = null,
         int? limit = null
      );

      Promise<GroupCreateResponse> CreateGroup (GroupCreateRequest request);
      Promise<AvailabilityResponse> CheckAvailability (string name, string tag);
      Promise<EmptyResponse> SetMotd (long group, string motd);
      Promise<EmptyResponse> SetSlogan (long group, string slogan);
      Promise<EmptyResponse> SetEnrollmentType (long group, string enrollmentType);
      Promise<EmptyResponse> SetRequirement (long group, long requirement);
      Promise<GroupMembershipResponse> Kick (long group, long gamerTag);
      Promise<EmptyResponse> SetRole (long group, long gamerTag, string role);
      string AddQuery(string query, string key, string value);
   }
}