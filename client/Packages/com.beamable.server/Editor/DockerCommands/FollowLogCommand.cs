using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beamable.Serialization.SmallerJSON;
using UnityEngine;

namespace Beamable.Server.Editor.DockerCommands
{
   public static class MicroserviceLogHelper
   {
      public static bool HandleLog(string label, string data)
      {
         if (Json.Deserialize(data) is ArrayDict jsonDict)
         {
            // this is a serilog message!

            var timestamp = "";
            var logLevel = "Info"; // info by default
            var message = ""; // rendered message
            var objs = new Dictionary<string, object>();
            foreach (var kvp in jsonDict)
            {
               var key = kvp.Key;
               if (key.StartsWith("__"))
               {
                  switch (key.Substring("__".Length))
                  {
                     case "l": // logLevel
                        logLevel = kvp.Value.ToString();
                        break;
                     case "t": // timestamp
                        timestamp = kvp.Value.ToString();
                        break;
                     case "m": // message
                        message = kvp.Value.ToString();
                        break;
                  }
               }
               else
               {
                  objs.Add(key, kvp.Value);
               }
            }

            string WithColor(Color logColor, string log)
            {
               var msg = $"<color=\"#{ColorUtility.ToHtmlStringRGB(logColor)}\">{log}</color>";
               return msg;
            }

            var color = Color.grey;
            switch (logLevel)
            {
               case "Debug":
                  color = new Color(.25f, .5f, 1);
                  break;
               case "Warning":
                  color = new Color(1, .6f, .15f);
                  break;
               case "Info":
                  color = Color.blue;
                  break;
               case "Error":
               case "Fatal":
                  color = Color.red;
                  break;
               default:
                  color = Color.black;
                  break;
            }

            var f = .8f;
            var darkColor = new Color(color.r * f, color.g * f, color.b * f);

            var objsToString = string.Join("\n", objs.Select(kvp => $"{kvp.Key}={Json.Serialize(kvp.Value, new StringBuilder())}"));

            Debug.Log($"{WithColor(Color.grey, $"[{label}]")} {WithColor(color, $"[{logLevel}]")} {WithColor(darkColor, $"{message}\n{objsToString}")}");

            return true;
         } else
         {
            return false;
         }
      }

   }


   public class FollowLogCommand : DockerCommand
   {
      public string ContainerName { get; }

      public FollowLogCommand(MicroserviceDescriptor descriptor)
      {
         ContainerName = descriptor.ContainerName;
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
         return $"{DOCKER_LOCATION} logs {ContainerName} -f --since 0m";
      }
   }
}