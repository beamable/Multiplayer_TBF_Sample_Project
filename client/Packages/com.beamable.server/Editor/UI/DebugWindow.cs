

   using System;
   using System.Collections.Generic;
   using System.Linq;
   using Beamable.Server;
   using Beamable.Server.Editor;
   using Beamable.Server.Editor.ManagerClient;
   using Beamable.Server.Editor.DockerCommands;
   using Beamable.Server.Editor.UI;
   using Beamable.Server.Editor.Uploader;
   using Beamable.Config;
   using Beamable.Platform.SDK;
   using Beamable.Editor;
   using UnityEditor;
   using UnityEngine;

   public class DebugWindow : CommandRunnerWindow
   {
      [MenuItem(
         ContentConstants.MENU_ITEM_PATH_WINDOW_BEAMABLE + "/" +
         ContentConstants.OPEN + " " +
         ContentConstants.MICROSERVICES_MANAGER,
         priority = ContentConstants.MENU_ITEM_PATH_WINDOW_PRIORITY_2)]
      public static void Init()
      {
         var inspector = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");

         DebugWindow wnd = GetWindow<DebugWindow>("Microservices Manager", true, inspector);
         wnd.Show();
      }

      //private List<MicroserviceDescriptor> _descriptors = new List<MicroserviceDescriptor>();

      private void OnGUI()
      {

         if (GUILayout.Button("PRINT STATUS"))
         {
            EditorAPI.Instance.Then(de =>
            {
               de.GetMicroserviceManager().GetStatus().Then(status =>
               {
                  Debug.Log($"----- status ----- isCurrent=[{status.isCurrent}]");
                  foreach (var service in status.services)
                  {
                     Debug.Log($"service=[{service.serviceName}] running=[{service.running}] imageId=[{service.imageId}] isCurrent=[{service.isCurrent}]");
                  }
               });
            });
         }
         if (GUILayout.Button("GET MANIFEST"))
         {
            EditorAPI.Instance.Then(de => { de.GetMicroserviceManager().GetCurrentManifest().Then(
               manifest =>
               {
                  Debug.Log("manifest " + manifest);
               }); });

         }

         if (GUILayout.Button("GET ALL MANIFEST"))
         {
            EditorAPI.Instance.Then(de => { de.GetMicroserviceManager().GetManifests().Then(res =>
            {
               res.ForEach(s => Debug.Log(string.Join(",", s.manifest.Select(m => m.serviceName))));
            }); });
         }

         if (GUILayout.Button("WRITE EMPTY MANIFEST"))
         {
            EditorAPI.Instance.Then(de =>
            {
               var service = de.GetMicroserviceManager();
               service.Deploy(new ServiceManifest());
            });
         }

         if (GUILayout.Button("WRITE ACTUAL MANIFEST"))
         {
            var wnd = DeployWindow.ShowDeployPopup();
         }


         if (GUILayout.Button("REFRESH") || Microservices.Descriptors == null)
         {
            //Microservices.Descriptors = Microservices.ListMicroservices().ToList();
         }

         #if BEAMABLE_DEVELOPER
         if (GUILayout.Button("BUILD BEAMSERVICE"))
         {
            var command = new BuildBeamServiceCommand();
            command.Start();
         }
         #endif


         foreach (var service in Microservices.Descriptors)
         {
            GUILayout.Label(service.ImageName + " // " + service.SourcePath);

            var machine = Microservices.GetServiceStateMachine(service);


            GUI.enabled = true;

//            if (GUILayout.Button("Upload container"))
//            {
//               var uploader = new ContainerUploadHarness(this);
//               //await uploader.UploadContainer(service);
//            }

            if (GUILayout.Button("Open Default Route"))
            {
               EditorAPI.Instance.Then(de =>
               {
                  var url = $"http://dev.api.beamable.com/basic/{de.Cid}.{de.Pid}.{service.Name}/ServerCall?myData=ping";
                  Application.OpenURL(url);
               });
            }

            GUI.enabled = machine.CurrentState == MicroserviceState.IDLE;
            if (GUILayout.Button("Build"))
            {
               machine.MoveNext(MicroserviceCommand.BUILD);
            }

            GUI.enabled = machine.CurrentState == MicroserviceState.IDLE;
            machine.UseDebug = GUILayout.Toggle(machine.UseDebug, "Use Debug");
            if (GUILayout.Button("Start"))
            {
               machine.MoveNext(MicroserviceCommand.START);

            }

            GUI.enabled = machine.CurrentState == MicroserviceState.RUNNING;
            if (GUILayout.Button("Stop"))
            {
               machine.MoveNext(MicroserviceCommand.STOP);
            }


            GUI.enabled = true;

         }
      }
   }
