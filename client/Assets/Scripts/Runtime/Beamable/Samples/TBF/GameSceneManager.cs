using Beamable.Content;
using Beamable.Samples.TBF.Data;
using Beamable.Samples.TBF.Multiplayer;
using Beamable.Samples.TBF.Multiplayer.Events;
using Beamable.Samples.TBF.Views;
using System;
using System.Collections;
using UnityEngine;

namespace Beamable.Samples.TBF
{
   public enum GameMoveType
   {
      High,
      Medium,
      Low
   }
   public enum GameState
   {
      Null,
      Loading,
      Loaded,
      Initializing,
      Initialized,
      Connecting,
      Connected,
      Starting,
      Started,
      Moving,
      Ending,
      
   }
   /// <summary>
   /// Handles the main scene logic: Game
   /// </summary>
   public class GameSceneManager : MonoBehaviour
   {
      //  Fields ---------------------------------------
      [SerializeField]
      private Configuration _configuration = null;

      [SerializeField]
      private GameUIView _gameUIView = null;

      private Coroutine _runGameCoroutine;
      private float _gameTimeRemaining = 0;
      private LeaderboardContent _leaderboardContent;
      private IBeamableAPI _beamableAPI = null;
      private TBFMultiplayerSession _tbfMultiplayerSession;
      private GameState _gameState = GameState.Starting;

      //  Unity Methods   ------------------------------
      protected void Start ()
      {
         _gameUIView.BackButton.onClick.AddListener(BackButton_OnClicked);
         _gameUIView.MoveButton_01.onClick.AddListener(MoveButton_01_OnClicked);
         _gameUIView.MoveButton_02.onClick.AddListener(MoveButton_02_OnClicked);
         _gameUIView.MoveButton_03.onClick.AddListener(MoveButton_03_OnClicked);

         //
         _gameUIView.MoveButtonsCanvasGroup.interactable = false;
         SetStatusText("");
         _gameUIView.AvatarUIViews[TBFConstants.PlayerIndexLocal].HealthBarView.Health = 100;
         _gameUIView.AvatarUIViews[TBFConstants.PlayerIndexRemote].HealthBarView.Health = 100;

         //
         SetupBeamable();
      }


      protected void Update()
      {
         _tbfMultiplayerSession?.Update();
      }

      //  Other Methods --------------------------------
      private async void SetupBeamable()
      {
         _gameState = GameState.Loading;
         SetStatusText(TBFConstants.StatusText_Beamable_Loading);
         

         await Beamable.API.Instance.Then(de =>
         {
            _gameState = GameState.Loaded;
            SetStatusText(TBFConstants.StatusText_Beamable_Loaded);

            try
            {
               _beamableAPI = de;

               //TODO: Fetch the found matchmaking info from the previous scene
               long localPlayerDbid = _beamableAPI.User.id;
               string roomId = TBFMatchmaking.RoomId;
               int targetPlayerCount = 1;

               _tbfMultiplayerSession = new TBFMultiplayerSession(localPlayerDbid,
                  targetPlayerCount, roomId);

               _gameState = GameState.Initializing;
               SetStatusText(TBFConstants.StatusText_Multiplayer_Initializing);

               _tbfMultiplayerSession.OnInit += MultiplayerSession_OnInit;
               _tbfMultiplayerSession.OnConnect += MultiplayerSession_OnConnect;
               _tbfMultiplayerSession.OnDisconnect += MultiplayerSession_OnDisconnect;
               _tbfMultiplayerSession.Initialize();

            }
            catch (Exception)
            {
               SetStatusText(TBFHelper.InternetOfflineInstructionsText);
            }
         });
      }



      private void RestartGame()
      {
         if (_runGameCoroutine != null)
         {
            StopCoroutine(_runGameCoroutine);
         }
         _runGameCoroutine = StartCoroutine(RunGame_Coroutine());
      }


      private IEnumerator RunGame_Coroutine()
      {
         //Initialize

         //Countdown Before Move
         float delayGameBeforeMove = _configuration.DelayGameBeforeMove;
         float elapsedGameBeforeMove = 0;
         while (elapsedGameBeforeMove <= delayGameBeforeMove)
         {
            elapsedGameBeforeMove += Time.deltaTime;
            float remainingGameBeforeMove = delayGameBeforeMove - elapsedGameBeforeMove;

            SetStatusText(string.Format(TBFConstants.StatusText_Before_Move, remainingGameBeforeMove));
            yield return new WaitForEndOfFrame();
         }

         //Countdown During Move
         float delayGameMaxDuringMove = _configuration.DelayGameMaxDuringMove;
         float elapsedGameDuringMove = 0;
         while (elapsedGameDuringMove <= delayGameMaxDuringMove)
         {
            elapsedGameDuringMove += Time.deltaTime;
            float remainingGameDuringMove = delayGameMaxDuringMove - elapsedGameDuringMove;

            //Show as "3.2 seconds"
            SetStatusText(string.Format(TBFConstants.StatusText_During_Move, remainingGameDuringMove));
            yield return new WaitForEndOfFrame();
         }
      }

      private void SetStatusText(string message)
      {
         _gameUIView.StatusText.text = message;
      }


      private void BindDbidToEvents(long playerDbid, bool isBinding)
      {
         if (isBinding)
         {
            string origin = playerDbid.ToString();
            _tbfMultiplayerSession.On<GameStartEvent>(origin, MultiplayerSession_OnGameStartEvent);
            _tbfMultiplayerSession.On<GameMoveEvent>(origin, MultiplayerSession_OnGameMoveEvent);
         }
         else
         {
            //_tbfMultiplayerSession.Remove(MultiplayerSession_OnGameStartEvent);
            //_tbfMultiplayerSession.Remove(MultiplayerSession_OnGameMoveEvent);
         }
      }


      //  Event Handlers -------------------------------

      private void BackButton_OnClicked()
      {
         //TEMP - restart game
         //RestartGame();
   
         //TODO: Disconnect the player?

         StartCoroutine(TBFHelper.LoadScene_Coroutine(_configuration.IntroSceneName,
            _configuration.DelayBeforeLoadScene));
      }

      private void MoveButton_01_OnClicked()
      {
         if (_gameState == GameState.Moving)
         {
            _tbfMultiplayerSession.SendEvent<GameMoveEvent>(
               new GameMoveEvent(GameMoveType.High));
         }
      }


      private void MoveButton_02_OnClicked()
      {
         if (_gameState == GameState.Moving)
         {
            _tbfMultiplayerSession.SendEvent<GameMoveEvent>(
               new GameMoveEvent(GameMoveType.Medium));
         }
      }

      private void MoveButton_03_OnClicked()
      {
         if (_gameState == GameState.Moving)
         {
            _tbfMultiplayerSession.SendEvent<GameMoveEvent>(
               new GameMoveEvent(GameMoveType.Low));
         }
      }

      private void MultiplayerSession_OnInit(long body)
      {
         _gameState = GameState.Initialized;
         SetStatusText(TBFConstants.StatusText_Multiplayer_Initialized);
      }


      private void MultiplayerSession_OnConnect(long playerDbid)
      {
         
         BindDbidToEvents(playerDbid, true);
 
         if (_tbfMultiplayerSession.PlayerDbids.Count < _tbfMultiplayerSession.TargetPlayerCount)
         {
            _gameState = GameState.Connecting;
            SetStatusText(string.Format(TBFConstants.StatusText_Multiplayer_OnConnect,
                  _tbfMultiplayerSession.PlayerDbids.Count.ToString(), 
                  _tbfMultiplayerSession.TargetPlayerCount));
         }
         else
         {
            _gameState = GameState.Connected;
            
            _tbfMultiplayerSession.SendEvent<GameStartEvent>(new GameStartEvent());
         }
      }


      private void MultiplayerSession_OnDisconnect(long playerDbid)
      {
         BindDbidToEvents(playerDbid, false);

         SetStatusText(string.Format(TBFConstants.StatusText_Multiplayer_OnDisconnect,
            _tbfMultiplayerSession.PlayerDbids.Count.ToString(),
            _tbfMultiplayerSession.TargetPlayerCount));
      }


      private void MultiplayerSession_OnGameStartEvent(GameStartEvent gameStartEvent)
      {
         SetStatusText(string.Format("gameStartEvent() {0}", gameStartEvent));

         if (_tbfMultiplayerSession.PlayerDbids.Count == _tbfMultiplayerSession.TargetPlayerCount)
         {
            //TODO: Move to a state pattern (version 1 where I just do a 
            //switch statement atop this class and...
            //1. toggle buttons off/on
            //2. show text to status
            _gameState = GameState.Started;
            _gameState = GameState.Moving;
         }
         else
         {
            _gameState = GameState.Starting;
         }
      }

      private void MultiplayerSession_OnGameMoveEvent(GameMoveEvent gameMoveEvent)
      {
         SetStatusText(string.Format("gameMoveEvent() {0}", gameMoveEvent));
         if (_tbfMultiplayerSession.PlayerDbids.Count == _tbfMultiplayerSession.TargetPlayerCount)
         {
            _gameUIView.MoveButtonsCanvasGroup.interactable = true;
         }
      }
   }
}