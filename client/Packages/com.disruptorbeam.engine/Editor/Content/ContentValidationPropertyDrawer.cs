using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Beamable.Common.Content.Validation;
using Beamable.Content;
using Beamable.Content.Validation;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Content
{

   [CustomPropertyDrawer(typeof(ValidationAttribute), true)]
   public class ContentValidationPropertyDrawer : PropertyDrawer
   {
      private GUIStyle _lblStyle;
      private const int WIDTH = 3;
      private const int OFFSET = -10;


      public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
      {
         var baseHeight = EditorGUI.GetPropertyHeight(property, label);

         if (property.serializedObject.isEditingMultipleObjects || !EditorAPI.Instance.IsCompleted)
         {
            return baseHeight;
         }
         var ctx = EditorAPI.Instance.GetResult().ContentIO.GetValidationContext();

         var attributes = fieldInfo.GetCustomAttributes<ValidationAttribute>();
         var contentObj = property.serializedObject.targetObject as ContentObject;

         var exceptions = new List<ContentException>();
         foreach (var attribute in attributes)
         {
            try
            {
               var value = ContentRefPropertyDrawer.GetTargetParentObjectOfProperty(property);
               var wrapper = new ValidationFieldWrapper(fieldInfo, value);
               attribute.Validate(wrapper, contentObj, ctx);
            }
            catch (ContentException ex)
            {
               exceptions.Add(ex);
            }
         }

         return baseHeight + EditorGUIUtility.singleLineHeight * exceptions.Count;
      }

      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {

         if (property.serializedObject.isEditingMultipleObjects || !EditorAPI.Instance.IsCompleted)
         {
            RefEditorGUI.DefaultPropertyField(position, property, label);
            return; // don't support multiple edit.
         }


         var ctx = EditorAPI.Instance.GetResult().ContentIO.GetValidationContext();

         var attributes = fieldInfo.GetCustomAttributes<ValidationAttribute>();
         var contentObj = property.serializedObject.targetObject as ContentObject;

         var exceptions = new List<ContentException>();
         foreach (var attribute in attributes)
         {
            try
            {
               var value = ContentRefPropertyDrawer.GetTargetParentObjectOfProperty(property);
               var wrapper = new ValidationFieldWrapper(fieldInfo, value);
               attribute.Validate(wrapper, contentObj, ctx);
            }
            catch (ContentException ex)
            {
               exceptions.Add(ex);
            }
         }

         RefEditorGUI.DefaultPropertyField(position, property, label);

         if (exceptions.Count > 0){

            var maxY = 0f;
            if (_lblStyle == null)
            {
               _lblStyle = new GUIStyle(GUI.skin.label);
               _lblStyle.fontSize = (int) (_lblStyle.fontSize * .7f);
               _lblStyle.normal.textColor = Color.red;
               _lblStyle.hover.textColor = Color.red;
            }

            for(var i = 0 ; i < exceptions.Count; i ++)
            {
               var ex = exceptions[i];
               var content = new GUIContent($"  {ex.FriendlyMessage}");
               EditorGUI.LabelField(new Rect(position.x,position.y + position.height + EditorGUIUtility.singleLineHeight*(i-1), position.width, EditorGUIUtility.singleLineHeight), content, _lblStyle);
               //EditorGUILayout.LabelField(content, _lblStyle);

               //maxY += _lblStyle.CalcSize(content).y;
            }
            var errRect = new Rect(position.x - WIDTH + OFFSET, position.y - 1, WIDTH, position.height + maxY + 2);

            EditorGUI.DrawRect(errRect, Color.red);
           // EditorGUI.
         }

//         EditorGUI.EndProperty();
      }
   }

   public static class RefEditorGUI
   {
      public delegate bool DefaultPropertyFieldDelegate(Rect position, SerializedProperty property, GUIContent label);

      private static Dictionary<Type, Type> _fieldTypeToDrawerType;
      private static Type[] _propertyDrawerTypes;
      public static DefaultPropertyFieldDelegate DefaultPropertyField;
      public static DefaultPropertyFieldDelegate VanillaPropertyField;
      static RefEditorGUI()
      {
         var asmName = typeof(PropertyDrawer).AssemblyQualifiedName;
         var t2 = Type.GetType(asmName.Replace("UnityEditor.PropertyDrawer", "UnityEditor.EditorAssemblies"));
         var subClassMethod = t2.GetMethod("SubclassesOf", BindingFlags.Static | BindingFlags.NonPublic);

         var propertyDrawerTypesObj = subClassMethod?.Invoke(null, new object[] {typeof(PropertyDrawer)});
         _propertyDrawerTypes = propertyDrawerTypesObj as Type[];

         var t = typeof(EditorGUI);
         var delegateType = typeof(DefaultPropertyFieldDelegate);
         var m = t.GetMethod("DefaultPropertyField", BindingFlags.Static | BindingFlags.NonPublic);
         VanillaPropertyField = (DefaultPropertyFieldDelegate)System.Delegate.CreateDelegate(delegateType, m);

         _fieldTypeToDrawerType = new Dictionary<Type, Type>();
         DefaultPropertyField = (position, property, label) =>
         {
            var parentType = property.serializedObject.targetObject.GetType();
            var field = parentType.GetField(property.propertyPath);

            var fieldType = GetPropertyType(property);
            if (!_fieldTypeToDrawerType.ContainsKey(fieldType))
            {
               var drawerType = GetPropertyDrawerType(fieldType);
               _fieldTypeToDrawerType.Add(fieldType, drawerType);
            }

            var foundDrawerType = _fieldTypeToDrawerType[fieldType];
            if (foundDrawerType == null)
            {
               EditorGUI.BeginProperty(position, label, property);
               EditorGUI.PropertyField(position, property, label, true);
               EditorGUI.EndProperty();
            }
            else
            {
               var instance = (PropertyDrawer) Activator.CreateInstance(foundDrawerType);
               instance.OnGUI(position, property, label);
            }
            return true;
         };

      }

      static Type GetPropertyType(SerializedProperty prop)
      {
         //gets parent type info
         string[] slices = prop.propertyPath.Split('.');
         System.Type type = prop.serializedObject.targetObject.GetType();

         for (int i = 0; i < slices.Length; i++)
         {
            if (slices[i] == "Array")
            {
               i++; //skips "data[x]"
               type = type.GetElementType() ?? type.GetGenericArguments()[0]; //gets info on array elements
            }

            //gets info on field and its type
            else
            {
               type = type.GetField(slices[i],
                     BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy |
                     BindingFlags.Instance)
                  .FieldType;

            }
         }

         return type;
      }


      static Type GetPropertyDrawerType(Type fieldType)
      {
         return _propertyDrawerTypes.FirstOrDefault(drawerType =>
         {
            var attribute = drawerType.GetCustomAttribute<CustomPropertyDrawer>();
            if (attribute == null) return false;

            var typeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
            var useChildrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic);
            var drawerTargetType = (Type) typeField?.GetValue(attribute);
            var drawerChildren = (bool) useChildrenField?.GetValue(attribute);

            bool match;
            if (drawerChildren)
            {
               match = drawerTargetType.IsAssignableFrom(fieldType);
            }
            else
            {
               match = fieldType == drawerTargetType;
            }
            return match;
         });
      }
   }
}