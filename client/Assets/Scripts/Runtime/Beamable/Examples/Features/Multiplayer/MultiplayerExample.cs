using Beamable.Platform.SDK.Sim;
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
      

      private SimClient _simClient;
      private string _sessionSeed;
      private long _currentFrame;
      private List<string> _sessionPlayerDbids = new List<string>();
      private long _localPlayerDbid;
      private string roomId = "";

      protected void Start()
      {
         // Access Local Player Information
         Beamable.API.Instance.Then(de =>
         {
            _localPlayerDbid = de.User.id;
         });

         //Randomize the roomId so that every play-session
         //The game connects to a new room with no previous event history.
         //This is optional, and ideal for a 'clean' demo.
         roomId = "MyCustomRoomId_" + UnityEngine.Random.Range(100, 200);

         // Create Multiplayer Session
         _simClient = new SimClient(new SimNetworkEventStream(roomId),
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
            Debug.Log($"SendEvent() for {MyPlayerMoveEvent.Name}.");
            _simClient.SendEvent(MyPlayerMoveEvent.Name,
               new MyPlayerMoveEvent(_localPlayerDbid, new Vector3(0, 0, 0)));
         }

         // More debug info. 
         string message = "";
         message += $"Room: {roomId}\n";
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
         Debug.Log($"SimClient_OnInit(): {roomId} {sessionSeed}");
      }

      private void SimClient_OnConnect(string dbid)
      {
         _sessionPlayerDbids.Add(dbid);

         // Handle Custom Events for EACH dbid
         _simClient.On<MyPlayerMoveEvent>(MyPlayerMoveEvent.Name, dbid,
            SimClient_OnMyPlayerMoveEvent);

         Debug.Log($"SimClient_OnConnect(): {dbid}");
         Debug.Log($"Click/Tap onscreen to send example event.");
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