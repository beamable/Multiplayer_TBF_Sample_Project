#if DB_MICROSERVICE
using System;

namespace UnityEngine
{
   [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
   public class TooltipAttribute : Attribute
   {
      public readonly string tooltip;

      public TooltipAttribute(string tooltip)
      {
         this.tooltip = tooltip;
      }
   }

   public class SerializeField : Attribute {
   }
}
#endif