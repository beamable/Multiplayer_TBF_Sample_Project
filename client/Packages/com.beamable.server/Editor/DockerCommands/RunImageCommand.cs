using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Beamable.Config;
using Beamable.Serialization.SmallerJSON;
using UnityEngine;

namespace Beamable.Server.Editor.DockerCommands
{

   public class RunImageCommand : DockerCommand
   {
      public string ImageName { get; set; }
      public string ContainerName { get; set; }

      private string Secret { get; set; }
      public string LogLevel { get; }

      public RunImageCommand(MicroserviceDescriptor descriptor, string secret, string logLevel="Information")
      {
         ImageName = descriptor.ImageName;
         ContainerName = descriptor.ContainerName;
         Secret = secret;
         LogLevel = logLevel;

         UnityLogLabel = $"Docker Run {descriptor.Name}";
      }

      protected override void HandleStandardOut(string data)
      {
         if (!MicroserviceLogHelper.HandleLog(UnityLogLabel, data))
         {
            base.HandleStandardOut(data);
         }
      }

      public override string GetCommandString()
      {
         string host = ConfigDatabase.GetString("socket");
         string cid = ConfigDatabase.GetString("cid");
         string pid = ConfigDatabase.GetString("pid");

         var port = FreeTcpPort();
         string command = $"{DOCKER_LOCATION} run --rm " +
                          $"-p {port}:80 " +
                          $"--env CID={cid} " +
                          $"--env PID={pid} " +
                          $"--env SECRET=\"{Secret}\" " +
                          $"--env HOST=\"{host}\" " +
                          $"--env LOG_LEVEL=\"{LogLevel}\" " +
                          $"--name {ContainerName} {ImageName}";

         return command;
      }

      static int FreeTcpPort()
      {
         TcpListener l = new TcpListener(IPAddress.Loopback, 0);
         l.Start();
         int port = ((IPEndPoint)l.LocalEndpoint).Port;
         l.Stop();
         return port;
      }
   }
}