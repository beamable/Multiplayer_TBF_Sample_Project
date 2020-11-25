using System.Collections;

namespace Beamable.Content.Validation
{
   public class CannotBeEmpty : ValidationAttribute
   {
      public override void Validate(ValidationFieldWrapper validationField, ContentObject obj, IValidationContext ctx)
      {
         if (validationField.FieldType == typeof(string))
         {
            if (string.IsNullOrEmpty(validationField.GetValue<string>()))
            {
               throw new ContentValidationException(obj, validationField, "Cannot be empty string");
            }

            return;
         }

         if (typeof(IEnumerable).IsAssignableFrom(validationField.FieldType))
         {
            var set = validationField.GetValue() as IEnumerable;
            if (!set.GetEnumerator().MoveNext())
            {
               throw new ContentValidationException(obj, validationField, "Cannot be empty");
            }
         }
      }
   }
}