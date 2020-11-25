using System;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Platform.SDK;
using UnityEngine;

namespace Beamable.Content
{
   public class ContentRef : BaseContentRef
   {
      private readonly Type _contentType;
      private string _id;

      public ContentRef(Type contentType, string id)
      {
         _contentType = contentType;
         _id = id;
      }


      public override string GetId() => _id;

      public override void SetId(string id) => _id = id;

      public override bool IsContent(IContentObject content)
      {
         return content.Id.Equals(_id);
      }

      public override Type GetReferencedType()
      {
         if (string.IsNullOrEmpty(_id))
            return _contentType;
         return ContentRegistry.GetTypeFromId(_id);
      }

      public override Type GetReferencedBaseType()
      {
         return _contentType;
      }
   }

   public class ContentRef<TContent> : AbsContentRef<TContent> where TContent : ContentObject, IContentObject, new()
   {
      private Promise<TContent> _promise;

      public override Promise<TContent> Resolve()
      {
         return _promise ?? (_promise = API.Instance.FlatMap(de => de.ContentService.GetContent(this)));
      }
   }

   public class ContentLink<TContent> : AbsContentLink<TContent> where TContent : ContentObject, IContentObject, new()
   {
      private Promise<TContent> _promise;

      public override Promise<TContent> Resolve()
      {
         return _promise ?? (_promise = API.Instance.FlatMap(de => de.ContentService.GetContent(this)));
      }

      public override void OnCreated()
      {
         if (Application.isPlaying)
         {
            Resolve();
         }
      }
   }

//   [System.Serializable]
//   public class ContentRef<TContent> : ContentRef
//      where TContent : ContentObject, new()
//   {
//      private Promise<TContent> _promise;
//
//      public Promise<TContent> Resolve()
//      {
//         return _promise ?? (_promise = DisruptorEngine.Instance.FlatMap(de => de.ContentService.GetContent(this)));
//      }
//
//      public override Type GetReferencedType()
//      {
//         return typeof(TContent);
//      }
//   }

//   public interface AutoResolveContent
//   {
//      void AutoResolve();
//   }

//   public class ContentLink<TContent> : ContentRef<TContent>, AutoResolveContent
//      where TContent: ContentObject, new()
//   {
//      public void AutoResolve()
//      {
//         Resolve();
//      }
//   }

//   public abstract class ContentRef
//   {
//      /// <summary>
//      /// ex: "currency.gems"
//      /// </summary>
//      public string Id;
//
//      public bool IsContent(ContentObject contentObject)
//      {
//         return contentObject.Id.Equals(Id);
//      }
//
//      public abstract Type GetReferencedType();
//   }
}