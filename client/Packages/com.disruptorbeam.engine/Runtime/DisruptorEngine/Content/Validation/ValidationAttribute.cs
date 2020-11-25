using System;
using System.Collections.Generic;
using System.Reflection;
using Beamable.Common.Content;
using UnityEngine;

namespace Beamable.Content.Validation
{
   [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
   public abstract class ValidationAttribute : PropertyAttribute
   {
      private static readonly List<Type> numericTypes = new List<Type>
      {
         typeof(byte),
         typeof(sbyte),
         typeof(short),
         typeof(ushort),
         typeof(int),
         typeof(uint),
         typeof(long),
         typeof(ulong),
         typeof(float),
         typeof(double),
         typeof(decimal)
      };

      protected static bool IsNumericType(Type type)
      {
         return numericTypes.Contains(type);
      }

      public abstract void Validate(ValidationFieldWrapper validationField, ContentObject obj, IValidationContext ctx);


   }
   public class ValidationFieldWrapper
   {
      public FieldInfo Field { get; }
      public object Target { get; }

      public ValidationFieldWrapper(FieldInfo field, object target)
      {
         Field = field;
         Target = target;
      }
      public Type FieldType => Field?.FieldType;
      public object GetValue() => Field?.GetValue(Target);
      public T GetValue<T>() => (T) Field?.GetValue(Target);
   }
   public interface IValidationContext
   {
      bool ContentExists(string id);
      string GetTypeName(Type type);
   }
   public class ValidationContext : IValidationContext
   {
      public HashSet<string> AllContentIds = new HashSet<string>();
      public bool ContentExists(string id) => AllContentIds?.Contains(id) ?? false;
      public string GetTypeName(Type type)
      {
         return ContentRegistry.GetContentTypeName(type);
      }
   }
}