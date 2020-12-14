using Beamable.Common.Api;
using Beamable.Common.Api.Groups;

namespace Beamable.Api.Groups
{
   public class GroupsService : GroupsApi
   {
      public GroupsService(IUserContext ctx, IBeamableRequester requester) : base(ctx, requester)
      {
      }
   }
}
