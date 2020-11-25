using System;
using System.Collections.Generic;
using Beamable.Common;

namespace Beamable.Platform.SDK.Chat
{
   public class ChatService
   {
      private readonly PlatformService _platform;
      private PlatformRequester _requester;
      private const string BaseUri = "/object/chatV2";

      public ChatService (PlatformService platform, PlatformRequester requester)
      {
         _platform = platform;
         _requester = requester;
      }

      public Promise<Message> SendMessage(string roomId, string message)
      {
         return _requester.Request<SendChatResponse>(
            Method.POST,
            string.Format("{0}/{1}/messages", BaseUri, _platform.User.id),
            new SendChatRequest(roomId, message)
         ).Map(response => response.message);
      }

      public Promise<List<RoomInfo>> GetMyRooms()
      {
         return _requester.Request<GetMyRoomsResponse>(
            Method.GET,
            string.Format("{0}/{1}/rooms", BaseUri, _platform.User.id)
         ).Map(response => response.rooms);
      }

      public Promise<RoomInfo> CreateRoom(string roomName, bool keepSubscribed, List<long> players)
      {
         return _requester.Request<CreateRoomResponse>(
            Method.POST,
            string.Format("{0}/{1}/rooms", BaseUri, _platform.User.id),
            new CreateRoomRequest(roomName, keepSubscribed, players)
         ).Map(response => response.room);
      }

      public Promise<EmptyResponse> LeaveRoom(string roomId)
      {
         return _requester.Request<EmptyResponse>(
            Method.DELETE,
            string.Format("{0}/{1}/rooms?roomId={2}", BaseUri, _platform.User.id, roomId)
         );
      }

      public Promise<EmptyResponse> ProfanityAssert(string text)
      {
         return _requester.Request<EmptyResponse>(
            Method.GET,
            $"/basic/chat/profanityAssert?text={text}"
         );
      }
   }

   [Serializable]
   public class SendChatRequest
   {
      public string roomId;
      public string content;

      public SendChatRequest(string roomId, string content)
      {
         this.roomId = roomId;
         this.content = content;
      }
   }

   [Serializable]
   public class SendChatResponse
   {
      public Message message;
   }

   [Serializable]
   public class GetMyRoomsResponse
   {
      public List<RoomInfo> rooms;
   }

   [Serializable]
   public class CreateRoomRequest
   {
      public string roomName;
      public bool keepSubscribed;
      public List<long> players;

      public CreateRoomRequest(string roomName, bool keepSubscribed, List<long> players)
      {
         this.roomName = roomName;
         this.keepSubscribed = keepSubscribed;
         this.players = players;
      }
   }

   [Serializable]
   public class CreateRoomResponse
   {
      public RoomInfo room;
   }

   [Serializable]
   public class RoomInfo
   {
      public string id;
      public string name;
      public bool keepSubscribed;
      public List<long> players;
   }
}