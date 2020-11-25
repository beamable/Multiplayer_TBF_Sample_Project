using System;

namespace Beamable.Common.Content
{
   public class ContentNotFoundException : Exception
   {
      public ContentNotFoundException() : base("Content reference not found")
      {

      }
   }
}