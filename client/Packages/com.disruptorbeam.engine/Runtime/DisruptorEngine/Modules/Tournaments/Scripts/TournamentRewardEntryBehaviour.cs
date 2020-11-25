using System.Collections;
using System.Collections.Generic;
using Beamable.Content;
using Beamable.UI.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Modules.Tournaments
{
    public class TournamentRewardEntryBehaviour : MonoBehaviour
    {
        public Image Image;
        public TextReference AmountText;
        public Material GreyMaterial;
        private OfferObtainCurrency _data;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Set(TournamentEntryViewData owner, OfferObtainCurrency data)
        {
            _data = data;
            var currencyRef = new CurrencyRef
            {
                Id = data.symbol
            };

            currencyRef.Resolve().Then(async currency =>
            {
                var sprite = await currency.Icon.LoadAssetAsync().Task;
                if (!Image) return;
                Image.sprite = sprite;
                Image.material = owner.IsGrey ? GreyMaterial : null;
                if (!AmountText) return;
                AmountText.Value = TournamentScoreUtil.GetShortScore((ulong)data.amount);
            });
        }
    }
}