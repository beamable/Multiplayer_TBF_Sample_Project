﻿using Beamable.Content;
using Beamable.Samples.TBF.Data;
using Beamable.Samples.TBF.Exceptions;
using Beamable.Samples.TBF.Multiplayer;
using Beamable.Samples.TBF.Multiplayer.Events;
using Beamable.Samples.TBF.Views;
using System;
using System.Collections;
using UnityEngine;
using static Beamable.Samples.TBF.Views.GameUIView;

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
      Moved,
      Evaluating,
      Evaluated,
      Ending,
      
   }
   /// <summary>
   /// Handles the main scene logic: Game
   /// </summary>
   public class GameSceneManager : MonoBehaviour
   {
      //  Properties -----------------------------------

      /// <summary>
      /// For both organization and understandability, here a 
      /// very LIGHT State Pattern is used.
      /// </summary>
      private GameState GameState
      {
         get
         { 
            return _gameState; 
         }
         set
         {

            DebugLog($"GameState() from {_gameState} to {value}");
            _gameState = value;
            
            switch (GameState)
            {
               case GameState.Null:
                  break;
               case GameState.Loading:
                  SetStatusText("", StatusTextMode.Immediate);
                  _gameProgressData = new GameProgressData(_configuration);
                  SetStatusText(TBFConstants.StatusText_GameState_Loading, StatusTextMode.Queue);
                  break;
               case GameState.Loaded:
                  SetStatusText(TBFConstants.StatusText_GameState_Loaded, StatusTextMode.Queue);
                  break;
               case GameState.Initializing:
                  SetStatusText(TBFConstants.StatusText_GameState_Initializing, StatusTextMode.Queue);
                  break;
               case GameState.Initialized:
                  SetStatusText(TBFConstants.StatusText_GameState_Initialized, StatusTextMode.Queue);
                  break;
               case GameState.Connecting:
                  SetStatusText(string.Format(TBFConstants.StatusText_GameState_Connecting,
                     _tbfMultiplayerSession.PlayerDbids.Count.ToString(),
                     _tbfMultiplayerSession.TargetPlayerCount), StatusTextMode.Queue);
                  break;
               case GameState.Connected:
                  break;
               case GameState.Starting:
                  _gameProgressData.GameRoundCurrent = 0;
                  break;
               case GameState.Started:
                  break;
               case GameState.Moving:
                  SetStatusText(string.Format(TBFConstants.StatusText_GameState_Moving,
                      _gameProgressData.GameRoundCurrent), StatusTextMode.Queue);
                  _gameProgressData.GameMoveEventsThisRoundByPlayerDbid.Clear();
                  _gameUIView.MoveButtonsCanvasGroup.interactable = true;
                  break;
               case GameState.Moved:
                  SetStatusText(string.Format(TBFConstants.StatusText_GameState_Moved,
                     _gameProgressData.GameRoundCurrent), StatusTextMode.Queue);
                  _gameUIView.MoveButtonsCanvasGroup.interactable = false;
                  break;
               case GameState.Evaluating:
                  _gameProgressData.EvaluateGameMoveEventsThisRound();
                  GameState = GameState.Evaluated;
                  break;
               case GameState.Evaluated:
                  SetStatusText(string.Format(TBFConstants.StatusText_GameState_Evaluated,
                     _gameProgressData.GameRoundCurrent,
                     _gameProgressData.GetRoundWinnerPlayerDbid()), StatusTextMode.Queue);

                  if (_gameProgressData.GameHasWinner())
                  {
                     GameState = GameState.Ending;
                  }
                  else
                  {
                     _gameProgressData.GameRoundCurrent++;
                     GameState = GameState.Moving;
                  }
                  break;
               case GameState.Ending:
                  SetStatusText(string.Format(TBFConstants.StatusText_GameState_Ending,
                     _gameProgressData.GameRoundCurrent,
                     _gameProgressData.GetGameWinnerPlayerDbid()), StatusTextMode.Queue);
                  break;
               default:
                  SwitchDefaultException.Throw(GameState);
                  break;
            }
         }
      }



      //  Fields ---------------------------------------

      /// <summary>
      /// Determines if using Unity debug log statements.
      /// </summary>
      private static bool IsDebugLogging = true;

      [SerializeField]
      private Configuration _configuration = null;

      [SerializeField]
      private GameUIView _gameUIView = null;

      private Coroutine _runGameCoroutine;
      private LeaderboardContent _leaderboardContent;
      private IBeamableAPI _beamableAPI = null;
      private TBFMultiplayerSession _tbfMultiplayerSession;
      private GameState _gameState = GameState.Null;
      private GameProgressData _gameProgressData;


      //  Unity Methods   ------------------------------
      protected void Start ()
      {
         _gameUIView.BackButton.onClick.AddListener(BackButton_OnClicked);
         _gameUIView.MoveButton_01.onClick.AddListener(MoveButton_01_OnClicked);
         _gameUIView.MoveButton_02.onClick.AddListener(MoveButton_02_OnClicked);
         _gameUIView.MoveButton_03.onClick.AddListener(MoveButton_03_OnClicked);
         //
         _gameUIView.MoveButtonsCanvasGroup.interactable = false;
         
         _gameUIView.AvatarUIViews[TBFConstants.PlayerIndexLocal].HealthBarView.Health = 100;
         _gameUIView.AvatarUIViews[TBFConstants.PlayerIndexRemote].HealthBarView.Health = 100;

         //
         SetupBeamable();
      }


      protected void Update()
      {
         _tbfMultiplayerSession?.Update();
      }

      //  Other Methods  -----------------------------
      private void DebugLog(string message)
      {
         if (IsDebugLogging)
         {
            Debug.Log(message);
         }
      }

      private async void SetupBeamable()
      {
         GameState = GameState.Loading;

         await Beamable.API.Instance.Then(de =>
         {
            GameState = GameState.Loaded;

            try
            {
               _beamableAPI = de;

               //TODO: Fetch the found matchmaking info from the previous scene
               long localPlayerDbid = _beamableAPI.User.id;
               string roomId = TBFMatchmaking.GetRandomRoomId();
               int targetPlayerCount = 1;

               _tbfMultiplayerSession = new TBFMultiplayerSession(localPlayerDbid,
                  targetPlayerCount, roomId);

               GameState = GameState.Initializing;
               

               _tbfMultiplayerSession.OnInit += MultiplayerSession_OnInit;
               _tbfMultiplayerSession.OnConnect += MultiplayerSession_OnConnect;
               _tbfMultiplayerSession.OnDisconnect += MultiplayerSession_OnDisconnect;
               _tbfMultiplayerSession.Initialize();

            }
            catch (Exception)
            {
               SetStatusText(TBFHelper.InternetOfflineInstructionsText, StatusTextMode.Immediate);
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

            SetStatusText(string.Format(TBFConstants.StatusText_Before_Move, remainingGameBeforeMove), StatusTextMode.Immediate);
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
            SetStatusText(string.Format(TBFConstants.StatusText_During_Move, remainingGameDuringMove), StatusTextMode.Immediate);
            yield return new WaitForEndOfFrame();
         }
      }

      private void SetStatusText(string message, GameUIView.StatusTextMode statusTextMode)
      {
         _gameUIView.SetStatusText (message, statusTextMode);
      }


      private void BindPlayerDbidToEvents(long playerDbid, bool isBinding)
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
         if (GameState == GameState.Moving)
         {
            _tbfMultiplayerSession.SendEvent<GameMoveEvent>(
               new GameMoveEvent(GameMoveType.High));
         }
      }


      private void MoveButton_02_OnClicked()
      {
         if (GameState == GameState.Moving)
         {
            _tbfMultiplayerSession.SendEvent<GameMoveEvent>(
               new GameMoveEvent(GameMoveType.Medium));
         }
      }

      private void MoveButton_03_OnClicked()
      {
         if (GameState == GameState.Moving)
         {
            _tbfMultiplayerSession.SendEvent<GameMoveEvent>(
               new GameMoveEvent(GameMoveType.Low));
         }
      }

      private void MultiplayerSession_OnInit(long body)
      {
         GameState = GameState.Initialized;
         
      }


      private void MultiplayerSession_OnConnect(long playerDbid)
      {
         
         BindPlayerDbidToEvents(playerDbid, true);
 
         if (_tbfMultiplayerSession.PlayerDbids.Count < _tbfMultiplayerSession.TargetPlayerCount)
         {
            GameState = GameState.Connecting;

         }
         else
         {
            GameState = GameState.Connected;
            
            _tbfMultiplayerSession.SendEvent<GameStartEvent>(new GameStartEvent());
         }
      }


      private void MultiplayerSession_OnDisconnect(long playerDbid)
      {
         BindPlayerDbidToEvents(playerDbid, false);

         SetStatusText(string.Format(TBFConstants.StatusText_Multiplayer_OnDisconnect,
            _tbfMultiplayerSession.PlayerDbids.Count.ToString(),
            _tbfMultiplayerSession.TargetPlayerCount), StatusTextMode.Immediate);
      }


      private void MultiplayerSession_OnGameStartEvent(GameStartEvent gameStartEvent)
      {
         if (_tbfMultiplayerSession.PlayerDbids.Count == _tbfMultiplayerSession.TargetPlayerCount)
         {
            //TODO: Move to a state pattern (version 1 where I just do a 
            //switch statement atop this class and...
            //1. toggle buttons off/on
            //2. show text to status
            GameState = GameState.Started;
            GameState = GameState.Moving;
         }
         else
         {
            GameState = GameState.Starting;
         }
      }

      private void MultiplayerSession_OnGameMoveEvent(GameMoveEvent gameMoveEvent)
      {
         //Add each player event to a list
         _gameProgressData.GameMoveEventsThisRoundByPlayerDbid[gameMoveEvent.PlayerDbid] = gameMoveEvent;

         //Enough players did a move? Then advance game state
         if (_gameProgressData.GameMoveEventsThisRoundByPlayerDbid.Count == _tbfMultiplayerSession.TargetPlayerCount)
         {
            GameState = GameState.Moved;
            GameState = GameState.Evaluating;
         }
      }
   }
}