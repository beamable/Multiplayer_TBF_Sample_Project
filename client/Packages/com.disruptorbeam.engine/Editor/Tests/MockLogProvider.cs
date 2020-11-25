using System;
using Beamable.Common;

namespace Beamable.Editor.Tests
{
   public class MockLogProvider : BeamableLogProvider
   {
      public Action<string, object[]> onInfo;
      public Action<string> onWarning;
      public Action<Exception> onException;

      public override void Info(string message)
      {
         onInfo?.Invoke(message, new object[]{});
      }

      public override void Info(string message, params object[] args)
      {
         onInfo?.Invoke(message, args);
      }

      public override void Warning(string message)
      {
         onWarning?.Invoke(message);
      }

      public override void Error(Exception ex)
      {
         onException?.Invoke(ex);
      }
   }
}