using System.Diagnostics;

#if BEAMABLE_DEVELOPER
namespace Beamable.Server.Editor.DockerCommands
{
   public class BuildBeamServiceCommand : DockerCommand
   {
      public override string GetCommandString()
      {

#if UNITY_EDITOR_OSX
         return "../microservice/build.sh";
#elif UNITY_EDITOR_WIN
         return "..\\microservice\\build.bat";
#endif

      }
   }
}


#endif