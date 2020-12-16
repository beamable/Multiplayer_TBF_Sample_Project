using System;
using System.IO;
using System.Threading.Tasks;

namespace Beamable.Samples.Core
{
   /// <summary>
   /// Store commonly reused functionality for concerns: async/await/Task
   /// </summary>
   public static class AsyncUtility 
   {
      /// <summary>
      /// By default "async" functionality hides any Exceptions thrown.
      /// Wrapping async calls with AsyncSafe provides visibility to Exceptions thrown.
      /// </summary>
      /// <param name="asyncAction"></param>
      /// <param name="incomingStackTrace"></param>
      /// <returns></returns>
      public static async Task AsyncSafe(Func<Task> asyncAction,
         System.Diagnostics.StackTrace incomingStackTrace)
      {
         // Safely run the async action
         try
         {
            await asyncAction();
         }
         // Handle any exceptions that occur in the async operations above
         catch (Exception exception)
         {
            var exceptionName = exception.GetType().Name;
            var frame = incomingStackTrace.GetFrame(0);
            var line = frame.GetFileLineNumber();
            var filePath = frame.GetFileName();
            var filename = Path.GetFileName(filePath);

            // Log terse, helpful info
            UnityEngine.Debug.LogError($"[ {exceptionName} ({filename} : {line}) ] {exception.Message}");
         }
      }
   }
}
