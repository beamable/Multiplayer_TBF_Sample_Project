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

         return "Packages/com.beamable.server/Editor/Resources/microserviceConfiguration.asset";

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
               Enabled = true
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

   }
}