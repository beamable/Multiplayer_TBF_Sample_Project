using Beamable.Api.Sim;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Examples.Features.Multiplayer
{
   /// <summary>
   /// Custom move
   /// </summary>
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

   /// <summary>
   /// Demonstrates send/receive of events to Beamable Multiplayer.
   /// </summary>
   public class MultiplayerExample : MonoBehaviour
   {
      //  Constants ---------------------------------------
      private const long FramesPerSecond = 20;
      private const long TargetNetworkLead = 4;

      //  Fields  -----------------------------------------
      private SimClient _simClient;
      private string _sessionSeed;
      private long _currentFrame;
      private List<string> _sessionPlayerDbids = new List<string>();
      private long _localPlayerDbid;
      private string roomId = "";

      //  Unity Methods -----------------------------------
      protected void Start()
      {
         // Access Local Player Information
         Beamable.API.Instance.Then(de =>
         {
            _localPlayerDbid = de.User.id;
         });

         roomId = GetRandomRoomId();

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

      //  Other Methods -----------------------------------

      /// <summary>
      /// During development, if the game scene is loaded directly (and thus no matchmaking)
      /// this method is used to give a RoomId. Why random? So that each connection is fresh
      /// and has no history. Otherwise a new connection (within 10-15 seconds of the last connection)
      /// may remember the 'old' session and contain 'old' events.
      /// </summary>
      public static string GetRandomRoomId()
      {
         return "MyCustomRoomId_" + string.Format("{00:00}", UnityEngine.Random.Range(0, 1000));
      }

      //  Event Handlers ----------------------------------

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