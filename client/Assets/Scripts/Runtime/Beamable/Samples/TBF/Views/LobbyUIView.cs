﻿using Beamable.Samples.TBF.Animation;
using Beamable.Samples.TBF.Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Samples.TBF.Views
{
   /// <summary>
   /// Handles the audio/graphics rendering logic: Lobby UI
   /// </summary>
   public class LobbyUIView : MonoBehaviour
   {
      //  Properties -----------------------------------
      public TMP_Text StatusBodyText { get { return _statusBodyText; } }
      public Button BackButton { get { return _backButton; } }

      //  Fields ---------------------------------------
      [SerializeField]
      private Configuration _configuration = null;

      [SerializeField]
      private Button _startGameButton = null;

      [SerializeField]
      private Button _backButton = null;

      [SerializeField]
      private TMP_Text _statusBodyText = null;

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