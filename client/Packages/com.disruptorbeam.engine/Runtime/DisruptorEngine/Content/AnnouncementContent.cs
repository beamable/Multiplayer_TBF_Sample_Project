using System.Collections.Generic;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Content.Validation;
using UnityEngine;

namespace Beamable.Content
{
   [ContentType("announcements")]
   [System.Serializable]
   public class AnnouncementContent : ContentObject
   {
      [CannotBeEmpty]
      public string channel;
      [CannotBeEmpty]
      public string title;
      [CannotBeEmpty]
      public string summary;
      [CannotBeEmpty]
      public string body;
      [MustBeDateString]
      public string start_date;
      [MustBeDateString]
      public string end_date;
      public List<AnnouncementAttachment> attachments;
   }

   [System.Serializable]
   public class AnnouncementAttachment
   {
      [MustReferenceContent(false, typeof(CurrencyContent), typeof(ItemContent))]
      public string symbol;
      [MustBePositive()]
      public int count;
      [MustBeOneOf("currency", "items")]
      public string type;
   }
}