using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.UI.Scripts;
using Beamable.Platform.SDK;
using Beamable.Platform.SDK.Auth;
using Beamable.Service;
using TMPro;
using UnityEngine;

namespace Beamable.Stats
{
   [System.Serializable]
   public class StatUpdateEvent : UnityEngine.Events.UnityEvent<string> { }

   public class StatBehaviour : MonoBehaviour
   {
      public StatObject Stat;
      public bool RefreshOnStart = false;
      public StatUpdateEvent OnStatReceived;

      private StatObject _lastStat;

      public string Value { get; private set; }

      public long DefaultPlayerDbid { get; private set; } = 0;
      public bool RegisterUserCallback { get; private set; } = true;

      private void Awake()
      {
         if (RegisterUserCallback)
         {
            API.Instance.Then(de =>
            {
               de.OnUserChanged += user =>
               {
                  if (DefaultPlayerDbid == 0)
                  {
                     Refresh(user.id, "");
                  }
               };
            });
         }
      }

      private void OnEnable()
      {
         Stat?.Attach(this);
         _lastStat = Stat;

         if (RefreshOnStart)
         {
            Refresh();
         }
      }

      private void OnDisable()
      {
         Stat?.Detach(this);
      }

      private void Update()
      {
         if (Stat != _lastStat)
         {
            _lastStat?.Detach(this);
            Stat?.Attach(this);
            _lastStat = Stat;
         }
      }

      public void SetForUser(User user)
      {
         if (user == null)
         {
            DefaultPlayerDbid = 0;
            RegisterUserCallback = true;
         }
         else
         {
            DefaultPlayerDbid = user.id;
            RegisterUserCallback = false;
         }
      }

      public void SetForUser(long dbid)
      {
         DefaultPlayerDbid = dbid;
         RegisterUserCallback = false;
      }

      public void SetCurrentValue(string value)
      {
         Value = value;
         OnStatReceived?.Invoke(Value);
      }

      public void Refresh()
      {
         Refresh(0, "");
      }

      public Promise<string> Read(long dbid=0, string noStatValue="")
      {
         if (Stat == null)
         {
            Value = noStatValue;
            OnStatReceived?.Invoke(noStatValue);
            return Promise<string>.Successful(noStatValue);
         }

         return API.Instance.FlatMap(de =>
         {
            var defaultDbid = DefaultPlayerDbid == 0 ? de.User.id : DefaultPlayerDbid;
            return de.Stats.GetStats("client", Stat.Access.GetString(), "player", dbid == 0 ? defaultDbid : dbid).Map(stats =>
            {
               if (!stats.TryGetValue(Stat.StatKey, out var statValue))
               {
                  statValue = Stat.DefaultValue;
               }

               Value = statValue;
               return statValue;
            });
         });
      }

      public Promise<string> Refresh(long dbid, string noStatValue)
      {
         return Read(dbid, noStatValue).Then(stat => OnStatReceived?.Invoke(stat));
      }

      public Promise<Unit> Write(string value)
      {
         return Stat?.Write(value);
      }

      public Promise<Unit> Write(TextReferenceBase text)
      {
         return Stat?.Write(text.Value);
      }

      public void SetStat(StatObject stat)
      {
         Stat = stat;
      }
   }
}