using Beamable.Common;
using Beamable.Common.Api.Auth;

namespace Beamable.Server.Api
{
   public interface IMicroserviceAuthApi : IAuthApi
   {
      Promise<User> GetUser(long userId);
   }
}