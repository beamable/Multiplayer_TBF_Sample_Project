using Beamable.Content;
using Beamable.Api.Payments;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Modules.Shop.Defaults
{
   public class ObtainSummaryListCurrencyRenderer : MonoBehaviour
   {
      public RawImage Icon;
      public TextMeshProUGUI Quantity;
      public TextMeshProUGUI Name;

      public async void RenderObtainCurrency(ObtainCurrency data)
      {
         Name.text = data.symbol.Split('.')[1];
         Quantity.text = data.amount.ToString();

         var contentRef = new CurrencyRef();
         contentRef.Id = data.symbol;
         var currency = await contentRef.Resolve();
         var icon = await currency.Icon.LoadAssetAsync().Task;
         Icon.texture = icon.texture;
      }
   }
}