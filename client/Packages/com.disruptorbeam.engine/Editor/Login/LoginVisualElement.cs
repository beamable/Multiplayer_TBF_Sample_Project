using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#endif


namespace Beamable.Editor.Login
{
   delegate void LoginAttemptEvent(string email, string password);
   delegate void RegisterAttemptEvent(string email, string password);

   class LoginVisualElement : VisualElement
   {
      private VisualElement _root;
      private Button _loginButton;
      private Button _registerButton;
      private TextField _emailField;
      private TextField _passwordField;

      private const string Asset_UXML_Login =
         "Packages/com.disruptorbeam.engine/Editor/Login/login.uxml";

      private const string Asset_USS_Login =
         "Packages/com.disruptorbeam.engine/Editor/Login/login.uss";

      public event LoginAttemptEvent OnLogin = (e, p) => { };
      public event RegisterAttemptEvent OnRegister = (e, p) => { };

      public LoginVisualElement()
      {
         var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Asset_UXML_Login);
         _root = treeAsset.CloneTree();
         this.AddStyleSheet(Asset_USS_Login);
         _emailField = _root.Q<TextField>(className: "login-email");
         _passwordField = _root.Q<TextField>(className: "login-password");
         _loginButton = _root.Q<Button>(className: "login-button");
         _registerButton = _root.Q<Button>(className: "register-button");
         _passwordField.isPasswordField = true;


         Add(_root);
         _root.RegisterCallback<KeyDownEvent>(evt => { if (KeyCode.Return == evt.keyCode) AttemptLogin(); });
         _passwordField.RegisterCallback<KeyDownEvent>(evt => { if (KeyCode.Return == evt.keyCode) AttemptLogin(); });
         _emailField.RegisterCallback<KeyDownEvent>(evt => { if (KeyCode.Return == evt.keyCode) AttemptLogin(); });

         _loginButton.clickable.clicked += AttemptLogin;
         _registerButton.clickable.clicked += AttemptRegister;
      }

      void AttemptRegister()
      {
         if (!_registerButton.enabledInHierarchy || !Validate())
         {
            return;
         }
         ClearError();
         OnRegister?.Invoke(_emailField.value, _passwordField.value);
      }

      void AttemptLogin()
      {
         if (!_loginButton.enabledInHierarchy || !Validate())
         {
            return;
         }
         ClearError();
         OnLogin?.Invoke(_emailField.value, _passwordField.value);
      }

      private bool Validate()
      {
         if (string.IsNullOrEmpty(_emailField.value))
         {
            ShowError("Must provide an email");
            return false;
         }

         if (string.IsNullOrEmpty(_passwordField.value))
         {
            ShowError("Must provide a password");
            return false;
         }

         return true;
      }

      public void BlockInput()
      {
         _loginButton.SetEnabled(false);
         _registerButton.SetEnabled(false);
      }

      public void UnblockInput()
      {
         _loginButton.SetEnabled(true);
         _registerButton.SetEnabled(true);
      }

      public void ShowError(string errorMessage)
      {
         _root.Q<Label>(className: "login-error").text = errorMessage;
      }

      public void ClearError()
      {
         ShowError("");
      }
   }
}