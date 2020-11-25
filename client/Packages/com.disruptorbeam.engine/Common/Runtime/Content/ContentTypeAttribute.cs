using System;

namespace Beamable.Content
{
   [AttributeUsage(AttributeTargets.Class)]
   public class ContentTypeAttribute : Attribute
   {
      public string TypeName { get; }

      public ContentTypeAttribute(string typeName)
      {
         TypeName = typeName;
      }
   }
}