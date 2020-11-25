using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Beamable.Common
{
   public abstract class PromiseBase
   {
      protected Action<Exception> errbacks;
      public bool HadAnyErrbacks { protected set; get; }

      protected Exception err;
      protected bool done;
      public static readonly Unit Unit = new Unit();

      public bool IsCompleted => done;

      private static PromiseEvent OnPotentialUncaughtError;

      public static void SetPotentialUncaughtErrorHandler(PromiseEvent handler)
      {
         OnPotentialUncaughtError = handler; // this overwrites it everytime, blowing away any other listeners. This allows someone to override the functionality.
      }

      protected void InvokeUncaughtPromise()
      {
         OnPotentialUncaughtError?.Invoke(this, err);
      }

   }

   public delegate void PromiseEvent(PromiseBase promise, Exception err);

   public class Promise<T> : PromiseBase, ICriticalNotifyCompletion
   {
      private Action<T> _callbacks;
      private T _val;


      public void CompleteSuccess(T val)
      {
         if (done)
         {
            return;
         }

         _val = val;
         done = true;
         try
         {
            _callbacks?.Invoke(val);
         }
         catch (Exception e)
         {
            // TODO: This breaks on Android device because no adapter is loaded in for Android. ~ACM 2020-10-19
            // See https://disruptorbeam.atlassian.net/browse/BEAM-698
            Debug.LogException(e);
         }

         _callbacks = null;
         errbacks = null;
      }

      public void CompleteError(Exception ex)
      {
         if (done)
         {
            return;
         }

         err = ex;
         done = true;



         try
         {
            if (!HadAnyErrbacks)
            {
               InvokeUncaughtPromise();
            }
            else
            {
               errbacks?.Invoke(ex);
            }
         }
         catch (Exception e)
         {
            Debug.LogException(e);
         }

         _callbacks = null;
         errbacks = null;

      }

      public Promise<T> Then(Action<T> callback)
      {

         if (done)
         {
            if (err == null)
            {
               try
               {
                  callback(_val);
               }
               catch (Exception e)
               {
                  Debug.LogException(e);
               }
            }
         }
         else
         {
            _callbacks += callback;
         }

         return this;
      }

      public Promise<T> Error(Action<Exception> errback)
      {
         HadAnyErrbacks = true;
         if (done)
         {
            if (err != null)
            {
               try
               {
                  errback(err);
               }
               catch (Exception e)
               {
                  Debug.LogException(e);
               }
            }
         }
         else
         {
            errbacks += errback;
         }

         return this;
      }

      public Promise<TU> Map<TU>(Func<T, TU> callback)
      {
         var result = new Promise<TU>();
         Then(value =>
            {
               try
               {
                  var nextResult = callback(value);
                  result.CompleteSuccess(nextResult);
               }
               catch (Exception ex)
               {
                  result.CompleteError(ex);
               }
            })
            .Error(ex => result.CompleteError(ex));
         return result;
      }

      public Promise<TU> FlatMap<TU>(Func<T, Promise<TU>> callback)
      {
         var result = new Promise<TU>();
         Then(value =>
         {
            try
            {
               callback(value)
                  .Then(valueInner => result.CompleteSuccess(valueInner))
                  .Error(ex => result.CompleteError(ex));
            }
            catch (Exception ex)
            {
               result.CompleteError(ex);
            }
         }).Error(ex =>
         {
            result.CompleteError(ex);
         });
         return result;
      }

      public static Promise<T> Successful(T value)
      {
         return new Promise<T>
         {
            done = true,
            _val = value
         };
      }

      public static Promise<T> Failed(Exception err)
      {
         return new Promise<T>
         {
            done = true,
            err = err
         };
      }

      void ICriticalNotifyCompletion.UnsafeOnCompleted(Action continuation)
      {
         Then(_ => continuation());
         Error(_ => continuation());
      }

      void INotifyCompletion.OnCompleted(Action continuation)
      {
         ((ICriticalNotifyCompletion)this).UnsafeOnCompleted(continuation);
      }


      public T GetResult()
      {
         if (err != null)
            throw err;
         return _val;
      }

      public Promise<T> GetAwaiter()
      {
         return this;
      }
   }

   public static class Promise
   {
      public static Promise<List<T>> Sequence<T>(IList<Promise<T>> promises)
      {
         var result = new Promise<List<T>>();
         var replies = new List<T>();

         if (promises == null || promises.Count == 0)
         {
            result.CompleteSuccess(replies);
            return result;
         }

         for (var i = 0; i < promises.Count; i++)
         {
            promises[i].Then(reply =>
            {
               replies.Add(reply);
               if (replies.Count == promises.Count)
               {
                  result.CompleteSuccess(replies);
               }
            }).Error(err => result.CompleteError(err));
         }

         return result;
      }

      public static Promise<List<T>> Sequence<T>(params Promise<T>[] promises)
      {
         return Sequence((IList<Promise<T>>)promises);
      }

      /// <summary>
      /// Given a list of promise generator functions, process the whole list, but serially.
      /// Only one promise will be active at any given moment.
      /// </summary>
      /// <param name="generators"></param>
      /// <typeparam name="T"></typeparam>
      /// <returns>A single promise of Unit to represent the completion of the processing. Any other side effects need to be handled separately</returns>
      public static Promise<Unit> ExecuteSerially<T>(List<Func<Promise<T>>> generators)
      {
         if (generators.Count == 0)
         {
            return Promise<Unit>.Successful(PromiseBase.Unit);
         }
         else
         {
            var first = generators[0];
            var rest = generators.GetRange(1, generators.Count - 1);
            var promise = first();
            return promise.FlatMap(_ => ExecuteSerially(rest));
         }
      }
   }

   public static class PromiseExtensions
   {
      public static Promise<T> Recover<T>(this Promise<T> promise, Func<Exception, T> callback)
      {
         var result = new Promise<T>();
         promise.Then(value => result.CompleteSuccess(value))
            .Error(err => result.CompleteSuccess(callback(err)));
         return result;
      }

      public static Promise<T> RecoverWith<T>(this Promise<T> promise, Func<Exception, Promise<T>> callback)
      {
         var result = new Promise<T>();
         promise.Then(value => result.CompleteSuccess(value)).Error(err =>
         {
            try
            {
               var nextPromise = callback(err);
               nextPromise.Then(value => result.CompleteSuccess(value)).Error(errInner =>
               {
                  result.CompleteError(errInner);
               });
            }
            catch (Exception ex)
            {
               result.CompleteError(ex);
            }
         });
         return result;
      }

      public static Promise<T> ToPromise<T>(this System.Threading.Tasks.Task<T> task)
      {
         var promise = new Promise<T>();

         async void Helper()
         {
            try
            {
               var result = await task;
               promise.CompleteSuccess(result);
            }
            catch (Exception ex)
            {
               promise.CompleteError(ex);
            }
         }

         Helper();

         return promise;
      }

      public static Promise<Unit> ToUnit<T>(this Promise<T> self)
      {
         return self.Map(_ => PromiseBase.Unit);
      }


   }

   public class UncaughtPromiseException : Exception
   {
      public PromiseBase Promise { get; }

      public UncaughtPromiseException(PromiseBase promise, Exception ex) : base(
         $"Uncaught promise innerMsg=[{ex.Message}]", ex)
      {
         Promise = promise;
      }
   }

   public readonly struct Unit
   {
   }
}