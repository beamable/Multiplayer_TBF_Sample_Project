using Beamable.Samples.TBF.Animation;
using Beamable.Samples.TBF.Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Samples.TBF.Views
{
   /// <summary>
   /// Handles the audio/graphics rendering logic: Avatar UI
   /// </summary>
   public class AvatarUIView : MonoBehaviour
   {
      //  Properties -----------------------------------
      public Button BackButton { get { return _backButton; } }
      public Button ClickMeButton { get { return _clickMeButton; } }
      public TMP_Text StatusText { get { return _statusText; } }

      //  Fields ---------------------------------------
      [SerializeField]
      private Configuration _configuration = null;

      [SerializeField]
      private TMP_Text _statusText = null;

      [SerializeField]
      private Button _backButton = null;

      [SerializeField]
      private Button _clickMeButton = null;

      [Header("Cosmetic Animation")]
      [SerializeField]
      private List<CanvasGroup> _canvasGroups = null;

      //  Unity Methods   ------------------------------
      protected void Start()
      {
         _statusText.text = "";
         TweenHelper.CanvasGroupsDoFade(_canvasGroups, 0, 1, 1, 0, _configuration.DelayFadeInUI);
      }
   }
}