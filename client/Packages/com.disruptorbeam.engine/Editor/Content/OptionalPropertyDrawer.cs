using System;
using Beamable.Common.Content;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Content
{
  [CustomPropertyDrawer(typeof(Optional), true)]
   public class OptionalPropertyDrawer : PropertyDrawer
   {
      public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
      {
         var hasValueProp = property.FindPropertyRelative(nameof(Optional.HasValue));
         var valueProp = property.FindPropertyRelative("Value");
         if (hasValueProp.boolValue)
         {
            return EditorGUI.GetPropertyHeight(valueProp);
         }
         else
         {
            return EditorGUIUtility.singleLineHeight;
         }
      }

      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {
         var hasValueProp = property.FindPropertyRelative(nameof(Optional.HasValue));
         var valueProp = property.FindPropertyRelative("Value");
         var fieldRect = new Rect(position.x, position.y, position.width - 15, position.height);

         var x = EditorGUI.indentLevel * 15;
         var checkRect = new Rect(position.x + position.width - (15 + x), position.y, 60, 15);


         if (property.isExpanded && !property.isArray)
         {
            property.isExpanded = false;
         }

         EditorGUI.BeginProperty(position, label, property);

         if (hasValueProp.boolValue)
         {
            EditorGUI.PropertyField(fieldRect, valueProp, label, true);
         }
         else
         {
            var wasEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.TextField(fieldRect,  label, "optional value not set");

            GUI.enabled = wasEnabled;
         }

         EditorGUI.BeginChangeCheck();
         var next = EditorGUI.Toggle(checkRect, hasValueProp.boolValue);
         if (EditorGUI.EndChangeCheck())
         {
            hasValueProp.boolValue = next;
         }

         EditorGUI.EndProperty();

      }
   }
}