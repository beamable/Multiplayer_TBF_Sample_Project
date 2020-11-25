using System;
using System.Collections.Generic;
using System.IO;
using Beamable.Editor.UI.Components;
using UnityEditor;

#if UNITY_2018
namespace UnityEngine.Experimental.UIElements
{
    using StyleSheets;

    public static class UIElementsPolyfill2018
    {

      public static void AddSplitPane(this VisualElement self, VisualElement left, VisualElement right)
      {
        var splitterElem = new SplitterVisualElement(){name="splitter"};

        var leftWrapper = new VisualElement();
        leftWrapper.AddToClassList("splitWrapper");
        leftWrapper.AddToClassList("leftSplit");
        var rightWrapper = new VisualElement();
        rightWrapper.AddToClassList("splitWrapper");
        rightWrapper.AddToClassList("rightSplit");
        leftWrapper.Add(left);
        rightWrapper.Add(right);

        splitterElem.Add(leftWrapper);
        splitterElem.Add(rightWrapper);

        self.Add(splitterElem);
      }

        public static void AddTextWrapStyle(this VisualElement self)
        {
          self.style.wordWrap = true;
        }

        public static VisualElement CloneTree(this VisualTreeAsset self)
        {
            return self.CloneTree(null);
        }

        public static void AddStyleSheet(this VisualElement self, string path)
        {
          var paths = UssLoader.GetAvailableSheetPaths(path);
          foreach (var ussPath in paths)
          {
            self.AddStyleSheetPath(ussPath);
          }
        }

        public static void RemoveStyleSheet(this VisualElement self, string path)
        {
          self.RemoveStyleSheetPath(path);
        }

        public static void SetRight(this IStyle self, float value)
        {
            self.positionRight = value;
        }

        public static float GetMaxHeight(this IStyle self)
        {
            return self.maxHeight;
        }

        public static void SetImage(this Image self, Texture texture)
        {
            self.image = StyleValue<Texture>.Create(texture);
        }

        public static void BeamableFocus(this TextField self)
        {
            self.Focus();
        }

        public static void BeamableAppendAction(this DropdownMenu self, string title, Action<Vector2> callback)
        {
          self.AppendAction(title, evt => callback(evt.eventInfo.mousePosition), DropdownMenu.MenuAction.AlwaysEnabled);
        }

        public static bool RegisterValueChangedCallback<T>(
          this INotifyValueChanged<T> control,
          EventCallback<ChangeEvent<T>> callback)
        {
          CallbackEventHandler callbackEventHandler = control as CallbackEventHandler;
          if (callbackEventHandler == null)
            return false;
          callbackEventHandler.RegisterCallback<ChangeEvent<T>>(callback, TrickleDown.NoTrickleDown);
          return true;
        }

        public static bool UnregisterValueChangedCallback<T>(
          this INotifyValueChanged<T> control,
          EventCallback<ChangeEvent<T>> callback)
        {
          CallbackEventHandler callbackEventHandler = control as CallbackEventHandler;
          if (callbackEventHandler == null)
            return false;
          callbackEventHandler.UnregisterCallback<ChangeEvent<T>>(callback, TrickleDown.NoTrickleDown);
          return true;
        }
    }
}
#endif

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;

namespace UnityEditor
{
  public static class UnityEditorPolyfill
  {
    public static VisualElement GetRootVisualContainer(this EditorWindow self)
    {
      return self.rootVisualElement;
    }
  }
}

namespace UnityEngine.UIElements
{

  public static class UIElementsPolyfill2019
  {

    public static void AddSplitPane(this VisualElement self, VisualElement left, VisualElement right) {

      var splitterElem = new SplitterVisualElement(){name="splitter"};

      var leftWrapper = new VisualElement();
      leftWrapper.AddToClassList("splitWrapper");
      leftWrapper.AddToClassList("leftSplit");
      var rightWrapper = new VisualElement();
      rightWrapper.AddToClassList("splitWrapper");
      rightWrapper.AddToClassList("rightSplit");
      leftWrapper.Add(left);
      rightWrapper.Add(right);

      splitterElem.Add(leftWrapper);
      splitterElem.Add(rightWrapper);

      self.Add(splitterElem);

//      self.Add(left);
//      self.Add(right);


    }



    public static void AddTextWrapStyle(this VisualElement self)
    {
      self.style.whiteSpace = WhiteSpace.Normal;
    }
    public static void AddStyleSheet(this VisualElement self, string path)
    {
      var paths = UssLoader.GetAvailableSheetPaths(path);
      foreach (var ussPath in paths)
      {
        self.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath));
        //self.AddStyleSheetPath(ussPath);
      }
    }

    public static void RemoveStyleSheet(this VisualElement self, string path)
    {
      self.styleSheets.Remove(AssetDatabase.LoadAssetAtPath<StyleSheet>(path));
    }
    public static void SetRight(this IStyle self, float value)
    {
      self.right = new StyleLength(value);
    }

    public static float GetMaxHeight(this IStyle self)
    {
      return self.maxHeight.value.value;
    }

    public static void SetImage(this Image self, Texture texture)
    {
      self.image = texture;
    }

    public static void BeamableFocus(this TextField self)
    {
      self.Q("unity-text-input").Focus();
    }

    public static void BeamableAppendAction(this DropdownMenu self, string title, Action<Vector2> callback)
    {
      self.AppendAction(title, evt => callback(evt.eventInfo.mousePosition));
    }
  }
}
#endif


public static class UssLoader
{
  public static List<string> GetAvailableSheetPaths(string ussPath)
  {
    var ussPaths = new List<string> {ussPath};

    var darkPath = ussPath.Replace(".uss", ".dark.uss");
    var lightPath = ussPath.Replace(".uss", ".light.uss");
    var darkAvailable = File.Exists(darkPath);
    var lightAvailable = File.Exists(lightPath);

    if (EditorGUIUtility.isProSkin && darkAvailable)
    {
      ussPaths.Add(darkPath);
    } else if (!EditorGUIUtility.isProSkin && lightAvailable)
    {
      ussPaths.Add(lightPath);
    }
    return ussPaths;
  }
}
