using System.Collections.Generic;

namespace Beamable.Common.Content.Validation
{
   public class MustBeOneOf : ValidationAttribute
   {
      public HashSet<object> PossibleValues { get; }
      private string _errorMessage;

      public MustBeOneOf(params object[] possibleValues)
      {
         PossibleValues = new HashSet<object>(possibleValues);
         if (possibleValues.Length > 1)
         {
            _errorMessage = $"Must be one of [{string.Join(", ", possibleValues)}]";
         } else if (possibleValues.Length == 1)
         {
            _errorMessage = $"Must be {possibleValues[0]}";
         }
         else
         {
            _errorMessage = $"No value supported.";
         }
      }
      public override void Validate(ValidationFieldWrapper validationField, IContentObject obj, IValidationContext ctx)
      {
         if (PossibleValues.Contains(validationField.GetValue()))
         {
            return;
         }

         throw new ContentValidationException(obj, validationField, _errorMessage);
      }
   }
}