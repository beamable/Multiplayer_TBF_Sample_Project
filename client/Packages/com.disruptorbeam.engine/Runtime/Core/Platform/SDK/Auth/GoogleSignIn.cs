using System;
using UnityEngine;

namespace Beamable.Platform.SDK.Auth
{
   public class GoogleSignIn
   {
      private readonly GameObject _target;
      private readonly string _callbackMethod;
      private readonly string _clientId;

      /// <summary>
      /// Google Sign-In harness. Because the Android plugin needs to use
      /// UnitySendMessage to call back, we need to know the GameObject and
      /// callback method name.
      /// </summary>
      /// <param name="target">GameObject to use for callback</param>
      /// <param name="callbackMethod">Name of the method to call back</param>
      /// <param name="clientId">The Google client-id of the authentication app</param>
      public GoogleSignIn(GameObject target, string callbackMethod, string clientId)
      {
         _target = target;
         _callbackMethod = callbackMethod;
         _clientId = clientId;
      }

      /// <summary>
      /// Initiate login using the Android native plugin. When complete, the
      /// plugin will call back to the GameObject specified in the constructor.
      /// </summary>
      public void Login()
      {
#if UNITY_ANDROID
         var login = new AndroidJavaClass("com.beamable.googlesignin.GoogleSignInActivity");
         login.CallStatic("login", _target.name, _callbackMethod, _clientId);
#else
         Debug.LogError("Google Sign-In is only available on Android");
#endif // UNITY_ANDROID
      }

      /// <summary>
      /// Unpack the response from the Google Sign-In plugin. Call this from
      /// the GameObject callback.
      /// </summary>
      /// <param name="message">Response message from the plugin</param>
      /// <param name="callback">Callback to be invoked when the result is complete</param>
      /// <param name="errback">Callback to call if authentication failed</param>
      public static void HandleResponse(string message, Action<string> callback, Action<GoogleInvalidTokenException> errback)
      {
         if (message.StartsWith("CANCELED"))
         {
            callback.Invoke(null);
         }
         else if (message.StartsWith("EXCEPTION") || message.StartsWith("UNKNOWN"))
         {
            errback.Invoke(new GoogleInvalidTokenException(message));
         }
         else
         {
            callback.Invoke(message);
         }
      }
   }

   public class GoogleInvalidTokenException : Exception
   {
      public GoogleInvalidTokenException(string message) : base(message) {}
   }
}
