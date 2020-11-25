namespace Beamable.Server.Editor.DockerCommands
{
   public class StopImageCommand : DockerCommand
   {
      public string ContainerName { get; set; }

      public StopImageCommand(MicroserviceDescriptor descriptor)
      {
         ContainerName = descriptor.ContainerName;
         UnityLogLabel = $"Docker Stop {descriptor.Name}";

      }


      public override string GetCommandString()
      {
         var command = $"{DOCKER_LOCATION} stop {ContainerName}";
         return command;
      }
   }
}