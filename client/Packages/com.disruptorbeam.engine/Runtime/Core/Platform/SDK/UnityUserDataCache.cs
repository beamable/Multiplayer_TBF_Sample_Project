using System.Collections;
using System.Collections.Generic;
using System;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Coroutines;
using Beamable.Service;

namespace Beamable.Api {
   public class UnityUserDataCache<T> : UserDataCache<T>
   {
      public static UnityUserDataCache<T> CreateInstance(string name, long ttlMs, CacheResolver resolver)
      {
         return new UnityUserDataCache<T>(name, ttlMs, resolver);
      }

      private string coroutineContext;

      // If TTL is 0, then never expire anything
      public UnityUserDataCache(string name, long ttlMs, CacheResolver resolver) : base(name, ttlMs, resolver) {
         coroutineContext = $"userdatacache_{name}";
      }

      private IEnumerator ScheduleResolve()
      {
         yield return Yielders.EndOfFrame;
         while (gamerTagsInFlight.Count != 0)
         {
            yield return Yielders.EndOfFrame;
         }
         Resolve();

      }

      protected override void PerformScheduleResolve()
      {
         ServiceManager.Resolve<CoroutineService>().StartNew(coroutineContext, ScheduleResolve());
      }
   }
}