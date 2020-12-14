using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;

namespace Beamable.Api.Auth
{
   public interface IAuthService : IAuthApi
   {
      Promise<User> GetUserForEditor();
   }

   public class AuthService : AuthApi, IAuthService
   {
      public AuthService(IBeamableRequester requester) : base(requester)
      {
      }

      // This API call will only work if made by editor code.
      public Promise<User> GetUserForEditor()
      {
         return Requester.Request<User>(Method.GET, $"{ACCOUNT_URL}/admin/me", useCache:true);
      }
   }
}
