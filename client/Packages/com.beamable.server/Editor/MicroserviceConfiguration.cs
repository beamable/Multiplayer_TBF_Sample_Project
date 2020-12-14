using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Modules;
using UnityEngine;

namespace Beamable.Server.Editor
{

   public class MicroserviceConfigConstants : IConfigurationConstants
   {
      public string GetSourcePath(Type type)
      {
         //
         // TODO: make this work for multiple config types
         //       but for now, there is just the one...

         return "Packages/com.beamable.server/Editor/microserviceConfiguration.asset";

      }
   }

   public class MicroserviceConfiguration : AbsModuleConfigurationObject<MicroserviceConfigConstants>
   {
      public static MicroserviceConfiguration Instance => Get<MicroserviceConfiguration>();

      public List<MicroserviceConfigurationEntry> Microservices;

      public MicroserviceConfigurationEntry GetEntry(string serviceName)
      {
         var existing = Microservices.FirstOrDefault(s => s.ServiceName == serviceName);
         if (existing == null)
         {
            existing = new MicroserviceConfigurationEntry
            {
               ServiceName = serviceName,
               TemplateId = "small",
               Enabled = true,
               DebugData = new MicroserviceConfigurationDebugEntry
               {
                  Password = "Password!",
                  Username = "root",
                  SshPort = 11100 + Microservices.Count
               }
            };
            Microservices.Add(existing);
         }
         return existing;
      }
   }

   [System.Serializable]
   public class MicroserviceConfigurationEntry
   {
      public string ServiceName;
      public bool Enabled;
      public string TemplateId;

      public MicroserviceConfigurationDebugEntry DebugData;
   }

   [System.Serializable]
   public class MicroserviceConfigurationDebugEntry
   {
      public string Username = "beamable";
      public string Password = "beamable";
      public int SshPort = -1;
   }
}