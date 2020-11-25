using System;

namespace Beamable.Platform.SDK.CloudSaving
{
   public class CloudSavingError : Exception
   {
      public CloudSavingError(string message, Exception inner) : base(message, inner)
      {
      }
   }
}