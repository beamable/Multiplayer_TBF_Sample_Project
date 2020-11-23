using Beamable.Samples.TBF.Animation;
using Beamable.Samples.TBF.Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Samples.TBF.Views
{
   /// <summary>
   /// Handles the audio/graphics rendering logic: Intro UI
   /// </summary>
   public class IntroUIView : MonoBehaviour
   {
      //  Properties -----------------------------------
      public string AboutBodyText
      {
         set
         {
            _aboutBodyText.text = value;
         }
      }

      public Button StartGameButton { get { return _startGameButton; } }

      //  Fields ---------------------------------------
      [SerializeField]
      private Configuration _configuration = null;

      [SerializeField]
      private Button _startGameButton = null;

      [SerializeField]
      private TMP_Text _aboutBodyText = null;

      [Header ("Cosmetic Animation")]
      [SerializeField]
      private List<CanvasGroup> _canvasGroups = null;

      //  Unity Methods   ------------------------------
      protected void Start()
      {
         TweenHelper.CanvasGroupsDoFade(_canvasGroups, 0, 1, 1, 0, _configuration.DelayFadeInUI);
      }
   }
}