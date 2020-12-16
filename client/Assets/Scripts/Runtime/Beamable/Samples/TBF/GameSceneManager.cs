using Beamable.Samples.TBF.Audio;
using Beamable.Samples.TBF.Data;
using Beamable.Samples.TBF.Multiplayer;
using Beamable.Samples.TBF.Multiplayer.Events;
using Beamable.Samples.TBF.Views;
using System;
using System.Threading.Tasks;
using UnityEngine;
using static Beamable.Samples.TBF.UI.TMP_BufferedText;

// Disable: "Because this call is not awaited, execution of the current method continues before the call is completed"
#pragma warning disable CS4014 

namespace Beamable.Samples.TBF
{

   /// <summary>
   /// List of all users' moves
   /// </summary>
   public enum GameMoveType
   {
      Null = 0,
      High = 10,     // Like "Rock"
      Medium = 20,   // Like "Paper"
      Low = 30       // Like "Scissors"
   }

   /// <summary>
   /// Handles the main scene logic: Game
   /// </summary>
   public class GameSceneManager : MonoBehaviour
   {
      //  Properties -----------------------------------
      public GameUIView GameUIView { get { return _gameUIView; } }
      public GameProgressData GameProgressData { get { return _gameProgressData; } set { _gameProgressData = value; } }
      public Configuration Configuration { get { return _configuration; } }
      public TBFMultiplayerSession MultiplayerSession { get { return _multiplayerSession; } }
      public RemotePlayerAI RemotePlayerAI { get { return _remotePlayerAI; } }

      //  Fields ---------------------------------------

      [SerializeField]
      private Configuration _configuration = null;

      [SerializeField]
      private GameUIView _gameUIView = null;

      private IBeamableAPI _beamableAPI = null;
      private TBFMultiplayerSession _multiplayerSession;
      private GameProgressData _gameProgressData;
      private RemotePlayerAI _remotePlayerAI;
      private GameStateHandler _gameStateHandler;


      //  Unity Methods   ------------------------------
      protected void Start()
      {
         _gameUIView.BackButton.onClick.AddListener(BackButton_OnClicked);
         _gameUIView.MoveButton_01.onClick.AddListener(MoveButton_01_OnClicked);
         _gameUIView.MoveButton_02.onClick.AddListener(MoveButton_02_OnClicked);
         _gameUIView.MoveButton_03.onClick.AddListener(MoveButton_03_OnClicked);

         _gameUIView.AvatarUIViews[TBFConstants.PlayerIndexLocal].HealthBarView.Health = 100;
         _gameUIView.AvatarUIViews[TBFConstants.PlayerIndexRemote].HealthBarView.Health = 100;

         //
         _gameStateHandler = new GameStateHandler(this);
         SetupBeamable();
      }


      protected void Update()
      {
         _multiplayerSession?.Update();
      }

      //  Other Methods  -----------------------------
      private void DebugLog(string message)
      {
         if (TBFConstants.IsDebugLogging)
         {
            Debug.Log(message);
         }
      }


      private async void SetupBeamable()
      {
         _gameStateHandler.SetGameState(GameState.Loading);

         await Beamable.API.Instance.Then(de =>
         {
            _gameStateHandler.SetGameState (GameState.Loaded);

            try
            {
               _beamableAPI = de;

               if (!RuntimeDataStorage.Instance.IsMatchmakingComplete)
               {
                  Debug.Log($"Scene '{gameObject.scene.name}' was loaded directly. That is ok. Setting defaults.");
                  RuntimeDataStorage.Instance.LocalPlayerDbid = _beamableAPI.User.id;
                  RuntimeDataStorage.Instance.TargetPlayerCount = 1;
                  RuntimeDataStorage.Instance.RoomId = TBFMatchmaking.GetRandomRoomId();
               }

               _multiplayerSession = new TBFMultiplayerSession(
                  RuntimeDataStorage.Instance.LocalPlayerDbid,
                  RuntimeDataStorage.Instance.TargetPlayerCount,
                  RuntimeDataStorage.Instance.RoomId) ;

               _gameStateHandler.SetGameState(GameState.Initializing);

               _multiplayerSession.OnInit += MultiplayerSession_OnInit;
               _multiplayerSession.OnConnect += MultiplayerSession_OnConnect;
               _multiplayerSession.OnDisconnect += MultiplayerSession_OnDisconnect;
               _multiplayerSession.Initialize();

            }
            catch (Exception)
            {
               SetStatusText(TBFHelper.InternetOfflineInstructionsText, BufferedTextMode.Immediate);
            }
         });
      }


      public void SetStatusText(string message, BufferedTextMode statusTextMode)
      {
         _gameUIView.BufferedText.SetText(message, statusTextMode);
      }


      private void BindPlayerDbidToEvents(long playerDbid, bool isBinding)
      {
         if (isBinding)
         {
            string origin = playerDbid.ToString();
            _multiplayerSession.On<GameStartEvent>(origin, MultiplayerSession_OnGameStartEvent);
            _multiplayerSession.On<GameMoveEvent>(origin, MultiplayerSession_OnGameMoveEvent);
         }
         else
         {
            _multiplayerSession.Remove<GameStartEvent>(MultiplayerSession_OnGameStartEvent);
            _multiplayerSession.Remove<GameMoveEvent>(MultiplayerSession_OnGameMoveEvent);
         }
      }

      private void SendGameMoveEventSave(GameMoveType gameMoveType)
      {
         if (_gameStateHandler.GameState == GameState.PlayerMoving)
         {
            _gameUIView.MoveButtonsCanvasGroup.interactable = false;
            SoundManager.Instance.PlayAudioClip(SoundConstants.Click02);

            _multiplayerSession.SendEvent<GameMoveEvent>(
               new GameMoveEvent(gameMoveType));
         }
      }

      //  Event Handlers -------------------------------
      private void BackButton_OnClicked()
      {
         //Change scenes
         StartCoroutine(TBFHelper.LoadScene_Coroutine(_configuration.IntroSceneName,
            _configuration.DelayBeforeLoadScene));
      }


      private void MoveButton_01_OnClicked()
      {
         SendGameMoveEventSave(GameMoveType.High);
      }


      private void MoveButton_02_OnClicked()
      {
         SendGameMoveEventSave(GameMoveType.Medium);
      }


      private void MoveButton_03_OnClicked()
      {
         SendGameMoveEventSave(GameMoveType.Low);
      }


      private void MultiplayerSession_OnInit(System.Random random)
      {
         //For simplicity RemotePlayerAI is always created, but only enaabled if needed
         bool isRemotePlayerAIEnabled = _multiplayerSession.IsHumanVsBotMode;
         _remotePlayerAI = new RemotePlayerAI(random, isRemotePlayerAIEnabled);

         _gameStateHandler.SetGameState(GameState.Initialized);
      }


      private void MultiplayerSession_OnConnect(long playerDbid)
      {
         BindPlayerDbidToEvents(playerDbid, true);

         if (_multiplayerSession.PlayerDbidsCount < _multiplayerSession.TargetPlayerCount)
         {
            _gameStateHandler.SetGameState (GameState.Connecting);

         }
         else
         {
            _gameStateHandler.SetGameState (GameState.Connected);

            _multiplayerSession.SendEvent<GameStartEvent>(new GameStartEvent());
         }
      }


      private void MultiplayerSession_OnDisconnect(long playerDbid)
      {
         BindPlayerDbidToEvents(playerDbid, false);

         SetStatusText(string.Format(TBFConstants.StatusText_Multiplayer_OnDisconnect,
            _multiplayerSession.PlayerDbidsCount.ToString(),
            _multiplayerSession.TargetPlayerCount), BufferedTextMode.Immediate);
      }


      private void MultiplayerSession_OnGameStartEvent(GameStartEvent gameStartEvent)
      {
         //TODO: check if I got X responses. Don't check the following...
         if (_multiplayerSession.PlayerDbidsCount == _multiplayerSession.TargetPlayerCount)
         {
            _gameStateHandler.SetGameState (GameState.GameStarted);
         }
         else
         {
            _gameStateHandler.SetGameState (GameState.GameStarting);
         }
      }


      private void MultiplayerSession_OnGameMoveEvent(GameMoveEvent gameMoveEvent)
      {
         //Add each player event to a list
         _gameProgressData.GameMoveEventsThisRoundByPlayerDbid[gameMoveEvent.PlayerDbid] = gameMoveEvent;

         Debug.Log($"gameMoveEvent.GameMoveType(): {gameMoveEvent.GameMoveType} for {gameMoveEvent.PlayerDbid}");
         _gameStateHandler.SetGameState(GameState.PlayerMoved);

      }
   }
}