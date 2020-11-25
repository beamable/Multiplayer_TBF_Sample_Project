using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Beamable.Common
{
   [AttributeUsage(AttributeTargets.Class)]
   public class AgnosticAttribute : Attribute
   {
      public Type[] SupportTypes { get; }
      public string SourcePath { get; }
      public string MemberName { get; }

      public AgnosticAttribute(Type[] supportTypes=null, [CallerFilePath] string sourcePath = "", [CallerMemberName] string memberName = "")
      {
         SupportTypes = supportTypes;
         SourcePath = sourcePath;
         MemberName = memberName;
      }
   }
}