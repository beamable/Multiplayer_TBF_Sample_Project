
using Beamable.Serialization;
using Beamable.Serialization.SmallerJSON;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Environment
{

   public static class BeamableEnvironment
   {
      private const string FilePath = "Packages/com.disruptorbeam.engine/Editor/Environment/env-default.json";

      static BeamableEnvironment()
      {
         // load the env on startup.
         ReloadEnvironment();
      }

      public static EnvironmentData Data { get; private set; } = new EnvironmentData();

      public static string ApiUrl => Data.ApiUrl;
      public static string PortalUrl => Data.PortalUrl;
      public static string Environment => Data.Environment;
      public static string SdkVersion => Data.SdkVersion;
      public static string BeamServiceTag => $"{Environment}_{SdkVersion}";

      public static void ReloadEnvironment()
      {
         var envAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(FilePath);
         var rawDict = Json.Deserialize(envAsset.text) as ArrayDict;
         JsonSerializable.Deserialize(Data, rawDict);
      }

      public class EnvironmentData : JsonSerializable.ISerializable
      {
         private const string BUILD__SDK__VERSION__STRING = "BUILD__SDK__VERSION__STRING";

         public string Environment;
         public string ApiUrl;
         public string PortalUrl;
         public string SdkVersion;

         public void Serialize(JsonSerializable.IStreamSerializer s)
         {
            s.Serialize("environment", ref Environment);
            s.Serialize("apiUrl", ref ApiUrl);
            s.Serialize("portalUrl", ref PortalUrl);
            s.Serialize("sdkVersion", ref SdkVersion);

            if (SdkVersion.Equals(BUILD__SDK__VERSION__STRING))
            {
               SdkVersion = "0.0.0";
            }
         }
      }
   }
}