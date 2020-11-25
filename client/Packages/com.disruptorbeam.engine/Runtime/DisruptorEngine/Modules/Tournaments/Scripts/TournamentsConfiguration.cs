using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Modules.Tournaments
{
    [CreateAssetMenu(
        fileName="Tournament Configuration",
        menuName= ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_CONFIGURATIONS + "/" +
                  "Tournament Configuration",
        order= ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
    public class TournamentsConfiguration : ModuleConfigurationObject
    {
        public static TournamentsConfiguration Instance => Get<TournamentsConfiguration>();

        public List<TournamentInfoPageSection> Info;

    }

    [System.Serializable]
    public class TournamentInfoPageSection
    {
        public string Title;
        [TextArea(4, 12)]
        public string Body;
        public string DetailTitle;
        public TournamentInfoDetailBehaviour DetailPrefab;
    }


}
