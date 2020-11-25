using System;

namespace Beamable.Server
{
   [AttributeUsage(AttributeTargets.Class)]
   public class MicroserviceAttribute : Attribute
   {
      public string MicroserviceName { get; }
      public string SourcePath { get; }


      public MicroserviceAttribute(string microserviceName, [System.Runtime.CompilerServices.CallerFilePath] string sourcePath="")
      {
         MicroserviceName = microserviceName;
         SourcePath = sourcePath;
      }


      public string GetSourcePath()
      {
         return SourcePath;
      }
   }
}