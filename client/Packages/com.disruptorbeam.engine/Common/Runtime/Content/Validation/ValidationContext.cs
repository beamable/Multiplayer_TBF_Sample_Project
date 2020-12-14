using System;
using System.Collections.Generic;
using System.Reflection;

namespace Beamable.Common.Content.Validation
{
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