using System;
using System.Collections;
using System.Collections.Generic;
using Beamable.Api.Calendars;
using Beamable.Common.Api.Calendars;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Modules.Calendars
{
   public class CalendarRewardsDay : MonoBehaviour
   {
      public Image RewardImage;
      public TextMeshProUGUI Name;
      private Image Background;

      public void Awake()
      {
         Background = gameObject.GetComponent<Image>();
      }

      public void setRewardForDay(RewardCalendarDay day, ClaimStatus claimStatus)
      {
         // TODO: At some point this whole thing should be replaced with something much better
         var obtain = day.obtain[0];
         Name.text = obtain.specialization;
         if (claimStatus == ClaimStatus.CLAIMED)
         {
            Background.color = Color.red;
         }
         else if (claimStatus == ClaimStatus.CLAIMABLE)
         {
            Background.color = Color.green;
         }
      }
   }
}

public enum ClaimStatus
{
   CLAIMED,
   CLAIMABLE,
   TOBECLAIMED
}
