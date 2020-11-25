using System;

namespace Beamable.Service
{
   // This interface should really only be used for collections of resolvers, and defines only those methods that can
   // be used generically across resolvers without knowing the type they resolve.  Pragmatically any implementor should
   // always inherit the generic typed interface below.
   public interface IServiceResolver
   {
      void OnTeardown();
   }


   public interface IServiceResolver<T> : IServiceResolver
      where T : class
   {
      // Returns true if Resolve will return a valid object, even if lazily constructed.
      bool CanResolve();

      // Returns true only if Resolve will return an existing object without doing any creation work.
      bool Exists();

      T Resolve();
   }


   public class ServiceContainer<T> : IServiceResolver<T>
      where T : class
   {
      protected T instance;

      public ServiceContainer(T instance)
      {
         this.instance = instance;
      }

      public bool CanResolve()
      {
         return instance != null;
      }

      public bool Exists()
      {
         return instance != null;
      }

      public T Resolve()
      {
         return instance;
      }

      public virtual void OnTeardown()
      {
         var disposable = instance as IDisposable;
         if (disposable != null)
         {
            disposable.Dispose();
         }
         instance = null;
         ServiceManager.Remove(this);
      }
   }
}
