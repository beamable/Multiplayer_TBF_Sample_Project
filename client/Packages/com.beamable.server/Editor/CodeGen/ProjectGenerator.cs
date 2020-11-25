using System.IO;

namespace Beamable.Server.Editor.CodeGen
{
   public class ProjectGenerator
   {
      public MicroserviceDescriptor Descriptor { get; }
      /*
       * <Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
  	<DefineConstants>DB_MICROSERVICE</DefineConstants>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp3.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="12.0.3" />
    <Reference Include="BeamableMicroserviceBase">
      <HintPath>/src/obj/Release/netcoreapp3.0/BeamableMicroserviceBase.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>

       */


      public ProjectGenerator(MicroserviceDescriptor descriptor)
      {
         Descriptor = descriptor;
      }


      public string GetString()
      {
         var text = $@"<Project Sdk=""Microsoft.NET.Sdk"">
            <PropertyGroup>
               <DefineConstants>DB_MICROSERVICE</DefineConstants>
               <OutputType>Exe</OutputType>
               <TargetFramework>netcoreapp3.0</TargetFramework>
            </PropertyGroup>

            <ItemGroup>
               <PackageReference Include=""Newtonsoft.Json"" Version=""12.0.3"" />
               <Reference Include=""BeamableMicroserviceBase"">
                  <HintPath>/app/BeamableMicroserviceBase.dll</HintPath>
               </Reference>
               <Reference Include=""Beamable.Common"">
                  <HintPath>/src/lib/Beamable.Common.dll</HintPath>
               </Reference>
               <Reference Include=""Beamable.Server"">
                  <HintPath>/src/lib/Beamable.Server.dll</HintPath>
               </Reference>
               </ItemGroup>
            </Project>
            ";
         return text;
      }

      public void Generate(string filePath)
      {
         var content = GetString();
         File.WriteAllText(filePath, content);
      }
   }
}