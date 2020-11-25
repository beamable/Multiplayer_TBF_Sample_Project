using System;

namespace Beamable.Common
{
   public abstract class BeamableLogProvider
   {

#if UNITY_EDITOR || UNITY_ENGINE
      private static readonly BeamableLogProvider DefaultProvider = new BeamableLogUnityProvider();
#else
      private static readonly BeamableLogProvider DefaultProvider = new SilentLogProvider();
#endif // UNITY_EDITOR || UNITY_ENGINE

      public static BeamableLogProvider Provider { get; set; } = DefaultProvider;

      public abstract void Info(string message);
      public abstract void Info(string message, params object[] args);
      public abstract void Warning(string message);
      public abstract void Error(Exception ex);
   }

   /// <summary>
   /// BeamableLogUnityProvider is a simple passthrough to UnityEngine.Debug
   /// methods such as Log, LogWarning, and LogException.
   /// </summary>
#if !DB_MICROSERVICE
   public class BeamableLogUnityProvider : BeamableLogProvider
   {
      public override void Info(string message)
      {
         UnityEngine.Debug.Log(message);
      }

      public override void Info(string message, params object[] args)
      {
         UnityEngine.Debug.Log(string.Format(message, args));
      }

      public override void Warning(string message)
      {
         UnityEngine.Debug.LogWarning(message);
      }

      public override void Error(Exception ex)
      {
         UnityEngine.Debug.LogException(ex);
      }
   }
#endif

   /// <summary>
   /// SilentLogProvider is a provider for use on physical devices, where
   /// spamming the device log is undesirable. This log provider silently
   /// swallows all input it receives.
   /// </summary>
   public class SilentLogProvider : BeamableLogProvider
   {
      public override void Info(string message) {}
      public override void Info(string message, params object[] args) {}
      public override void Warning(string message) {}
      public override void Error(Exception ex) {}
   }

   /// <summary>
   /// The Beamable Debug is a simple mock of the UnityEngine Debug class.
   /// The intention is not to replicate the entire set of functionality from Unity's Debug class,
   /// but to provide an easy reflexive log solution for dotnet core code.
   /// </summary>
   public static class Debug
   {
      public static void Assert(bool assertion)
      {
         if (!assertion)
         {
            LogError(new Exception("Assertion failed")); // TODO throw callstack info?
         }
      }
      public static void Log(string info)
      {
         BeamableLogProvider.Provider.Info(info);
      }

      public static void Log(string info, params object[] args)
      {
         BeamableLogProvider.Provider.Info(info, args);
      }

      public static void LogWarning(string warning)
      {
         BeamableLogProvider.Provider.Warning(warning);

      }

      public static void LogException(Exception ex)
      {
         BeamableLogProvider.Provider.Error(ex);
      }

      public static void LogError(Exception ex)
      {
         BeamableLogProvider.Provider.Error(ex);
      }
   }
}
