namespace Beamable.Common.Api.Auth
{
   public interface IAuthApi : IBeamableApi
   {
      Promise<User> GetUser();
      Promise<User> GetUser(TokenResponse token);
      Promise<bool> IsEmailAvailable(string email);
      Promise<bool> IsThirdPartyAvailable(AuthThirdParty thirdParty, string token);
      Promise<TokenResponse> CreateUser();
      Promise<TokenResponse> LoginRefreshToken(string refreshToken);
      Promise<TokenResponse> Login(string username, string password, bool mergeGamerTagToAccount = true);
      Promise<TokenResponse> LoginThirdParty(AuthThirdParty thirdParty, string thirdPartyToken, bool includeAuthHeader = true);
      Promise<User> RegisterDBCredentials(string email, string password);
      Promise<User> RegisterThirdPartyCredentials(AuthThirdParty thirdParty, string accessToken);
      Promise<EmptyResponse> IssueEmailUpdate(string newEmail);
      Promise<EmptyResponse> ConfirmEmailUpdate(string code, string password);
      Promise<EmptyResponse> IssuePasswordUpdate(string email);
      Promise<EmptyResponse> ConfirmPasswordUpdate(string code, string newPassword);
      Promise<CustomerRegistrationResponse> RegisterDisruptorEngineCustomer(string email, string password, string projectName);
   }
}