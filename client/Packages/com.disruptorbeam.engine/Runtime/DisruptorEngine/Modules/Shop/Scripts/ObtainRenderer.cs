using Beamable.Platform.SDK.Payments;
using UnityEngine;

namespace Beamable.Modules.Shop
{
   public abstract class ObtainRenderer : MonoBehaviour
   {
      public abstract void RenderObtain(PlayerListingView data);
   }
}