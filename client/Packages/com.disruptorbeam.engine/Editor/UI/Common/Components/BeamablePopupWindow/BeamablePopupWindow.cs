using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Editor.UI.Buss.Components;
using Beamable.UI.Buss;
using UnityEditor;
using UnityEngine;
using Beamable.Editor.Content.Components;
using Beamable.Editor.Content;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.UI.Buss.Components
{
   public class BeamablePopupWindow : EditorWindow
   {
      public static Vector2 ConfirmationPopupSize = new Vector2(250, 120);

      /// <summary>
      /// Create screen-relative, parent <see cref="VisualElement"/>-relative
      /// <see cref="Rect"/> for new <see cref="BeamablePopupWindow"/>
      /// </summary>
      /// <param name="visualElementBounds"></param>
      public static Rect GetLowerLeftOfBounds(Rect visualElementBounds)
      {
         var newWindowPosition = new Vector2(visualElementBounds.xMin, visualElementBounds.yMax);
         newWindowPosition = GUIUtility.GUIToScreenPoint(newWindowPosition);
         return new Rect(newWindowPosition.x, newWindowPosition.y, 0, 0);
      }

      /// <summary>
      /// Create CENTERED screen-relative, parent <see cref="VisualElement"/>-relative
      /// <see cref="Rect"/> for new <see cref="ConfirmationPopupVisualElement"/>
      /// </summary>
      /// <param name="visualElementBounds"></param>
      public static Rect GetCenteredScreenRectFromWorldBounds(Rect visualElementBounds, Vector2 newWindowSize)
      {
         //Get relative position
         //TODO: Make this truely sit in the dead center of the window - WIP - srivello
         var newWindowPosition = new Vector2(visualElementBounds.center.x, 0);
         newWindowPosition = GUIUtility.GUIToScreenPoint(newWindowPosition);

         //Adjust by absolute size
         newWindowPosition.x -= newWindowSize.x/2;

         return new Rect(newWindowPosition.x, newWindowPosition.y, newWindowSize.x, newWindowSize.y);
      }

      /// <summary>
      /// Create new popup with contents of any <see cref="BeamableVisualElement"/>
      /// </summary>
      /// <param name="title"></param>
      /// <param name="sourceRect"></param>
      /// <param name="size"></param>
      /// <param name="content"></param>
      /// <returns></returns>
      public static BeamablePopupWindow ShowDropdown(string title, Rect sourceRect, Vector2 size, BeamableVisualElement content)
      {
         var wnd = CreateInstance<BeamablePopupWindow>();
         wnd.titleContent = new GUIContent(title);
         wnd.ContentElement = content;
         wnd.ShowAsDropDown(sourceRect, size);
         wnd.Refresh();
         return wnd;
      }

      /// <summary>
      /// Create new popup with contents of <see cref="ConfirmationPopupVisualElement"/>.
      /// Useful for an "Are you Sure?" user experience.
      /// </summary>
      /// <param name="popupWindowRect"></param>
      /// <returns></returns>
      public static BeamablePopupWindow ShowConfirmationPopup(VisualElement centerWithMeVisualElement)
      {
         //Find the world bounds of the root of the window. Center the Popup within
         Rect popupWindowRect = BeamablePopupWindow.GetCenteredScreenRectFromWorldBounds(
            centerWithMeVisualElement.worldBound, ConfirmationPopupSize);

         var confirmationPopupVisualElement = new ConfirmationPopupVisualElement();
         var wnd = BeamablePopupWindow.ShowDropdown("Confirmation",
            popupWindowRect, popupWindowRect.size, confirmationPopupVisualElement);
         return wnd;
      }

      public static BeamablePopupWindow ShowUtility(string title, BeamableVisualElement content, EditorWindow parent)
      {
         var wnd = CreateInstance<BeamablePopupWindow>();
         wnd.titleContent = new GUIContent(title);
         wnd.ContentElement = content;

         wnd.ShowUtility();
         // TODO: Somehow position the utility based on the parent view.
         wnd.Refresh();
         wnd.GetRootVisualContainer().AddToClassList("fill-popup-window");
         return wnd;
      }

      public BeamableVisualElement ContentElement;
      private VisualElement _windowRoot;
      private VisualElement _contentRoot;
      private VisualElement _container;

      private void OnEnable()
      {
         VisualElement root = this.GetRootVisualContainer();
         var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{BeamableComponentsConstants.COMP_PATH}/BeamablePopupWindow/beamablePopupWindow.uxml");
         _windowRoot = uiAsset.CloneTree();
         this.GetRootVisualContainer().AddStyleSheet($"{BeamableComponentsConstants.COMP_PATH}/BeamablePopupWindow/beamablePopupWindow.uss");
         _windowRoot.name = nameof(_windowRoot);

         root.Add(_windowRoot);
      }

      public void SwapContent(BeamableVisualElement other)
      {
         ContentElement = other;
         Refresh();

      }

      public void Refresh()
      {
         _container = _windowRoot.Q<VisualElement>("container");
         _container.Clear();
         _container.Add(ContentElement);
         ContentElement.Refresh();
         Repaint();
      }
   }
}