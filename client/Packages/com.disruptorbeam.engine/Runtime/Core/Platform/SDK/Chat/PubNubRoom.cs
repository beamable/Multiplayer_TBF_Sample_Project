using System.Collections.Generic;
using Beamable.Common;
using Beamable.Platform.SDK.Notification;
using Beamable.Serialization;
using Beamable.Service;
using Beamable.Spew;
using Debug = UnityEngine.Debug;

namespace Beamable.Platform.SDK.Chat
{
   using Promise = Promise<Unit>;

   public class PubNubRoom : Room
   {
      private const string ChatEvent = "CHAT.RECEIVED";
      private bool _isSubscribed;

      public PubNubRoom(RoomInfo roomInfo) : this(roomInfo.id, roomInfo.name, roomInfo.keepSubscribed) {}

      private PubNubRoom(string id, string name, bool keepSubscribed) : base(id, name, keepSubscribed, true) {}

      public override Promise Sync()
      {
         // XXX: This should be a bit smarter in when and/or how often it fetches the history.
         var promise = new Promise();

         var pubnub = ServiceManager.Resolve<PlatformService>().PubnubSubscriptionManager;
         pubnub.LoadChannelHistory(
            Id,
            50,
            pubnubMessages =>
            {
               Messages.Clear();
               foreach (var message in pubnubMessages)
               {
                  OnChatEvent(message);
               }

               promise.CompleteSuccess(PromiseBase.Unit);
            },
            error =>
            {
               Debug.LogError(error.Message);
               promise.CompleteError(new ErrorCode(error.StatusCode));
            }
         );
         return promise;
      }

      public override Promise<Message> SendMessage(string message)
      {
         var platform = ServiceManager.Resolve<PlatformService>();
         return platform.Chat.SendMessage(Id, message);
      }

      public override Promise<Room> Join(OnMessageReceivedDelegate callback = null)
      {
         var basePromise = base.Join(callback);

         if (_isSubscribed)
         {
            return basePromise;
         }

         var promise = new Promise<Room>();

         basePromise.Then(_ =>
         {
            var pubnub = ServiceManager.Resolve<PlatformService>().PubnubSubscriptionManager;
            pubnub.EnqueueOperation(new PubNubOp(PubNubOp.PNO.OpSubscribe, Id, () =>
            {
               _isSubscribed = true;
               ServiceManager.Resolve<PlatformService>().Notification.Subscribe(ChatEvent, OnChatEvent);
               promise.CompleteSuccess(this);
            }), shouldRunNextOp: true);
         });

         return promise;
      }

      public override Promise<Room> Leave()
      {
         var basePromise = base.Leave();

         // Stay in the room if keep subscribe
         if (KeepSubscribed)
         {
            ChatLogger.Log("PubNubRoom: Will not unsubscribe from room marked as 'KeepSubscribed'.");
            return basePromise;
         }

         var promise = new Promise<Room>();

         basePromise.Then(_ =>
         {
            var pubnub = ServiceManager.Resolve<PlatformService>().PubnubSubscriptionManager;
            pubnub.EnqueueOperation(new PubNubOp(PubNubOp.PNO.OpUnsubscribe, Id, () =>
            {
               _isSubscribed = false;
               ServiceManager.Resolve<PlatformService>().Notification.Unsubscribe(ChatEvent, OnChatEvent);
               promise.CompleteSuccess(this);
            }), shouldRunNextOp: true);
         }).Error(promise.CompleteError);

         return promise;
      }

      public override Promise<Room> Forget()
      {
         var basePromise = base.Forget();

         var platform = ServiceManager.Resolve<PlatformService>();
         return basePromise.FlatMap(baseRsp => platform.Chat.LeaveRoom(Id).Map<Room>(rsp => this));
      }

      private void OnChatEvent(object payload)
      {
         var result = new Message();
         using (var ls = ServiceManager.Resolve<StreamFactoryService>()
            .CreateLoadStream(payload as IDictionary<string, object>, JsonSerializable.ListMode.kMerge))
         {
            result.Serialize(ls);
         }

         if (result.roomId == Id && !ContainsMessage(result))
         {
            MessageReceived(result);
         }
      }
   }
}