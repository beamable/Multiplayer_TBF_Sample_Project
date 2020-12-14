using Beamable.Common.Content;
using Beamable.Common.Content.Validation;

namespace Beamable.Content.Validation
{
   public sealed class MustBePositive : ValidationAttribute
   {
      public bool AllowZero { get; set; }
      private const string NUMERIC_TYPE = "Value must be a numeric type.";
      private const string NOT_POSITIVE_MESSAGE = "Value must be positive";
      private const string NOT_POSITIVE_MESSAGE_ALLOW_ZERO = "Value must be zero or greater";

      public MustBePositive(bool allowZero=false)
      {
         AllowZero = allowZero;
      }

      public override void Validate(ValidationFieldWrapper field, IContentObject obj, IValidationContext ctx)
      {
         var fieldType = field.FieldType;
         var value = field.GetValue();
         if (typeof(Optional).IsAssignableFrom(fieldType))
         {
            var optional = value as Optional;
            if (!optional.HasValue)
            {
               return; // nothing going on, here.
            }

            fieldType = optional.GetOptionalType();
            value = optional.GetValue();
         }

         if (!IsNumericType(fieldType))
         {
            throw new ContentValidationException(obj, field, NUMERIC_TYPE);
         }

         var msg = AllowZero ? NOT_POSITIVE_MESSAGE_ALLOW_ZERO : NOT_POSITIVE_MESSAGE;

         // XXX: Eww.. Would be nice if we could use the `dynamic` keyword.
         if (fieldType == typeof(sbyte))
         {
            if (AllowZero ? (sbyte)value < 0 : (sbyte)value <= 0)
            {
               throw new ContentValidationException(obj, field, msg);
            }
         }
         else if (fieldType == typeof(short))
         {
            if (AllowZero ? (short)value < 0 : (short)value <= 0)
            {
               throw new ContentValidationException(obj, field, msg);
            }
         }
         else if (fieldType == typeof(int))
         {
            if (AllowZero ? (int)value < 0 : (int)value <= 0)
            {
               throw new ContentValidationException(obj, field, msg);
            }
         }
         else if (fieldType == typeof(long))
         {
            if (AllowZero ? (long)value < 0 : (long)value <= 0)
            {
               throw new ContentValidationException(obj, field, msg);
            }
         }
         else if (fieldType == typeof(float))
         {
            if (AllowZero ? (float)value < 0 : (float)value <= 0)
            {
               throw new ContentValidationException(obj, field, msg);
            }
         }
         else if (fieldType == typeof(double))
         {
            if (AllowZero ? (double)value < 0 : (double)value <= 0)
            {
               throw new ContentValidationException(obj, field, msg);
            }
         }
         else if (fieldType == typeof(decimal))
         {
            if (AllowZero ? (decimal)value < 0 : (decimal)value <= 0)
            {
               throw new ContentValidationException(obj, field, msg);
            }
         }
      }
   }
}