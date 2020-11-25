using System;
using System.Linq;
using Beamable.Content;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beamable.Editor.Content.UI
{
   [CanEditMultipleObjects]
   [CustomEditor(typeof(ContentObject), true)]
   public class ContentObjectEditor : UnityEditor.Editor
   {
      protected override void OnHeaderGUI()
      {
         base.OnHeaderGUI();
         var leftMargin = 45;
         var rightMargin = 56;
         Rect lastRect = GUILayoutUtility.GetLastRect();
         Rect r = new Rect(lastRect.x + leftMargin, lastRect.height - 25, lastRect.width - (leftMargin + rightMargin),
            20);


         var contentObject = target as ContentObject;
         if (contentObject == null) return;

         EditorGUI.BeginChangeCheck();

         var oldFieldWith = EditorGUIUtility.labelWidth;


         var value = GetTagString(contentObject.Tags);
         if (targets.Length > 1)
         {
            for (var i = 0; i < targets.Length; i++)
            {
               var otherContentObject = targets[i] as ContentObject;
               if (otherContentObject == null) continue;
               var otherValue = GetTagString(otherContentObject.Tags);
               if (otherValue != value)
               {
                  value = "-";
                  break;
               }
            }
         }

         EditorGUIUtility.labelWidth = 75;
         var edit = EditorGUI.TextField(r, "Content Tag", value);
         EditorGUIUtility.labelWidth = oldFieldWith;

         if (EditorGUI.EndChangeCheck())
         {
            var tags = GetTagsFromString(edit);
            Undo.RecordObjects(targets, "Change Content Tag");
            foreach (Object obj in targets)
            {
               var otherContentObject = obj as ContentObject;
               if (otherContentObject != null)
               {
                  otherContentObject.Tags = tags.ToArray(); // copy.

                  otherContentObject.ForceValidate();
               }
            }
         }



      }

      public string GetTagString(string[] tags)
      {
         return string.Join(" ", tags);
      }

      public string[] GetTagsFromString(string tagString)
      {
         return tagString?.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
      }
   }
}