using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Content.Validation;

namespace Beamable.Content
{
   [ContentType("emails")]
   [System.Serializable]
   [Agnostic]
   public class EmailContent : ContentObject
   {
      [CannotBeEmpty]
      public string subject;

      public string body;
   }

   [System.Serializable]
   [Agnostic]
   public class EmailRef : ContentRef<EmailContent> {}
}