﻿using UnityEngine;

namespace Beamable.Samples.TBF.Data
{
   /// <summary>
   /// Store the common configuration for easy editing ats
   /// EditTime and RuntTime with the Unity Inspector Window.
   /// </summary>
   [CreateAssetMenu(
      fileName = Title,
      menuName = ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_SAMPLES + "/" +
      "Multiplayer/Create New " + Title,
      order = ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
   public class Configuration : ScriptableObject
   {
      //  Constants  -----------------------------------
      private const string Title = "TFB Configuration";

      //  Properties -----------------------------------
      public string IntroSceneName { get { return _introSceneName; } }
      public string GameSceneName { get { return _gameSceneName; } }
      public string LobbySceneName { get { return _lobbySceneName; } }
      public float DelayBeforeLoadScene { get { return _delayBeforeLoadScene; } }
      //
      public float DelayFadeInUI { get { return _delayFadeInUI; } }

      public float DelayGameBeforeMove { get { return _delayGameBeforeMove; } }
      public float DelayGameMaxDuringMove { get { return _delayGameMaxDuringMove; } }
      public float DelayGameAfterMove { get { return _delayGameAfterMove; } }

      //  Fields ---------------------------------------
      [Header("Scene Names")]
      [SerializeField]
      private string _introSceneName = "";

      [SerializeField]
      private string _lobbySceneName = "";

      [SerializeField]
      private string _gameSceneName = "";

      [Header("Cosmetic Delays")]
      [SerializeField]
      private float _delayBeforeLoadScene = 0;

      [Header("Cosmetic Animation")]
      [SerializeField]
      private float _delayFadeInUI = 0.25f;

      [Header("Game Data")]
      [SerializeField]
      private float _delayGameBeforeMove = 1;

      [SerializeField]
      private float _delayGameMaxDuringMove = 10;

      [SerializeField]
      private float _delayGameAfterMove = 1;

      [SerializeField]
      private float _delayMatchmakingMax = 10f;

      [Header("Mock Data")]
      [SerializeField]
      private string _avatarTitle_01 = "You";

      [SerializeField]
      private string _avatarTitle_02 = "Opponent";
   }
}