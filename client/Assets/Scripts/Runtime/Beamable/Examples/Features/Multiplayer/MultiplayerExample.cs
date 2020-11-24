using Core.Platform.SDK.Sim;
using DisruptorBeam;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Examples.Features.Multiplayer
{
   public class MyPlayerMoveEvent
   {
      public static string Name = "MyPlayerMoveEvent";
      private long PlayerDbid;
      public Vector3 Position;

      public MyPlayerMoveEvent(long playerDbid, Vector3 position)
      {
         PlayerDbid = playerDbid;
         Position = position;
      }
   }

   public class MultiplayerExample : MonoBehaviour
   {
      private const long FramesPerSecond = 20;
      private const long TargetNetworkLead = 4;
      private const string RoomId = "MyCustomRoomId";

      private SimClient _simClient;
      private string _sessionSeed;
      private long _currentFrame;
      private List<string> _sessionPlayerDbids = new List<string>();
      private long _localPlayerDbid;

      protected void Start()
      {
         // Access Local Player Information
         DisruptorEngine.Instance.Then(de =>
         {
            _localPlayerDbid = de.User.id;
         });

         // Create Multiplayer Session
         _simClient = new SimClient(new SimNetworkEventStream(RoomId),
            FramesPerSecond, TargetNetworkLead);

         // Handle Common Events
         _simClient.OnInit(SimClient_OnInit);
         _simClient.OnConnect(SimClient_OnConnect);
         _simClient.OnDisconnect(SimClient_OnDisconnect);
         _simClient.OnTick(SimClient_OnTick);
      }

      protected void Update()
      {
         if (_simClient != null)
         {
            _simClient.Update();
         }

         // Send Custom Events
         if (Input.GetMouseButtonDown(0))
         {
            _simClient.SendEvent(MyPlayerMoveEvent.Name,
               new MyPlayerMoveEvent(_localPlayerDbid, new Vector3(0, 0, 0)));
         }

         string message = "";
         message += $"Room: {RoomId}\n";
         message += $"Seed: {_sessionSeed}\n";
         message += $"Frame: {_currentFrame}\n";
         message += $"Dbids:";
         foreach (var dbid in _sessionPlayerDbids)
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
         _sessionSeed = sessionSeed;
         Debug.Log($"SimClient_OnInit(): {RoomId} {sessionSeed}");
      }

      private void SimClient_OnConnect(string dbid)
      {
         _sessionPlayerDbids.Add(dbid);

         // Handle Custom Events for EACH dbid
         _simClient.On<MyPlayerMoveEvent>(MyPlayerMoveEvent.Name, dbid,
            SimClient_OnMyPlayerMoveEvent);

         Debug.Log($"SimClient_OnConnect(): {dbid}");
      }

      private void SimClient_OnDisconnect(string dbid)
      {
         _sessionPlayerDbids.Remove(dbid);
         Debug.Log($"SimClient_OnDisconnect(): {dbid}");
      }

      private void SimClient_OnTick(long currentFrame)
      {
         _currentFrame = currentFrame;
      }

      private void SimClient_OnMyPlayerMoveEvent(MyPlayerMoveEvent myPlayerMoveEvent)
      {
         Debug.Log($"SimClient_OnMyPlayerMoveEvent(): {myPlayerMoveEvent}");
      }
   }
}