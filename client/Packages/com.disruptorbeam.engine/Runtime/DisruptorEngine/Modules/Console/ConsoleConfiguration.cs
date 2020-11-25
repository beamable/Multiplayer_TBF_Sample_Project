using UnityEngine;

namespace Beamable.Modules.Console
{
   [CreateAssetMenu]
   public class ConsoleConfiguration : ModuleConfigurationObject
   {
      public static ConsoleConfiguration Instance => Get<ConsoleConfiguration>();
      public KeyCode ToggleKey = KeyCode.BackQuote;
      public bool ForceEnabled = false;
   }
}