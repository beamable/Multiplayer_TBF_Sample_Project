using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Beamable.Server;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Server.Editor.CodeGen;
using Beamable.Server.Editor.ManagerClient;
using Beamable.Server.Editor.DockerCommands;
using Beamable.Server.Editor.UI.Components;
using Beamable.Server.Editor.Uploader;
using Beamable.Platform.SDK;
using Beamable.Editor;
using UnityEditor.Callbacks;
using UnityEditor.VersionControl;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.Server.Editor
{
   public static class Microservices
   {
      private static Dictionary<string, MicroserviceStateMachine> _serviceToStateMachine = new Dictionary<string, MicroserviceStateMachine>();

      private static List<MicroserviceDescriptor> _descriptors = null;
      public static List<MicroserviceDescriptor> Descriptors => _descriptors ?? (_descriptors = ListMicroservices());


      public static List<MicroserviceDescriptor> ListMicroservices()
      {
         var assemblies = AppDomain.CurrentDomain.GetAssemblies();

         var dataPath = Application.dataPath;
         var scriptLibraryPath =  dataPath.Substring(0, dataPath.Length - "Assets".Length);

         var output = new List<MicroserviceDescriptor>();
         foreach (var assembly in assemblies)
         {

            try
            {
//               if (!assembly.Location.StartsWith(scriptLibraryPath))
//               {
//                  continue;
//               }

               foreach (var type in assembly.GetTypes())
               {
                  var attribute = type.GetCustomAttribute<MicroserviceAttribute>(false);
                  if (!type.IsClass || attribute == null)
                  {
                     continue;
                  }

                  if (attribute.MicroserviceName.ToLower().Equals("xxxx"))
                  {
                     continue; // TODO: XXX this is a hacky way to ignore the default microservice...
                  }

                  if (!typeof(Microservice).IsAssignableFrom(type))
                  {
                     Debug.LogError(
                        $"The {nameof(MicroserviceAttribute)} is only valid on classes that are assignable from {nameof(Microservice)}");
                     continue;
                  }

                  var descriptor = new MicroserviceDescriptor
                  {
                     Name = attribute.MicroserviceName,
                     Type = type,
                     AttributePath = attribute.GetSourcePath()
                  };
                  output.Add(descriptor);
               }
            }
            catch (Exception)
            {
               continue; // ignore anything that doesn't have a Location property..
            }
         }

         return output;
      }


      [DidReloadScripts]
      static void WatchMicroserviceFiles()
      {
         foreach (var service in ListMicroservices())
         {
            GenerateClientSourceCode(service);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
               using (var fsw = new FileSystemWatcher(service.SourcePath))
               {
                  fsw.IncludeSubdirectories = false;
                  fsw.NotifyFilter = NotifyFilters.LastWrite;
                  fsw.Filter = "*.cs";

                  fsw.Changed += (sender, args) =>
                  {
                     GenerateClientSourceCode(service);
                  };
                  fsw.Deleted += (sender, args) =>
                  {
                     /* TODO: Delete the generated client? */
                  };

                  fsw.EnableRaisingEvents = true;

                  // spin.
                  while (true) {}
               }
            });
         }
      }

      public static Promise<ManifestModel> GenerateUploadModel()
      {
         // first, get the server manifest
         return EditorAPI.Instance.FlatMap(de =>
         {
            var client = de.GetMicroserviceManager();
            return client.GetCurrentManifest().Map(manifest =>
            {
               var allServices = new HashSet<string>();

               // make sure all server-side things are represented
               foreach (var serverSideService in manifest.manifest.Select(s => s.serviceName))
               {
                  allServices.Add(serverSideService);
               }

               // add in anything locally...
               foreach (var descriptor in Descriptors)
               {
                  allServices.Add(descriptor.Name);
               }

               // get enablement for each service...
               var config = MicroserviceConfiguration.Instance.Microservices;
               var entries = allServices.Select(name =>
               {
                  var configEntry = MicroserviceConfiguration.Instance.GetEntry(name);//config.FirstOrDefault(s => s.ServiceName == name);
                  return new ManifestEntryModel
                  {
                     Comment = "",
                     ServiceName = name,
                     Enabled = configEntry?.Enabled ?? true,
                     TemplateId = configEntry?.TemplateId ?? "small",
                  };
               }).ToList();

               return new ManifestModel
               {
                  ServerManifest = manifest.manifest.ToDictionary(e => e.serviceName),
                  Comment = "",
                  Services = entries.ToDictionary(e => e.ServiceName)
               };
            });
         });
      }

      [DidReloadScripts]
      static void AutomaticMachine()
      {
         foreach (var d in Descriptors)
         {
            GetServiceStateMachine(d);
         }
      }

      static void GenerateClientSourceCode(MicroserviceDescriptor service)
      {
         var key = service.Name;
         Directory.CreateDirectory("Assets/Beamable/AutoGenerated/Microservices");
         var targetFile = $"Assets/Beamable/Autogenerated/Microservices/{service.Name}Client.cs";

         var tempFile = Path.Combine("Temp", $"{service.Name}Client.cs");

         var oldChecksum = Checksum(targetFile);

         var generator = new ClientCodeGenerator(service);
         generator.GenerateCSharpCode(tempFile);

         var nextChecksum = Checksum(tempFile);
         var requiresRebuild = !oldChecksum.Equals(nextChecksum);

         Debug.Log($"Considering rebuilding {key}. {requiresRebuild} Old=[{oldChecksum}] Next=[{nextChecksum}]");
         if (requiresRebuild)
         {
            Debug.Log($"Generating client for {service.Name}");
            File.Copy(tempFile, targetFile, true);
         }
      }

      public static MicroserviceStateMachine GetServiceStateMachine(MicroserviceDescriptor descriptor)
      {
         var key = descriptor.Name;

         if (!_serviceToStateMachine.ContainsKey(key))
         {
            var pw = new CheckImageCommand(descriptor);
            pw.WriteLogToUnity = false;
            pw.Start();
            pw.Join();


            var initialState = pw.IsRunning ? MicroserviceState.RUNNING : MicroserviceState.IDLE;

            _serviceToStateMachine.Add(key, new MicroserviceStateMachine(descriptor, initialState));
         }

         return _serviceToStateMachine[key];
      }

      private static string Checksum(string filePath)
      {
         if (!File.Exists(filePath))
         {
            return "";
         }
         using(var stream = new BufferedStream(File.OpenRead(filePath), 1200000))
         {
            var md5 = MD5.Create();
            byte[] checksum = md5.ComputeHash(stream);
            return BitConverter.ToString(checksum).Replace("-", String.Empty);
         }
      }



      public static async System.Threading.Tasks.Task Deploy(ManifestModel model, CommandRunnerWindow context)
      {
         // TODO perform sort of diff, and only do what is required. Because this is a lot of work.
         var de = await EditorAPI.Instance;

         var nameToImageId = new Dictionary<string, string>();

         foreach (var descriptor in Descriptors)
         {
            var entry = model.Services[descriptor.Name];
            Debug.Log($"Building service=[{descriptor.Name}]");
            var buildCommand = new BuildImageCommand(descriptor, false);
            await buildCommand.Start(context);

            var uploader = new ContainerUploadHarness(context);

            Debug.Log($"Getting Id service=[{descriptor.Name}]");
            var imageId = await uploader.GetImageId(descriptor);
            nameToImageId.Add(descriptor.Name, imageId);


            Debug.Log($"Uploading container service=[{descriptor.Name}]");
            await uploader.UploadContainer(descriptor, imageId);

         }

         var client = de.GetMicroserviceManager();
         Debug.Log($"Deploying manifest");

         await client.Deploy(new ServiceManifest
         {
            comments = model.Comment,
            manifest = model.Services.Select(kvp => new ServiceReference
            {
               serviceName = kvp.Value.ServiceName,
               templateId = kvp.Value.TemplateId,
               enabled = kvp.Value.Enabled,
               comments = kvp.Value.Comment,
               imageId = nameToImageId[kvp.Value.ServiceName]
            }).ToList()
         });

         Debug.Log("Service Deploy Complete");
      }

   }
}