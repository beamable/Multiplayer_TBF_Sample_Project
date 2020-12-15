using Beamable.Api.Sim;
using Beamable.Samples.TBF.Multiplayer.Events;
using System.Collections.Generic;
using UnityEngine;
using static Beamable.Api.Sim.SimClient;

namespace Beamable.Samples.TBF.Multiplayer
{
   public class TBFMultiplayerSession
   {
      //  Fields ---------------------------------------
      public event EventCallback<long> OnInit;
      public event EventCallback<long> OnConnect;
      public event EventCallback<long> OnDisconnect;

      /// <summary>
      /// Determines if events objects are transfered with fullly qualified event names.
      /// True, is more correct.
      /// False, is easier debug logging.
      /// </summary>
      private static bool IsNamespaceSensitive = false;

      private const long FramesPerSecond = 20;
      private const long TargetNetworkLead = 4;

      public long SessionSeed { get { return _sessionSeed; } }
      public List<long> PlayerDbids { get { return _playerDbids; } }
      public int TargetPlayerCount { get { return _targetPlayerCount; } }

      public bool IsLocalPlayerDbid (long dbid) { return dbid == _localPlayerDbid; }

      private long _sessionSeed;
      private List<long> _playerDbids = new List<long>();
      private SimClient _simClient;
      private long _currentFrame;
      private long _localPlayerDbid;
      private string _roomId;
      private int _targetPlayerCount;

      //  Constructor   --------------------------------
      public TBFMultiplayerSession(long localPlayerDbid, int targetPlayerCount, string roomId)
      {
         _roomId = roomId;
         _localPlayerDbid = localPlayerDbid;
         _targetPlayerCount = targetPlayerCount;
      }

      //  Other Methods   ------------------------------

      /// <summary>
      /// Initialize the <see cref="SimClient"/>.
      /// </summary>
      public void Initialize()
      {
         // Create Multiplayer Session
         _simClient = new SimClient(new SimNetworkEventStream(_roomId),
            FramesPerSecond, TargetNetworkLead);

         // Handle Common Events
         _simClient.OnInit(SimClient_OnInit);
         _simClient.OnConnect(SimClient_OnConnect);
         _simClient.OnDisconnect(SimClient_OnDisconnect);
         _simClient.OnTick(SimClient_OnTick);
      }

      /// <summary>
      /// Convenience. Wrap <see cref="SimClient"/> method.
      /// </summary>
      public void Update()
      {
         if (_simClient != null)
         {
            _simClient.Update();
         }

         string message = "";
         message += $"Room: {_roomId}\n";
         message += $"Seed: {_sessionSeed}\n";
         message += $"Frame: {_currentFrame}\n";
         message += $"Dbids:";
         foreach (var dbid in _playerDbids)
         {
            message += $"{dbid},";
         }
         //DebugLog($"message:{message}");
      }


      /// <summary>
      /// Convenience. Wrap <see cref="SimClient"/> method.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="callback"></param>
      /// <returns></returns>
      public EventCallback<string> On<T>(string origin, EventCallback<T> callback) where T : TBFEvent
      {
         string name = GetEventName<T>();
         DebugLog($"SimClient_On(): {name}");
         return _simClient.On<T>(GetEventName<T>(), origin, callback);
         
      }

      /// <summary>
      /// Convenience. Wrap <see cref="SimClient"/> method.
      /// </summary>
      /// <param name="callback"></param>
      public void Remove(EventCallback<string> callback)
      {
         _simClient.Remove(callback);
      }


      /// <summary>
      /// Convenience. Wrap <see cref="SimClient"/> method.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="evt"></param>
      public void SendEvent<T>(T evt) where T : TBFEvent
      {
         string name = GetEventName<T>();
         DebugLog($"SimClient_SendEvent(): {name}");
         _simClient.SendEvent(name, evt);
      }

      //  Private Methods  -----------------------------
      private void DebugLog(string message)
      {
         if (TBFConstants.IsDebugLogging)
         {
            Debug.Log(message);
         }
      }


      /// <summary>
      /// Convert <see cref="TBFEvent"/> name subclass
      /// for server transfer.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <returns></returns>
      private string GetEventName<T>() where T : TBFEvent
      {
         if (IsNamespaceSensitive)
         {
            return typeof(T).FullName.ToString();
         }
         else
         {
            return typeof(T).Name.ToString();
         }
      }


      //  Event Handlers  ------------------------------
      private void SimClient_OnInit(string sessionSeed)
      {
         _sessionSeed = long.Parse(sessionSeed);
         DebugLog($"SimClient_OnInit(): {_roomId} {_sessionSeed}");
         OnInit?.Invoke(_sessionSeed);
      }

      private void SimClient_OnConnect(string dbid)
      {
         _playerDbids.Add(long.Parse(dbid));
         DebugLog($"SimClient_OnConnect(): {long.Parse(dbid)}");
         OnConnect?.Invoke(long.Parse(dbid));
      }

      private void SimClient_OnDisconnect(string dbid)
      {
         _playerDbids.Remove(long.Parse(dbid));
         DebugLog($"SimClient_OnDisconnect(): {long.Parse(dbid)}");
         OnDisconnect?.Invoke(long.Parse(dbid));
      }

      private void SimClient_OnTick(long currentFrame)
      {
         _currentFrame = currentFrame;
      }
   }
}