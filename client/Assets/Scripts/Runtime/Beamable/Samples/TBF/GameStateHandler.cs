using Beamable.Samples.Core;
using Beamable.Samples.TBF.Audio;
using Beamable.Samples.TBF.Data;
using Beamable.Samples.TBF.Exceptions;
using Beamable.Samples.TBF.Multiplayer;
using Beamable.Samples.TBF.Multiplayer.Events;
using Beamable.Samples.TBF.Views;
using System;
using System.Threading.Tasks;
using UnityAsync;
using UnityEngine;
using static Beamable.Samples.TBF.UI.TMP_BufferedText;

namespace Beamable.Samples.TBF
{
   /// <summary>
   /// List of all phases of the gameplay.
   /// There are arguably more states here than are needed, 
   /// however all are indeed used, in the order shown, for deliberate separation.
   /// </summary>
   public enum GameState
   {
      //Game loads within here
      Null,
      Loading,
      Loaded,
      Initializing,
      Initialized,
      Connecting,
      Connected,
      GameStarting,
      GameStarted,

      //Game repeats within here
      RoundStarting,
      RoundStarted,
      RoundPlayerMoving,
      RoundPlayerMoved,
      RoundEvaluating,
      RoundEvaluated,

      //Game ends here
      GameEnding,
   }

   /// <summary>
   /// Handles the <see cref="GameState"/> for the <see cref="GameSceneManager"/>.
   /// </summary>
   public class GameStateHandler
   {
      //  Properties -----------------------------------
      public GameState GameState { get { return _gameState; } }

      //  Fields ---------------------------------------
      private GameState _gameState = GameState.Null;
      private GameSceneManager _gameSceneManager;

      //  Other Methods  -----------------------------
      public GameStateHandler(GameSceneManager gameSceneManager)
      {
         _gameSceneManager = gameSceneManager;
      }


      /// <summary>
      /// Store and handle changes to the <see cref="GameState"/>.
      /// </summary>
      /// <param name="gameState"></param>
      /// <returns></returns>
      public async Task SetGameState(GameState gameState)
      {
         DebugLog($"SetGameState() from {_gameState} to {gameState}");

         //NOTE: Do not set "_gameState" directly anywhere, except here.
         _gameState = gameState;

         // SetGameState() is async...
         //    Pros: We can use operations like "Task.Delay" to slow down execution
         //    Cons: Error handling is tricky. 
         //    Workaround: AsyncUtility helps with its try/catch.
         await AsyncUtility.AsyncSafe(async () =>
         {
            switch (_gameState)
            {
               case GameState.Null:
                  break;

               case GameState.Loading:
                  // **************************************
                  // Render the scene before any latency 
                  // of multiplayer begins
                  // **************************************

                  _gameSceneManager.SetStatusText("", BufferedTextMode.Immediate);

                  _gameSceneManager.GameUIView.AvatarViews[TBFConstants.PlayerIndexLocal].PlayAnimationIdle();
                  _gameSceneManager.GameUIView.AvatarViews[TBFConstants.PlayerIndexRemote].PlayAnimationIdle();

                  _gameSceneManager.GameProgressData = new GameProgressData(_gameSceneManager.Configuration);
                  _gameSceneManager.GameUIView.MoveButtonsCanvasGroup.interactable = false;
                  _gameSceneManager.SetStatusText(TBFConstants.StatusText_GameState_Loading, BufferedTextMode.Queue);

                  break;

               case GameState.Loaded:
                  // **************************************
                  //  Update UI
                  //  
                  // **************************************

                  _gameSceneManager.SetStatusText(TBFConstants.StatusText_GameState_Loaded, BufferedTextMode.Queue);
                  break;

               case GameState.Initializing:
                  // **************************************
                  //  Update UI
                  //  
                  // **************************************

                  _gameSceneManager.SetStatusText(TBFConstants.StatusText_GameState_Initializing, BufferedTextMode.Queue);
                  break;

               case GameState.Initialized:
                  // **************************************
                  //  Update UI
                  //  
                  // **************************************

                  _gameSceneManager.SetStatusText(TBFConstants.StatusText_GameState_Initialized, BufferedTextMode.Queue);
                  break;

               case GameState.Connecting:
                  // **************************************
                  //  Update UI
                  //  
                  // **************************************

                  _gameSceneManager.SetStatusText(string.Format(TBFConstants.StatusText_GameState_Connecting,
                     _gameSceneManager.MultiplayerSession.PlayerDbidsCount.ToString(),
                     _gameSceneManager.MultiplayerSession.TargetPlayerCount), BufferedTextMode.Queue);
                  break;

               case GameState.Connected:
                  // **************************************
                  //  Advanced the state 
                  //  
                  // **************************************
                  await SetGameState(GameState.GameStarting);
                  break;

               case GameState.GameStarting:
                  // **************************************
                  //  Reset the game-specific data
                  //  
                  // **************************************
                  _gameSceneManager.GameProgressData.GameRoundCurrent = 0;
                  break;

               case GameState.GameStarted:
                  // **************************************
                  //  Now that all players have connected, setup AI
                  //  
                  // **************************************

                  // RemotePlayerAI is always created, but enabled only sometimes
                  bool isEnabledRemotePlayerAI = _gameSceneManager.MultiplayerSession.IsHumanVsBotMode;
                  System.Random random = _gameSceneManager.MultiplayerSession.Random;

                  if (TBFConstants.IsDebugLogging)
                  {
                     Debug.Log($"isEnabledRemotePlayerAI={isEnabledRemotePlayerAI}");
                  }

                  _gameSceneManager.RemotePlayerAI = new RemotePlayerAI(random, isEnabledRemotePlayerAI);

                  await SetGameState(GameState.RoundStarting);
                  break;

               case GameState.RoundStarting:
                  // **************************************
                  //  Reste the round-specific data.
                  //  Advance the state. 
                  //  This happens before EACH round during a game
                  // **************************************
                  _gameSceneManager.GameProgressData.GameRoundCurrent++;
                  _gameSceneManager.GameProgressData.GameMoveEventsThisRoundByPlayerDbid.Clear();
                  await SetGameState(GameState.RoundStarted);
                  break;

               case GameState.RoundStarted:
                  // **************************************
                  //  Advance the state
                  //  
                  // **************************************

                  while (_gameSceneManager.GameUIView.BufferedText.HasRemainingQueueText)
                  {
                     // Wait for old messages to pass before allowing button clicks
                     await Await.NextUpdate();
                  }
                  _gameSceneManager.GameUIView.MoveButtonsCanvasGroup.interactable = true;

                  await SetGameState(GameState.RoundPlayerMoving);
                  break;

               case GameState.RoundPlayerMoving:
                  // **************************************
                  //  Update UI
                  //  
                  // **************************************
                  _gameSceneManager.SetStatusText(string.Format(TBFConstants.StatusText_GameState_PlayerMoving,
                      _gameSceneManager.GameProgressData.GameRoundCurrent), BufferedTextMode.Queue);

                  break;

               case GameState.RoundPlayerMoved:
                  // **************************************
                  //  
                  //  
                  // **************************************
                  long localPlayerDbid = _gameSceneManager.MultiplayerSession.GetPlayerDbidForIndex(TBFConstants.PlayerIndexLocal);
                  GameMoveEvent localGameMoveEvent;
                  _gameSceneManager.GameProgressData.GameMoveEventsThisRoundByPlayerDbid.TryGetValue(localPlayerDbid, out localGameMoveEvent);

                  GameMoveType localGameMoveType = localGameMoveEvent.GameMoveType;
                  GameMoveType remoteGameMoveType = GameMoveType.Null;
                  if (_gameSceneManager.RemotePlayerAI.IsEnabled)
                  {
                     remoteGameMoveType = _gameSceneManager.RemotePlayerAI.GetNextGameMoveType();
                  }
                  else
                  {
                     long remotePlayerDbid = _gameSceneManager.MultiplayerSession.GetPlayerDbidForIndex(TBFConstants.PlayerIndexRemote);

                     GameMoveEvent remoteGameEvent;
                     _gameSceneManager.GameProgressData.GameMoveEventsThisRoundByPlayerDbid.TryGetValue(remotePlayerDbid, out remoteGameEvent);
                     remoteGameMoveType = remoteGameEvent.GameMoveType;
                  }

                  // LOCAL
                  await RenderPlayerMove(TBFConstants.PlayerIndexLocal, localGameMoveType);

                  // REMOTE
                  await RenderPlayerMove(TBFConstants.PlayerIndexRemote, remoteGameMoveType);

                  // All players have moved
                  _gameSceneManager.SetStatusText(string.Format(TBFConstants.StatusText_GameState_PlayersAllMoved,
                     _gameSceneManager.GameProgressData.GameRoundCurrent), BufferedTextMode.Queue);

                  if (_gameSceneManager.GameProgressData.GameMoveEventsThisRoundByPlayerDbid.Count ==
                     _gameSceneManager.MultiplayerSession.TargetPlayerCount)
                  {
                     // ALL players moved? See who won the round
                     await SetGameState(GameState.RoundEvaluating);
                  }
                  else
                  {
                     // Only SOME players moved? Wait for others...
                     await SetGameState(GameState.RoundPlayerMoving);
                  }

                  break;

               case GameState.RoundEvaluating:
                  // **************************************
                  //  
                  //  
                  // **************************************
                  _gameSceneManager.GameProgressData.EvaluateGameMoveEventsThisRound();
                  await SetGameState(GameState.RoundEvaluated);

                  break;

               case GameState.RoundEvaluated:
                  // **************************************
                  //  
                  //  
                  // **************************************

                  long roundWinnerDbid = _gameSceneManager.GameProgressData.GetRoundWinnerPlayerDbid();
                  string roundWinnerName;

                  if (_gameSceneManager.MultiplayerSession.IsLocalPlayerDbid(roundWinnerDbid))
                  {
                     roundWinnerName = GetPlayerName(TBFConstants.PlayerIndexLocal);
                  }
                  else
                  {
                     roundWinnerName = GetPlayerName(TBFConstants.PlayerIndexRemote);
                  }

                  _gameSceneManager.SetStatusText(string.Format(TBFConstants.StatusText_GameState_Evaluated,
                     _gameSceneManager.GameProgressData.GameRoundCurrent, roundWinnerName), BufferedTextMode.Queue);

                  while (_gameSceneManager.GameUIView.BufferedText.HasRemainingQueueText)
                  {
                     // Wait for old messages to pass before allowing button clicks
                     await Await.NextUpdate();
                  }

                  SoundManager.Instance.PlayAudioClip(SoundConstants.HealthBarDecrement);

                  if (_gameSceneManager.MultiplayerSession.IsLocalPlayerDbid(roundWinnerDbid))
                  {
                     _gameSceneManager.GameUIView.AvatarUIViews[TBFConstants.PlayerIndexRemote].HealthBarView.Health -= 34;
                  }
                  else
                  {
                     _gameSceneManager.GameUIView.AvatarUIViews[TBFConstants.PlayerIndexLocal].HealthBarView.Health -= 34;
                  }

                  //Wait for animations to finish
                  await AsyncUtility.TaskDelaySeconds(_gameSceneManager.Configuration.DelayGameBeforeGameOver);

                  if (_gameSceneManager.GameProgressData.GameHasWinnerPlayerDbid())
                  {
                     await SetGameState(GameState.GameEnding);
                  }
                  else
                  {

                     await SetGameState(GameState.RoundStarting);
                  }

                  break;

               case GameState.GameEnding:
                  // **************************************
                  //  
                  //  
                  // **************************************

                  long gameWinnerDbid = _gameSceneManager.GameProgressData.GetGameWinnerPlayerDbid();
                  string gameWinnerName;


                  if (_gameSceneManager.MultiplayerSession.IsLocalPlayerDbid(gameWinnerDbid))
                  {
                     gameWinnerName = GetPlayerName(TBFConstants.PlayerIndexLocal);

                     //Local winner
                     SoundManager.Instance.PlayAudioClip(SoundConstants.GameOverWin);
                     _gameSceneManager.GameUIView.AvatarViews[TBFConstants.PlayerIndexLocal].PlayAnimationWin();
                     _gameSceneManager.GameUIView.AvatarViews[TBFConstants.PlayerIndexRemote].PlayAnimationLoss();
                  }
                  else
                  {
                     gameWinnerName = GetPlayerName(TBFConstants.PlayerIndexRemote);   

                     //Remote winner
                     SoundManager.Instance.PlayAudioClip(SoundConstants.GameOverLoss);
                     _gameSceneManager.GameUIView.AvatarViews[TBFConstants.PlayerIndexLocal].PlayAnimationLoss();
                     _gameSceneManager.GameUIView.AvatarViews[TBFConstants.PlayerIndexRemote].PlayAnimationWin();
                  }

                  _gameSceneManager.SetStatusText(string.Format(TBFConstants.StatusText_GameState_Ending,
                    _gameSceneManager.GameProgressData.GameRoundCurrent, gameWinnerName), BufferedTextMode.Queue);

                  break;

               default:
                  SwitchDefaultException.Throw(_gameState);
                  break;
            }
         }, new System.Diagnostics.StackTrace(true));
      }

      private async Task RenderPlayerMove(int playerIndex, GameMoveType gameMoveType)
      {
         string playerName = GetPlayerName(playerIndex);
         _gameSceneManager.SetStatusText(string.Format(TBFConstants.StatusText_GameState_PlayerMoved,
         _gameSceneManager.GameProgressData.GameRoundCurrent, playerName, gameMoveType), BufferedTextMode.Queue);

         AvatarView avatarView = _gameSceneManager.GameUIView.AvatarViews[playerIndex];
         avatarView.PlayAnimationByGameMoveType(gameMoveType);

         // 1 Unity needs time to START non-IDLE animation ...
         await Await.While(() =>
         {
            return avatarView.IsIdleAnimation;
         });

         // 2 Unity needs time to RETURN to the IDLE animation ...
         await Await.While(() =>
         {
            return !avatarView.IsIdleAnimation;
         });
      }

      private string GetPlayerName(int playerIndex)
      {
         return _gameSceneManager.Configuration.AvatarDatas[playerIndex].Location;
      }

      private void DebugLog(string message)
      {
         if (TBFConstants.IsDebugLogging)
         {
            Debug.Log(message);
         }
      }
   }
}
