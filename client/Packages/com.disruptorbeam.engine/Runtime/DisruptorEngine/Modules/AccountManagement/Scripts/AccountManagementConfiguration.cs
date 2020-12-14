using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Stats;
using Beamable.Platform.SDK;
using Beamable.Platform.SDK.Auth;
using UnityEngine;

namespace Beamable.Modules.AccountManagement
{
   [CreateAssetMenu(
      fileName="Account Management Configuration",
      menuName= ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_CONFIGURATIONS + "/" +
      "Account Management Configuration",
      order= ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
   public class AccountManagementConfiguration : ModuleConfigurationObject
   {
      public struct UserThirdPartyAssociation
      {
         public AuthThirdParty ThirdParty;
         public bool HasAssociation;
         public bool ThirdPartyEnabled;

         public bool ShouldShowIcon => ThirdPartyEnabled && HasAssociation;
         public bool ShouldShowButton => ThirdPartyEnabled && !HasAssociation;
      }

      public static AccountManagementConfiguration Instance => Get<AccountManagementConfiguration>();

      public bool Facebook, Apple, Google;

      [Tooltip("Google Cloud client ID from web application API credentials. https://console.cloud.google.com/apis/credentials")]
      public string GoogleClientID;

      [Tooltip("The stat to use to show account names")]
      public StatObject DisplayNameStat;

      [Tooltip("The label to use next to the sub text")]
      public string SubtextLabel = "Progress";

      [Tooltip("The stat to use to show account sub text")]
      public StatObject SubtextStat;

      [Tooltip("The stat to use to hold an avatar addressable sprite asset")]
      public StatObject AvatarStat;

      [Tooltip("Allows you to override specific account management functionality")]
      [SerializeField]
      private AccountManagementAdapter _overrides;

      [Tooltip("The max character limit of a player's alias")]
      public int AliasCharacterLimit = 18;

      [Tooltip("Controls the presence of the promotional banner on the main menu of account management.")]
      public bool ShowPromotionalSlider = false;

      public AccountManagementAdapter Overrides
      {
         get
         {
            if (_overrides == null)
            {
               if (_overrides == null)
               {
                  var gob = new GameObject();
                  _overrides = gob.AddComponent<AccountManagementAdapter>();
               }
            }
            return _overrides;
         }
      }

      public bool Has(AuthThirdParty thirdParty)
      {
         switch (thirdParty)
         {
            case AuthThirdParty.Facebook:
               return Facebook;
            case AuthThirdParty.Apple:
#if UNITY_IOS
               return Apple;
#else
               return false;
#endif // UNITY_IOS
            case AuthThirdParty.Google:
#if UNITY_ANDROID
               return Google;
#else
               return false;
#endif // UNITY_ANDROID
            default:
               return false;
         }
      }

      public Promise<List<UserThirdPartyAssociation>> GetAllEnabledThirdPartiesForUser(User user)
      {
         // for each user, we need to run a promise out
         var promises = new List<Promise<UserThirdPartyAssociation>>();

         var thirdParties = (AuthThirdParty[])Enum.GetValues(typeof(AuthThirdParty));
         foreach (var thirdParty in thirdParties)
         {
            if (!Has(thirdParty))
            {
               promises.Add(Promise<UserThirdPartyAssociation>.Successful(new UserThirdPartyAssociation
               {
                  HasAssociation = false,
                  ThirdParty = thirdParty,
                  ThirdPartyEnabled = false
               }));
            }
            else
            {
               // TODO, somehow we should be able to cache this fact, so we don't keep on pinging apis.
               promises.Add(Overrides.DoesUserHaveThirdParty(user, thirdParty).Map(hasThirdParty =>
               new UserThirdPartyAssociation
               {
                  HasAssociation = hasThirdParty,
                  ThirdParty = thirdParty,
                  ThirdPartyEnabled = true
               }));
            }
         }

         return Promise.Sequence(promises);
      }
   }
}
