using System;

namespace Beamable.Samples.TBF.Exceptions
{
   /// <summary>
   /// Handles errors generated when the default clause of a
   /// switch statement is reached despite intention.
   /// </summary>
   public class SwitchDefaultException : Exception
   {
      private object obj;

      public SwitchDefaultException(object obj)
      {
         this.obj = obj;
      }

      public override string Message
      {
         get
         {
            return string.Format("Switch must contain case of '{0}' for " +
                                 "type '{1}'.", obj.GetType().Name, obj);
         }
      }

      /// <summary>
      /// Recommended instead of 'throw' to avoid warning of 
      /// "warning CS0162: Unreachable code detected"
      /// </summary>
      public static void Throw(object obj)
      {
         throw new SwitchDefaultException(obj);
      }
   }
}