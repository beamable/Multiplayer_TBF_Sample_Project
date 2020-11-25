using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace Beamable.Editor.Register
{
   delegate void RegisterAttemptEvent(string email, string password, string projectName);

   class RegisterVisualElement : VisualElement
   {
      private VisualElement _root;
      private Button _button;
      private TextField _emailField;
      private TextField _passwordField;
      private TextField _projectField;

      private const string Asset_UXML_Register =
         "Packages/com.disruptorbeam.engine/Editor/Register/register.uxml";

      private const string Asset_USS_Register =
         "Packages/com.disruptorbeam.engine/Editor/Register/register.uss";

      public event RegisterAttemptEvent OnRegisterAttemped = (e, p, n) => { };

      public RegisterVisualElement()
      {
         var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Asset_UXML_Register);
         _root = treeAsset.CloneTree();
         _root.AddStyleSheet(Asset_USS_Register);
         _emailField = _root.Q<TextField>(className: "register-email");
         _passwordField = _root.Q<TextField>(className: "register-password");
         _projectField = _root.Q<TextField>(className: "register-project");
         _button = _root.Q<Button>();
         _passwordField.isPasswordField = true;


         Add(_root);
         _root.RegisterCallback<KeyDownEvent>(evt => { if (KeyCode.Return == evt.keyCode) AttemptLogin(); });
         _passwordField.RegisterCallback<KeyDownEvent>(evt => { if (KeyCode.Return == evt.keyCode) AttemptLogin(); });
         _emailField.RegisterCallback<KeyDownEvent>(evt => { if (KeyCode.Return == evt.keyCode) AttemptLogin(); });
         _projectField.RegisterCallback<KeyDownEvent>(evt => { if (KeyCode.Return == evt.keyCode) AttemptLogin(); });

         _button.clickable.clicked += AttemptLogin;
      }

      void AttemptLogin()
      {
         if (!_button.enabledInHierarchy)
         {
            return;
         }

         var email = _emailField.value;
         var password = _passwordField.value;
         var project = _projectField.value;

         var missingFields = new List<string>();
         if (string.IsNullOrEmpty(email))
         {
            missingFields.Add("email");
         }
         if (string.IsNullOrEmpty(password))
         {
            missingFields.Add("project");
         }
         if (string.IsNullOrEmpty(project))
         {
            missingFields.Add("project name");
         }

         if (missingFields.Count > 0)
         {
            ShowError($"Must provide {string.Join(", ", missingFields)}");
            return;
         }

         ClearError();
         OnRegisterAttemped?.Invoke(email, password, project);
      }

      public void BlockInput()
      {
         _button.SetEnabled(false);
      }

      public void UnblockInput()
      {
         _button.SetEnabled(true);
      }

      public void ShowError(string errorMessage)
      {
         _root.Q<Label>(className: "register-error").text = errorMessage;
      }

      public void ClearError()
      {
         ShowError("");
      }
   }
}