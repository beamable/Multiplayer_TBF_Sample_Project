using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Beamable.Common;
using Beamable.Platform.SDK;
using UnityEngine;

namespace Beamable.Stats
{
   public enum StatAccess
   {
      Public, Private
   }

   public static class StatAccessExtensions
   {
      public static string GetString(this StatAccess access)
      {
         switch (access)
         {
            case StatAccess.Private:
               return "private";
            case StatAccess.Public:
               return "public";
            default:
               throw new Exception("unknown stat access");
         }
      }
   }

   [CreateAssetMenu(
      fileName = "Stat",
      menuName = ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE + "/" +
      "Stat",
      order = ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
   [System.Serializable]
   public class StatObject : ScriptableObject
   {
      [Tooltip("The lookup value for a statistic")]
      public string StatKey;

      public StatAccess Access = StatAccess.Public;

      [Tooltip("When a player has no stat, this default will be used instead.")]
      public string DefaultValue;

      public bool ProfanityChecked = false;


      private List<StatBehaviour> _listeners = new List<StatBehaviour>();


      public Promise<Unit> Write(string value)
      {
         return API.Instance.FlatMap(de =>
         {
            Promise<Unit> profanityPromise;
            if (ProfanityChecked)
            {
               profanityPromise = de.ChatService.ProfanityAssert(value).Map(empty => PromiseBase.Unit);
            }
            else
            {
               profanityPromise = Promise<Unit>.Successful(PromiseBase.Unit);
            }

            return profanityPromise.FlatMap(unit =>
            {
               var writeOperation = de.Stats.SetStats(Access.GetString(), new Dictionary<string, string> {{StatKey, value}});

               writeOperation.Then(_ =>
               {
                  _listeners.ForEach(l => l.Refresh());
               });
               return writeOperation.Map(_ => PromiseBase.Unit);
            });
         });

      }

      public void Attach(StatBehaviour behaviour)
      {
         if (!_listeners.Contains(behaviour))
            _listeners.Add(behaviour);
      }

      public void Detach(StatBehaviour behaviour)
      {
         _listeners.Remove(behaviour);
      }

   }
}