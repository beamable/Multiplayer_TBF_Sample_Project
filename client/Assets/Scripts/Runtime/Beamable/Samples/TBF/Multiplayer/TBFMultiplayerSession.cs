using Core.Platform.SDK.Sim;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Core.Platform.SDK.Sim.SimClient;

namespace Beamable.Samples.TBF.Multiplayer
{
   public class TBFMultiplayerSession
   {
      public event EventCallback<long> OnInit;
      public event EventCallback<long> OnConnect;
      public event EventCallback<long> OnDisconnect;

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

      public TBFMultiplayerSession(long localPlayerDbid, int targetPlayerCount, string roomId)
      {
         _roomId = roomId;
         _localPlayerDbid = localPlayerDbid;
         _targetPlayerCount = targetPlayerCount;
      }

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

      public void Tick()
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
         //Debug.Log($"message:{message}");
      }

      protected void OnDestroy()
      {
         if (_simClient != null)
         {
            //TODO: Manually leave session. Needed?
         }
      }

      private void SimClient_OnInit(string sessionSeed)
      {
         _sessionSeed = long.Parse(sessionSeed);
         OnInit?.Invoke(_sessionSeed);
         Debug.Log($"SimClient_OnInit(): {_roomId} {_sessionSeed}");
      }

      private void SimClient_OnConnect(string dbid)
      {
         _playerDbids.Add(long.Parse(dbid));
         OnConnect?.Invoke(long.Parse(dbid));
         Debug.Log($"SimClient_OnConnect(): {dbid}");
      }

      private void SimClient_OnDisconnect(string dbid)
      {
         _playerDbids.Remove(long.Parse(dbid));
         OnDisconnect?.Invoke(long.Parse(dbid));
         Debug.Log($"SimClient_OnDisconnect(): {dbid}");
      }

      private void SimClient_OnTick(long currentFrame)
      {
         _currentFrame = currentFrame;
      }

      private void SimClient_OnMyPlayerMoveEvent(object myPlayerMoveEvent)
      {
         Debug.Log($"SimClient_OnMyPlayerMoveEvent(): {myPlayerMoveEvent}");
      }
   }
}