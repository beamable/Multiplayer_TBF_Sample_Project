using System;

namespace Beamable.Common.Content
{
   public interface IContentLink
   {
      string GetId();
      void SetId(string id);
      void OnCreated();
   }

   public abstract class AbsContentLink<TContent> : AbsContentRef<TContent>, IContentLink where TContent : IContentObject, new()
   {
      public abstract void OnCreated(); // the resolution of this method is different based on client/server...
   }

   public abstract class BaseContentRef : IContentRef
   {
      public abstract string GetId();

      public abstract void SetId(string id);

      public abstract bool IsContent(IContentObject content);

      public abstract Type GetReferencedType();

      public abstract Type GetReferencedBaseType();

   }

   public abstract class AbsContentRef<TContent> : BaseContentRef, IContentRef<TContent> where TContent : IContentObject, new()
   {
      public string Id;

      public abstract Promise<TContent> Resolve(); // the resolution of this method is different based on client/server.

      public override string GetId()
      {
         return Id;
      }

      public override void SetId(string id)
      {
         Id = id;
      }

      public override bool IsContent(IContentObject content)
      {
         return content.Id.Equals(Id);
      }

      public override Type GetReferencedType()
      {
         if (string.IsNullOrEmpty(Id))
            return typeof(TContent);
         return ContentRegistry.GetTypeFromId(Id);
      }

      public override Type GetReferencedBaseType()
      {
         return typeof(TContent);
      }
   }

   public interface IContentRef
   {
      string GetId();
      void SetId(string id);
      bool IsContent(IContentObject content);
      Type GetReferencedType();
      Type GetReferencedBaseType();
   }

   public interface IContentRef<TContent> : IContentRef where TContent : IContentObject, new()
   {
      Promise<TContent> Resolve();
   }

}