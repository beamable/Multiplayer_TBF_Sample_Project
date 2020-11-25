using System;
using System.Collections.Generic;

namespace Beamable.Content.Validation
{
   public class AggregateContentValidationException : AggregateException
   {
      public AggregateContentValidationException(IEnumerable<ContentValidationException> exs) : base(exs) {}
   }
}