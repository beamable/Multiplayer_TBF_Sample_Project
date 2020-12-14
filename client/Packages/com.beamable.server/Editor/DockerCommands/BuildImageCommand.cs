using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Beamable.Common;
using Beamable.Server.Editor.CodeGen;
using Beamable.Platform.SDK;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.Server.Editor.DockerCommands
{
   public class BuildImageCommand : DockerCommandReturnable<Unit>
   {
      public bool IncludeDebugTools { get; }
      public string ImageName { get; set; }
      public string BuildPath { get; set; }

      public BuildImageCommand(MicroserviceDescriptor descriptor, bool includeDebugTools)
      {
         IncludeDebugTools = includeDebugTools;
         ImageName = descriptor.ImageName;
         BuildPath = descriptor.BuildPath;
         UnityLogLabel = $"Docker Build {descriptor.Name}";

         // copy the cs files from the source path to the build path

         var directoryQueue = new Queue<string>();
         directoryQueue.Enqueue(descriptor.SourcePath);

         string rootPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);

         // remove everything in the hidden folder...
         if (Directory.Exists(descriptor.BuildPath))
         {
            Directory.Delete(descriptor.BuildPath, true);
         }
         Directory.CreateDirectory(descriptor.BuildPath);

         while (directoryQueue.Count > 0)
         {
            var path = directoryQueue.Dequeue();

            var files = Directory
               .GetFiles(path);
            foreach (var file in files)
            {
               var subPath = file.Substring(descriptor.SourcePath.Length + 1);

               var destinationFile = Path.Combine(descriptor.BuildPath, subPath);

               Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));

               File.Copy(file, destinationFile, true);
            }

            var subDirs = Directory.GetDirectories(path);
            foreach (var subDir in subDirs)
            {
               var dirName = Path.GetFileName(subDir);
               if (new[] {"~", "obj", "bin"}.Contains(dirName) || dirName.StartsWith(".")) continue; // skip hidden or dumb folders...

               directoryQueue.Enqueue(subDir);
            }
         }



         // build the Program file, and place it in the temp dir.
         var programFilePath = Path.Combine(descriptor.BuildPath, "Program.cs");
         var csProjFilePath = Path.Combine(descriptor.BuildPath, $"{descriptor.ImageName}.csproj");
         var dockerfilePath = Path.Combine(descriptor.BuildPath, "Dockerfile");
         (new ProgramCodeGenerator(descriptor)).GenerateCSharpCode(programFilePath);
         (new DockerfileGenerator(descriptor, IncludeDebugTools)).Generate(dockerfilePath);
         (new ProjectGenerator(descriptor)).Generate(csProjFilePath);

         var deps = DependencyResolver.GetDependencies(descriptor);
         foreach (var dep in deps)
         {
            var targetRelative = dep.Agnostic.SourcePath.Substring(Application.dataPath.Length - "Assets/".Length);
            var targetFull = descriptor.BuildPath + targetRelative;

            Debug.Log("Copying to " + targetFull);
            var targetDir = Path.GetDirectoryName(targetFull);
            Directory.CreateDirectory(targetDir);

            // to avoid any file issues, we load the file into memory
            var src = File.ReadAllText(dep.Agnostic.SourcePath);
            File.WriteAllText(targetFull, src);
         }

         // TODO: Check that no UnityEngine references exist.
         // TODO: Check that there are no invalid types in the serialization process.
      }

      public override string GetCommandString()
      {
         return $"{DOCKER_LOCATION} build -t {ImageName} \"{BuildPath}\"";
      }

      protected override void Resolve()
      {

         if (string.IsNullOrEmpty(StandardErrorBuffer))
         {
            Promise.CompleteSuccess(PromiseBase.Unit);
         }
         else
         {
            Promise.CompleteError(new Exception($"Build failed err=[{StandardErrorBuffer}]"));
         }
      }
   }
}