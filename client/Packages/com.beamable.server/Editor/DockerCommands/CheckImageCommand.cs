using UnityEngine;

namespace Beamable.Server.Editor.DockerCommands
{
   public class CheckImageCommand : DockerCommand
   {
      public string ContainerName { get; set; }

      public bool IsRunning { get; private set; }

      public CheckImageCommand(MicroserviceDescriptor descriptor)
      {
         ContainerName = descriptor.ContainerName;
      }

      protected override void HandleStandardOut(string data)
      {
         base.HandleStandardOut(data);

         // 7c7e95c20caf        tunafish            "dotnet tunafish.dll"   7 hours ago         Up 7 hours          0.0.0.0:56798->80/tcp   tunafishcontainer

         // TODO: We could use a better text maching system, but for now...
         if (data != null && data.Contains($" {ContainerName}") && data.Contains(" Up "))
         {
            IsRunning = true;
         }
      }

      public override string GetCommandString()
      {
         var command = $"{DOCKER_LOCATION} ps -f \"name={ContainerName}\"";
         return command;
      }
   }
}