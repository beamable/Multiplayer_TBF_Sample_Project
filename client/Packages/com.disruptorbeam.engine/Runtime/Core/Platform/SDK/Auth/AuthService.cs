using UnityEngine;
using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Serialization;
using UnityEngine.Networking;

namespace Beamable.Platform.SDK.Auth
{
   public interface IAuthService
   {
      Promise<User> GetUser();
      // This API call will only work if made by editor code.
      Promise<User> GetUserForEditor();
      Promise<User> GetUser(TokenResponse token);
      Promise<bool> IsEmailAvailable(string email);
      Promise<bool> IsThirdPartyAvailable(AuthThirdParty thirdParty, string token);
      Promise<TokenResponse> CreateUser();
      Promise<TokenResponse> LoginRefreshToken(string refreshToken);
      Promise<TokenResponse> Login(
         string username,
         string password,
         bool mergeGamerTagToAccount = true,
         bool customerScoped = false);
      Promise<TokenResponse> LoginThirdParty(AuthThirdParty thirdParty, string thirdPartyToken, bool includeAuthHeader = true);
      Promise<User> RegisterDBCredentials(string email, string password);
      Promise<User> RegisterThirdPartyCredentials(AuthThirdParty thirdParty, string accessToken);
      Promise<EmptyResponse> IssueEmailUpdate(string newEmail);
      Promise<EmptyResponse> ConfirmEmailUpdate(string code, string password);
      Promise<EmptyResponse> IssuePasswordUpdate(string email);
      Promise<EmptyResponse> ConfirmPasswordUpdate(string code, string newPassword);
      Promise<CustomerRegistrationResponse> RegisterDisruptorEngineCustomer(string email, string password, string projectName);
   }

   public class AuthService : IAuthService
   {
      private const string TOKEN_URL = "/basic/auth/token";
      private const string ACCOUNT_URL = "/basic/accounts";

      private IPlatformRequester _requester;

      public AuthService(IPlatformRequester requester)
      {
         _requester = requester;
         _requester.AuthService = this;
      }

      public Promise<User> GetUser()
      {
         return _requester.Request<User>(Method.GET, $"{ACCOUNT_URL}/me", useCache:true);
      }

      // This API call will only work if made by editor code.
      public Promise<User> GetUserForEditor()
      {
         return _requester.Request<User>(Method.GET, $"{ACCOUNT_URL}/admin/me", useCache:true);
      }

      public Promise<User> GetUser(TokenResponse token)
      {
         var tokenizedRequester = _requester.WithAccessToken(token);
         return tokenizedRequester.Request<User>(Method.GET, $"{ACCOUNT_URL}/me", useCache:true);
      }

      public Promise<bool> IsEmailAvailable(string email)
      {
         var encodedEmail = UnityWebRequest.EscapeURL(email);
         return _requester.Request<AvailabilityResponse>(Method.GET, $"{ACCOUNT_URL}/available?email={encodedEmail}", null, false)
            .Map(resp => resp.available);
      }

      public Promise<bool> IsThirdPartyAvailable(AuthThirdParty thirdParty, string token)
      {
         return _requester.Request<AvailabilityResponse>(Method.GET, $"{ACCOUNT_URL}/available/third-party?thirdParty={thirdParty.GetString()}&token={token}", null, false)
            .Map(resp => resp.available);
      }


      public Promise<TokenResponse> CreateUser()
      {
         var form = new WWWForm();
         form.AddField("grant_type", "guest");

         return _requester.RequestForm<TokenResponse>(TOKEN_URL, form, false);
      }

      public Promise<TokenResponse> LoginRefreshToken(string refreshToken)
      {
         var form = new WWWForm();
         form.AddField("grant_type", "refresh_token");
         form.AddField("refresh_token", refreshToken);
         return _requester.RequestForm<TokenResponse>(TOKEN_URL, form, includeAuthHeader: false);
      }

      public Promise<TokenResponse> Login(
         string username,
         string password,
         bool mergeGamerTagToAccount = true,
         bool customerScoped = false
      )
      {
         var body = new LoginPasswordRequest
         {
            username = username,
            password = password,
            customerScoped = customerScoped
         };

         return _requester.RequestJson<TokenResponse>(Method.POST, TOKEN_URL, body, includeAuthHeader: mergeGamerTagToAccount);
      }

      public Promise<TokenResponse> LoginThirdParty(AuthThirdParty thirdParty, string thirdPartyToken, bool includeAuthHeader = true)
      {
         var form = new WWWForm();
         form.AddField("grant_type", "third_party");
         form.AddField("third_party", thirdParty.GetString());
         form.AddField("token", thirdPartyToken);
         return _requester.RequestForm<TokenResponse>(TOKEN_URL, form, includeAuthHeader);
      }

      public Promise<User> RegisterDBCredentials(string email, string password)
      {
         var form = new WWWForm();
         form.AddField("email", email);
         form.AddField("password", password);

         return _requester.RequestForm<User>($"{ACCOUNT_URL}/register", form, Method.POST);
      }

      public Promise<User> RegisterThirdPartyCredentials(AuthThirdParty thirdParty, string accessToken)
      {
         var form = new WWWForm();

         form.AddField("thirdParty", thirdParty.GetString());
         form.AddField("token", accessToken);

         return _requester.RequestForm<User>($"{ACCOUNT_URL}/me", form, Method.PUT);
      }

      public Promise<EmptyResponse> IssueEmailUpdate(string newEmail)
      {
         var form = new WWWForm();
         form.AddField("newEmail", newEmail);

         return _requester.RequestForm<EmptyResponse>($"{ACCOUNT_URL}/email-update/init", form);
      }

      public Promise<EmptyResponse> ConfirmEmailUpdate(string code, string password)
      {
         var form = new WWWForm();
         form.AddField("code", code);
         form.AddField("password", password);

         return _requester.RequestForm<EmptyResponse>($"{ACCOUNT_URL}/email-update/confirm", form);
      }

      public Promise<EmptyResponse> IssuePasswordUpdate(string email)
      {
         var form = new WWWForm();
         form.AddField("email", email);

         return _requester.RequestForm<EmptyResponse>($"{ACCOUNT_URL}/password-update/init", form);
      }

      public Promise<EmptyResponse> ConfirmPasswordUpdate(string code, string newPassword)
      {
         var form = new WWWForm();
         form.AddField("code", code);
         form.AddField("newPassword", newPassword);

         return _requester.RequestForm<EmptyResponse>($"{ACCOUNT_URL}/password-update/confirm", form);
      }

      public Promise<CustomerRegistrationResponse> RegisterDisruptorEngineCustomer(string email, string password, string projectName)
      {
         var request = new CustomerRegistrationRequest(email, password, projectName);
         return _requester.Request<CustomerRegistrationResponse>(Method.POST, "/basic/realms/customer", request, false);
      }

      public Promise<CurrentProjectResponse> GetCurrentProject()
      {
         return _requester.Request<CurrentProjectResponse>(Method.GET, "/basic/realms/project", null, useCache: true);
      }
   }

   public class UserBundle
   {
      public User User;
      public TokenResponse Token;

      public override bool Equals(object obj)
      {
         return Equals(obj as UserBundle);
      }

      public override int GetHashCode()
      {
         return User.id.GetHashCode();
      }

      public bool Equals(UserBundle other)
      {
         if (other == null) return false;

         return other.User.id == User.id;
      }
   }

   [Serializable]
   public class User
   {
      public long id;
      public string email;
      public string language;
      public List<string> scopes;
      public List<string> thirdPartyAppAssociations;
      public bool HasDBCredentials()
      {
         return !string.IsNullOrEmpty(email);
      }

      public bool HasThirdPartyAssociation(AuthThirdParty thirdParty)
      {
         return thirdPartyAppAssociations != null && thirdPartyAppAssociations.Contains(thirdParty.GetString());
      }

      public bool HasAnyCredentials()
      {
         return HasDBCredentials() || (thirdPartyAppAssociations != null && thirdPartyAppAssociations.Count > 0);
      }

      public bool HasScope(string scope)
      {
         return scopes.Contains(scope) || scopes.Contains("*");
      }
   }

   [Serializable]
   public class TokenResponse
   {
      public string access_token;
      public string token_type;
      public long expires_in;
      public string refresh_token;
   }


   [Serializable]
   public class AvailabilityRequest
   {
      public string email;
   }

   [Serializable]
   public class AvailabilityResponse
   {
      public bool available;
   }

   public enum AuthThirdParty
   {
      Facebook,
      Apple,
      Google
   }

   public static class AuthThirdPartyMethods
   {
      public static string GetString(this AuthThirdParty thirdParty)
      {
         switch (thirdParty)
         {
            case AuthThirdParty.Facebook:
               return "facebook";
            case AuthThirdParty.Apple:
               return "apple";
            case AuthThirdParty.Google:
               return "google";
            default:
               return null;
         }
      }
   }

   [System.Serializable]
   public class CustomerRegistrationRequest
   {
      public string email;
      public string password;
      public string projectName;

      public CustomerRegistrationRequest(string email, string password, string projectName)
      {
         this.email = email;
         this.password = password;
         this.projectName = projectName;
      }
   }

   [System.Serializable]
   public class CustomerRegistrationResponse
   {
      public string cid, pid;
      public TokenResponse token;
   }

   public class CurrentProjectResponse
   {
      public string cid, pid, projectName;
   }

   public class LoginPasswordRequest : JsonSerializable.ISerializable
   {
      public string grant_type = "password";
      public string username;
      public string password;
      public bool customerScoped;
      public void Serialize(JsonSerializable.IStreamSerializer s)
      {
         s.Serialize("grant_type", ref grant_type);
         s.Serialize("username", ref username);
         s.Serialize("password", ref password);
         s.Serialize("customerScoped", ref customerScoped);
      }
   }
}
