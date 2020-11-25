using System;

namespace Beamable.Content
{
   [AttributeUsage(validOn: AttributeTargets.Field)]
   public class IgnoreContentFieldAttribute : Attribute
   {

   }
}