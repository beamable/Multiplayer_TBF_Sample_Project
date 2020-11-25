using System;
using System.Collections.Generic;
using Beamable.Editor.Login;
using Beamable.Editor.Register;
using Beamable.Editor.Toolbox;
using Beamable.Modules.Avatars;
using Beamable.Platform.SDK.Auth;
using Beamable.Editor.Content;
using Beamable.Modules.AccountManagement;
using UnityEngine;
using UnityEditor;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor
{
   class DisruptorEngineWindow : EditorWindow
   {
      private VisualElement _windowRoot;
      private VisualElement _contentRoot;
      private EditorAPI _de;
      private readonly Vector2 windowMax = new Vector2(800, 750);
      private readonly Vector2 windowMin = new Vector2(500, 200);

      private List<IEnumerator<int>> _enumerators = new List<IEnumerator<int>>();
      private Button _logoutButton;

      [MenuItem(
         ContentConstants.MENU_ITEM_PATH_WINDOW_BEAMABLE + "/" +
         ContentConstants.OPEN + " " +
         ContentConstants.TOOLBOX,
         priority = ContentConstants.MENU_ITEM_PATH_WINDOW_PRIORITY_1)]
      public static DisruptorEngineWindow Init()
      {
         var hierarchy = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
         var inspector = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");

         DisruptorEngineWindow wnd = GetWindow<DisruptorEngineWindow>(ContentConstants.TOOLBOX, true, inspector, hierarchy);
         return wnd;
      }

      public async void OnEnable()
      {
         this.maxSize = windowMax;
         this.minSize = windowMin;
         VisualElement root = this.GetRootVisualContainer();
         var uiAsset =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.disruptorbeam.engine/Editor/window.uxml");
         _windowRoot = uiAsset.CloneTree();
         _windowRoot.AddStyleSheet("Packages/com.disruptorbeam.engine/Editor/window.uss");
         _windowRoot.name = nameof(_windowRoot);
         root.Add(_windowRoot);

         _de = await EditorAPI.Instance;
         _contentRoot = _windowRoot.Q("de-window-content");

         _logoutButton = root.Q<Button>(className: "de-window-logout");
         _logoutButton.clickable.clicked += () =>
         {
            _de.Logout();
            RefreshContent();
         };

         RefreshContent();
      }
      void Update()
      {
         // handle any animations that need to play out.
         var completed = new List<IEnumerator<int>>();
         _enumerators.ForEach(e =>
         {
            var hasNext = e.MoveNext();
            if (!hasNext)
            {
               completed.Add(e);
            }
         });
         completed.ForEach(e => _enumerators.Remove(e));
      }

      public void RefreshContent()
      {
         var avatars = AvatarConfiguration.Instance;

         var content = CreateContentElement();
         _contentRoot.Clear();
         _contentRoot.Add(content);

         string email = "";

         if (!string.IsNullOrEmpty(_de.Cid))
         {
            _de.AuthService.GetCurrentProject().Then(SetProjectName);
         }

         if (_de.User != null)
         {
            email = _de.User.email;
            _logoutButton.RemoveFromClassList("hide");
         }
         else
         {
            _logoutButton.AddToClassList("hide");
         }

         _windowRoot.Q<Label>(className: "lblEmail").text = email;
      }

      void SetProjectName(CurrentProjectResponse name)
      {
         _windowRoot.Q<Label>(className: "lblProject").text = name.projectName;
      }

      VisualElement CreateContentElement()
      {
         // before we can use DE, we need to show an agreement screen, and confirmation of use (which will prepare the project for assets and configuration)
         if (!_de.HasDependencies())
         {
            return CreateRequirements();
         }

         // if we have a cid, we have an org, if no cid, then we *MUST* create an org...
         if (string.IsNullOrEmpty(_de.Cid))
         {
            return CreateRegistration();
         }

         // if we have a token, we are logged in already. If no token, then we need to get one...
         if (_de.Token == null)
         {
            return CreateLogin();
         }

         // the standard flow is to show the toolbox
         return CreateToolbox();
      }

      VisualElement CreateRequirements()
      {
         var panel = new VisualElement();

         var textPanel = new VisualElement();
         var text = new TextElement
         {
            text = "Welcome to Disruptor Engine! Before we get started, please be aware that this package relies on Addressables, and TextMeshPro. By clicking the 'Accept' button below, you agree to let Disruptor Engine install those packages to your project. Thanks!"
         };
         textPanel.AddToClassList("agree-text");
         text.AddTextWrapStyle();

         textPanel.Add(text);

         panel.Add(textPanel);

         var button = new Button(() =>
         {
            _de.CreateDependencies().Then(_ =>
            {
               _de.ContentIO.EnsureAllDefaultContent();
               RefreshContent();
            });
         });
         button.text = "Accept";
         panel.Add(button);
         return panel;
      }

      LoginVisualElement CreateLogin()
      {
         var login = new LoginVisualElement();

         login.OnLogin += async (email, password) =>
         {
            try
            {
               login.BlockInput();
               var response = await _de.AuthService.Login(email, password, customerScoped: true);
               await _de.ApplyToken(response);
               RefreshContent();
            }
            catch (Exception ex)
            {
               login.ShowError(ex.Message);
            }
            finally
            {
               login.UnblockInput();
            }
         };

         login.OnRegister += async (email, password) =>
         {
            try
            {
               login.BlockInput();
               var newToken = await _de.AuthService.CreateUser();
               await _de.ApplyToken(newToken);
               await _de.AuthService.RegisterDBCredentials(email, password);
               RefreshContent();
            }
            catch (Exception ex)
            {
               login.ShowError(ex.Message);
               _de.Logout();
            }
            finally
            {
               login.UnblockInput();
            }
         };


         return login;
      }

      RegisterVisualElement CreateRegistration()
      {
         var registration = new RegisterVisualElement();
         registration.OnRegisterAttemped += async (email, password, project) =>
         {
            try
            {
               registration.BlockInput();
               var rsp = await _de.AuthService.RegisterDisruptorEngineCustomer(email, password, project);
               _de.SaveConfig(rsp.cid, rsp.pid);
               await _de.ApplyToken(rsp.token);

               await _de.ContentPublisher.ClearManifest();
               await _de.ContentPublisher.CreatePublishSet().FlatMap(set => _de.ContentPublisher.Publish(set, progress => { }));
               await _de.ContentIO.FetchManifest();

               RefreshContent();
            }
            catch (Exception ex)
            {
               Debug.LogError(ex);
               registration.ShowError(ex.Message);
            }
            finally
            {
               registration.UnblockInput();
            }
         };
         return registration;
      }


      ToolboxVisualElement CreateToolbox()
      {
         var toolbox = new ToolboxVisualElement();
         toolbox.OnAnimationRequested += a => _enumerators.Add(a);
         return toolbox;
      }

   }
}