using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Inventory;

namespace Beamable.Api.Inventory
{
   public class InventorySubscription : PlatformSubscribable<InventoryResponse, InventoryView>
   {
      private const string SERVICE = "inventory";

      private InventoryView view = new InventoryView();

      public InventorySubscription(PlatformService platform, IBeamableRequester requester) : base(platform, requester, SERVICE)
      {
      }

      protected override void OnRefresh(InventoryResponse data)
      {
         data.MergeView(view);
         foreach (var scope in data.GetNotifyScopes())
         {
            Notify(scope, view);
         }
      }
   }

   public class InventoryService : AbsInventoryApi, IHasPlatformSubscriber<InventorySubscription, InventoryResponse, InventoryView>
   {
      public IBeamableRequester Requester { get; }

      public InventorySubscription Subscribable { get; }

      public InventoryService (PlatformService platform, IBeamableRequester requester) : base(requester, platform)
      {
         Subscribable = new InventorySubscription(platform, requester);
         Requester = requester;
      }

      public override Promise<InventoryView> GetCurrent(string scope = "") => Subscribable.GetCurrent(scope);
   }
}

