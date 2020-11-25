using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Beamable.Modules
{
   public interface IConfigurationConstants
   {

      string GetSourcePath(Type type);
   }

   public class BeamableConfigurationConstants : IConfigurationConstants
   {
      private const string PACKAGE_EDITOR_DIR = "Packages/com.disruptorbeam.engine/Editor/Modules";
      private const string MODULE_CONFIG_DIR = "Config";

      public string GetSourcePath(Type type)
      {
         var name = type.Name;
         var moduleName = ModuleConfigurationUtil.GetModuleName(type);
         var sourcePath = $"{PACKAGE_EDITOR_DIR}/{moduleName}/{MODULE_CONFIG_DIR}/{name}.asset";
         return sourcePath;
      }
   }

   public static class ModuleConfigurationUtil
   {
      private const string CONFIGURATION = "Configuration";

      public static string GetModuleName(Type configurationType)
      {

#if UNITY_EDITOR

         if (!configurationType.Name.EndsWith(CONFIGURATION))
         {
            throw new Exception($"A module configuration object class name must always end with the literal string, \"{CONFIGURATION}\"");
         }

         var moduleName = configurationType.Name.Substring(0, configurationType.Name.Length - CONFIGURATION.Length);
         return moduleName;
#else
         throw new NotImplementedException();
#endif

      }

   }

   public abstract class BaseModuleConfigurationObject : ScriptableObject
   {

   }

   public abstract class AbsModuleConfigurationObject<TConstants> : BaseModuleConfigurationObject
      where TConstants : IConfigurationConstants, new()
   {
      private const string CONFIG_RESOURCES_DIR = "Assets/DisruptorEngine/Resources";
      private static Dictionary<Type, BaseModuleConfigurationObject> _typeToConfig = new Dictionary<Type, BaseModuleConfigurationObject>();

      public static TConfig Get<TConfig>() where TConfig : BaseModuleConfigurationObject
      {
         var type = typeof(TConfig);
         if (_typeToConfig.TryGetValue(type, out var existingData))
         {
            return existingData as TConfig;
         }

         var constants = new TConstants();
         var name = type.Name;

         var data = Resources.Load<TConfig>(name);
#if UNITY_EDITOR
         if (data == null)
         {

            var sourcePath = constants.GetSourcePath(type);

            if (!File.Exists(sourcePath))
            {
               throw new Exception($"No module configuration exists at {sourcePath}. Please create it.");
            }

            Directory.CreateDirectory(CONFIG_RESOURCES_DIR);
            var sourceData = File.ReadAllText(sourcePath);
            File.WriteAllText($"{CONFIG_RESOURCES_DIR}/{name}.asset", sourceData);
            UnityEditor.AssetDatabase.Refresh();
            data = Resources.Load<TConfig>(name);
         }
#endif
         _typeToConfig[type] = data;
         return data;
      }

   }


   public class ModuleConfigurationObject : AbsModuleConfigurationObject<BeamableConfigurationConstants>
   {

   }

}