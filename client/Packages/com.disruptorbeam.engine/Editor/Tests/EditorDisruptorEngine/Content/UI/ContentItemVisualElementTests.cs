using System.Collections;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Editor.Content.UI.ProjectView;
using Beamable.Platform.SDK;
using Beamable.Content;
using NUnit.Framework;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#endif
using UnityEngine.TestTools;

namespace Beamable.Editor.Tests.EditorDisruptorEngine.Content.UI
{
   public class ContentItemVisualElementTests
   {

      private ExampleContent _content;
      private ContentItemVisualElement<ExampleContent> _element;
      private string _contentName = "test";

      private Label Label => _element.Q<Label>();
      private TextField Text => _element.Q<TextField>();


      [SetUp]
      public void Init()
      {
         _contentName = "test";
         _content = ScriptableObject.CreateInstance<ExampleContent>();
         _content.SetContentName(_contentName);
         _element = new ContentItemVisualElement<ExampleContent>();
         _element.Content = _content;
         _element.Refresh();
      }

      [Test]
      public void ShowsLabelByDefault()
      {
         Assert.IsFalse(Label.ClassListContains("hide"));
         Assert.IsTrue(Text.ClassListContains("hide"));

      }

      [Test]
      public void ShowsTextfieldWhenNaming()
      {
         _element.BeginNaming();
         Assert.IsTrue(Label.ClassListContains("hide"));
         Assert.IsFalse(Text.ClassListContains("hide"));
      }

      [Test]
      public void TextIsNameByDefault()
      {
         Assert.AreEqual(Label.text, _contentName);
         Assert.AreEqual(Text.value, _contentName);
      }

      [Test]
      public void LabelReactsToContentEvent()
      {
         var nextName = "changedName";
         _content.SetContentName(nextName);
         _content.BroadcastUpdate();

         Assert.AreEqual(Label.text, nextName);
      }

//      [UnityTest]
//      public IEnumerator NamingResolvesOnEnterKey()
//      {
//         //var promise = _element.BeginNaming();
//
//         var evt = KeyDownEvent.GetPooled('\n', KeyCode.Return, EventModifiers.None);
//         Text.HandleEvent(evt);
//
////         yield return AsIEnumeratorReturnNull(promise);
////
////         Assert.IsTrue(promise.IsCompleted);
////
//         yield return null;
//      }

      public static IEnumerator AsIEnumeratorReturnNull<T>(Promise<T> promise)
      {
         while (!promise.IsCompleted)
         {
            yield return null;
         }

         yield return null;
      }
   }
}