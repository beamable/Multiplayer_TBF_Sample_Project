using System;
using System.Collections.Generic;

namespace Beamable.Common.Api
{
   public abstract class UserDataCache<T>
   {
      public delegate UserDataCache<T> FactoryFunction(string name, long ttlMs, CacheResolver resolver);

      public delegate Promise<Dictionary<long, T>> CacheResolver(List<long> gamerTags);

      public string Name { get; }
      public long TtlMs { get; }
      public CacheResolver Resolver { get; }

      protected Dictionary<long, UserDataCacheEntry> cache = new Dictionary<long, UserDataCacheEntry>();
      protected List<long> gamerTagsPending = new List<long>();
      protected List<long> gamerTagsInFlight = new List<long>();
      protected Promise<Dictionary<long, T>> nextPromise = new Promise<Dictionary<long, T>>();
      protected Dictionary<long, T> result = new Dictionary<long, T>();

      protected UserDataCache(string name, long ttlMs, CacheResolver resolver)
      {
         Name = name;
         TtlMs = ttlMs;
         Resolver = resolver;
      }

      protected abstract void PerformScheduleResolve();

      public Promise<T> Get(long gamerTag)
      {
         if (gamerTagsPending.Count == 0)
         {
            PerformScheduleResolve();
         }
         gamerTagsPending.Add(gamerTag);
         return nextPromise.Map(rsp => rsp[gamerTag]);
      }

      public Promise<Dictionary<long, T>> GetBatch (List<long> gamerTags)
      {
         if (gamerTagsPending.Count == 0)
         {
            PerformScheduleResolve();
         }
         gamerTagsPending.AddRange(gamerTags);
         return nextPromise;
      }

      public void Set (long gamerTag, T data) {
         cache[gamerTag] = new UserDataCacheEntry(data);
      }

      public void Remove (long gamerTag) {
         cache.Remove(gamerTag);
      }

      protected virtual void Resolve()
      {

         // Save in flight state and reset pending state
         var promise = nextPromise;
         nextPromise = new Promise<Dictionary<long, T>>();
         result.Clear();
         gamerTagsInFlight.Clear();

         // Resolve cache
         for (int i = 0; i < gamerTagsPending.Count; i++)
         {
            UserDataCacheEntry found;
            long nextGamerTag = gamerTagsPending[i];
            if (result.ContainsKey(nextGamerTag))
            {
               continue;
            }

            if (cache.TryGetValue(nextGamerTag, out found))
            {
               if (found.IsExpired(TtlMs))
               {
                  cache.Remove(nextGamerTag);
                  gamerTagsInFlight.Add(nextGamerTag);
               }
               else
               {
                  result.Add(nextGamerTag, found.data);
               }
            }
            else
            {
               if (!gamerTagsInFlight.Contains(nextGamerTag))
               {
                  gamerTagsInFlight.Add(nextGamerTag);
               }
            }
         }
         gamerTagsPending.Clear();

         // Short circuit if cache deflected everything
         if (gamerTagsInFlight.Count == 0)
         {
            promise.CompleteSuccess(result);
         }
         else
         {
            var resolvedData = Resolver.Invoke(gamerTagsInFlight);
            resolvedData.Then(data =>
            {
               gamerTagsInFlight.Clear();

               // Update cache and fill result
               foreach (var next in data)
               {
                  Set(next.Key, next.Value);
                  result.Add(next.Key, next.Value);
               }

               // Resolve waiters
               promise.CompleteSuccess(result);
            }).Error(err =>
            {
               gamerTagsInFlight.Clear();
               promise.CompleteError(err);
            });
         }
      }


      protected class UserDataCacheEntry {
         public T data;
         private long cacheTime;

         public UserDataCacheEntry(T data) {
            this.data = data;
            this.cacheTime = Environment.TickCount;
         }

         public bool IsExpired (long ttlMs) {
            if (ttlMs == 0) {
               return false;
            }
            return ((Environment.TickCount - cacheTime) > ttlMs);
         }
      }
   }


}