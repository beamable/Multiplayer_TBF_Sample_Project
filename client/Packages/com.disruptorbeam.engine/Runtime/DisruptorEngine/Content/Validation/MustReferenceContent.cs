using System;
using System.Linq;
using System.Reflection;
using Beamable.Common.Content;

namespace Beamable.Content.Validation
{
   public class MustReferenceContent : ValidationAttribute
   {
      public bool AllowNull { get; set; }
      public Type[] AllowedTypes { get; set; }

      public MustReferenceContent(bool allowNull=false, params Type[] allowedTypes)
      {
         AllowNull = allowNull;
         AllowedTypes = allowedTypes;
      }

      public override void Validate(ValidationFieldWrapper field, ContentObject obj, IValidationContext ctx)
      {
         // this works for ContentRefs, or strings...
         if (typeof(string) == field.FieldType)
         {
            ValidateAsString(field, obj, ctx);
            return;
         }

         if (typeof(IContentRef).IsAssignableFrom(field.FieldType))
         {
            ValidateAsReference(field, obj, ctx);
            return;
         }
         throw new ContentValidationException(obj, field, "MustReferenceContent only works for IContentRef or String fields");


      }

      void ValidateAsReference(ValidationFieldWrapper field, ContentObject obj, IValidationContext ctx)
      {
         var reference = field.GetValue() as IContentRef;

         if (reference == null)
         {
            throw new ContentValidationException(obj, field, "reference cannot be null");
         }

         var id = reference.GetId();
         ValidateId(id, field, obj, ctx);
      }

      void ValidateAsString(ValidationFieldWrapper field, ContentObject obj, IValidationContext ctx)
      {

         var id = field.GetValue() as string;
         ValidateId(id, field, obj, ctx);
      }

      void ValidateId(string id, ValidationFieldWrapper field, ContentObject obj, IValidationContext ctx)
      {
         if (AllowNull && string.IsNullOrEmpty(id)) return;

         if (!ctx.ContentExists(id))
         {
            throw new ContentValidationException(obj, field, "reference does not exist");
         }

         if (AllowedTypes.Length > 0)
         {
            // the content must be one of the types...
            var typeNames = AllowedTypes.Select(type => ctx.GetTypeName(type)).ToList();

            if (!typeNames.Any(id.StartsWith))
            {
               throw new ContentValidationException(obj, field, $"reference must be of {string.Join(",", typeNames)}");
            }
         }

      }


   }
}