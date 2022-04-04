using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Samples.TBF.Data
{
   /// <summary>
   /// Store the common configuration for easy editing ats
   /// EditTime and RuntTime with the Unity Inspector Window.
   /// </summary>
   [CreateAssetMenu(fileName = Title, menuName = "Beamable/Samples/Multiplayer/Create New " + Title, order = 0)]
   public class Configuration : ScriptableObject
   {
      //  Constants  -----------------------------------
      private const string Title = "TBF Configuration";

      //  Properties -----------------------------------
      public string IntroSceneName { get { return _introSceneName; } }
      public string GameSceneName { get { return _gameSceneName; } }
      public string LobbySceneName { get { return _lobbySceneName; } }
      public float DelayBeforeLoadScene { get { return _delayBeforeLoadScene; } }
      public float DelayFadeInUI { get { return _delayFadeInUI; } }
      public float DelayBeforeSoundAttack_01a { get { return _delayBeforeSoundAttack_01a; } }
      public float DelayBeforeSoundAttack_01b { get { return _delayBeforeSoundAttack_01b; } }
      public float DelayBeforeSoundAttack_02a { get { return _delayBeforeSoundAttack_02a; } }
      public float DelayBeforeSoundAttack_02b { get { return _delayBeforeSoundAttack_02b; } }
      public float DelayBeforeSoundAttack_03 { get { return _delayBeforeSoundAttack_03; } }
      public float DelayGameBeforeGameOver { get { return _delayGameBeforeGameOver; } }
      public List<AvatarData> AvatarDatas { get { return _avatarDatas; } }
      public int GameRoundsTotal { get { return _gameRoundsTotal; } }


      //  Fields ---------------------------------------
      [Header("Scene Names")]
      [SerializeField]
      private string _introSceneName = "";

      [SerializeField]
      private string _lobbySceneName = "";

      [SerializeField]
      private string _gameSceneName = "";

      [Header("Game Data")]
      [Range (1,2)]
      [SerializeField]
      private int _targetPlayerCount = 2;

      [SerializeField]
      private int _gameRoundsTotal = 3;

      [SerializeField]
      private float _delayGameBeforeMove = 1;

      [SerializeField]
      private float _delayGameMaxDuringMove = 10;

      [SerializeField]
      private float _delayGameAfterMove = 1;

      [SerializeField]
      private float _delayGameBeforeGameOver = 3;

      [Header("Cosmetic Data")]
      [SerializeField]
      private List<AvatarData> _avatarDatas = null;

      [Header("Cosmetic Delays")]
      [SerializeField]
      private float _delayBeforeLoadScene = 0;

      [SerializeField]
      private float _delayBeforeSoundAttack_01a = 1;


      [SerializeField]
      private float _delayBeforeSoundAttack_01b = 1;

      [SerializeField]
      private float _delayBeforeSoundAttack_02a = 1;

      [SerializeField]
      private float _delayBeforeSoundAttack_02b = 1;

      [SerializeField]
      private float _delayBeforeSoundAttack_03 = 1;

      [Range (0,3)]
      [SerializeField]
      public float _statusMessageMinDuration = 3000;

      [Header("Cosmetic Animation")]
      [SerializeField]
      private float _delayFadeInUI = 0.25f;

      //  Unity Methods ---------------------------------------
      protected void OnValidate()
      {
         _targetPlayerCount = Mathf.Clamp(_targetPlayerCount, 1, 2);
      }
   }
}
