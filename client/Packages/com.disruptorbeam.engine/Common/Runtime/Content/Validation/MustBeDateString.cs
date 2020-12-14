using System;
using System.Globalization;
using System.Reflection;

namespace Beamable.Common.Content.Validation
{
   public class MustBeDateString : ValidationAttribute
   {
      public override void Validate(ValidationFieldWrapper validationField, IContentObject obj, IValidationContext ctx)
      {
         if (validationField.FieldType == typeof(OptionalString))
         {
            var optional = validationField.GetValue<OptionalString>();
            if (optional.HasValue)
            {
               ValidateString(optional.Value, validationField, obj, ctx);
            }

            return;
         }

         if (validationField.FieldType == typeof(string))
         {
            var strValue = validationField.GetValue<string>();
            ValidateString(strValue, validationField, obj, ctx);
            return;
         }

         throw new ContentValidationException(obj, validationField, "date must be a string field.");
      }

      public void ValidateString(string strValue, ValidationFieldWrapper validationField, IContentObject obj, IValidationContext ctx)
      {
         if (string.IsNullOrEmpty(strValue))
         {
            throw new ContentValidationException(obj, validationField, "date cannot be an empty string.");
         }

         if (!DateTime.TryParseExact(strValue, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out _))
         {
            throw new ContentValidationException(obj, validationField, "date is not in 8601 iso format.");
         }
      }
   }

}