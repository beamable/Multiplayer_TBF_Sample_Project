using Beamable.Samples.TBF.Audio;
using Beamable.Samples.TBF.Data;
using Beamable.Samples.TBF.Exceptions;
using Beamable.Samples.TBF.Multiplayer.Events;
using System;
using System.Threading.Tasks;
using UnityEngine;
using static Beamable.Samples.TBF.UI.TMP_BufferedText;

// Disable: "Because this call is not awaited, execution of the current method continues before the call is completed"
#pragma warning disable CS4014 

namespace Beamable.Samples.TBF
{

   /// <summary>
   /// List of all phases of the gameplay.
   /// There are arguably more states here than are needed, 
   /// however all are used for deliberate separation.
   /// </summary>
   public enum GameState
   {
      Null,
      Loading,
      Loaded,
      Initializing,
      Initialized,
      Connecting,
      Connected,
      GameStarting,
      GameStarted,
      RoundStarting,
      RoundStarted,
      PlayerMoving,
      PlayerMoved,
      Evaluating,
      Evaluated,
      Ending,
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
      public GameStateHandler (GameSceneManager gameSceneManager)
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
         //Do not set "_gameState" directly anywhere, except here.
         _gameState = gameState;

         // SetGameState() is async...
         //    Pros: We can use operations like "Task.Delay" to slow down execution
         //    Cons: Error handling is tricky. So the try/catch is used.
         try
         {
            switch (_gameState)
            {
               case GameState.Null:
                  break;
               case GameState.Loading:

                  _gameSceneManager.GameUIView.AvatarViews[TBFConstants.PlayerIndexLocal].PlayAnimationIdle();
                  _gameSceneManager.GameUIView.AvatarViews[TBFConstants.PlayerIndexRemote].PlayAnimationIdle();

                  _gameSceneManager.GameProgressData = new GameProgressData(_gameSceneManager.Configuration);
                  _gameSceneManager.SetStatusText("", BufferedTextMode.Immediate);
                  _gameSceneManager.GameUIView.MoveButtonsCanvasGroup.interactable = false;
                  _gameSceneManager.SetStatusText(TBFConstants.StatusText_GameState_Loading, BufferedTextMode.Queue);

                  break;
               case GameState.Loaded:
                  _gameSceneManager.SetStatusText(TBFConstants.StatusText_GameState_Loaded, BufferedTextMode.Queue);
                  break;
               case GameState.Initializing:
                  _gameSceneManager.SetStatusText(TBFConstants.StatusText_GameState_Initializing, BufferedTextMode.Queue);
                  break;
               case GameState.Initialized:
                  _gameSceneManager.SetStatusText(TBFConstants.StatusText_GameState_Initialized, BufferedTextMode.Queue);
                  break;
               case GameState.Connecting:
                  _gameSceneManager.SetStatusText(string.Format(TBFConstants.StatusText_GameState_Connecting,
                     _gameSceneManager.MultiplayerSession.PlayerDbidsCount.ToString(),
                     _gameSceneManager.MultiplayerSession.TargetPlayerCount), BufferedTextMode.Queue);
                  break;
               case GameState.Connected:
                  //Waits for GameStartEvent...
                  break;
               case GameState.GameStarting:
                  _gameSceneManager.GameProgressData.GameRoundCurrent = 0;
                  break;
               case GameState.GameStarted:
                  SetGameState(GameState.RoundStarting);
                  break;
               case GameState.RoundStarting:
                  _gameSceneManager.GameProgressData.GameRoundCurrent++;
                  SetGameState(GameState.RoundStarted);
                  break;
               case GameState.RoundStarted:
                  SetGameState(GameState.PlayerMoving);
                  break;
               case GameState.PlayerMoving:

                  _gameSceneManager.SetStatusText(string.Format(TBFConstants.StatusText_GameState_Moving,
                      _gameSceneManager.GameProgressData.GameRoundCurrent), BufferedTextMode.Queue);

                  _gameSceneManager.GameProgressData.GameMoveEventsThisRoundByPlayerDbid.Clear();

                  //Wait for old messages to pass before allowing button clicks
                  while (_gameSceneManager.GameUIView.BufferedText.HasRemainingQueueText)
                  {
                     await Task.Delay(TBFConstants.TaskDelayMin);
                  }
                  Debug.Log($"1:");

                  _gameSceneManager.GameUIView.MoveButtonsCanvasGroup.interactable = true;

                  break;
               case GameState.PlayerMoved:

                  Debug.Log($"PlayerMoved, {_gameSceneManager.GameProgressData.GameMoveEventsThisRoundByPlayerDbid.Count}" +
                              $" ==? {_gameSceneManager.MultiplayerSession.TargetPlayerCount}");

                  long localPlayerDbid = _gameSceneManager.MultiplayerSession.GetPlayerDbidForIndex(TBFConstants.PlayerIndexLocal);

                  GameMoveEvent localGameMoveEvent = new GameMoveEvent(GameMoveType.Null);
                  Debug.Log($"3b1 CONTAINS {localPlayerDbid} : " +
                     _gameSceneManager.GameProgressData.GameMoveEventsThisRoundByPlayerDbid.ContainsKey(localPlayerDbid));

                  Debug.Log($"3b1 y3 : " + localGameMoveEvent);

                  _gameSceneManager.GameProgressData.GameMoveEventsThisRoundByPlayerDbid.TryGetValue(localPlayerDbid, out localGameMoveEvent);
                  Debug.Log($"3b1 z: for " + localGameMoveEvent.GameMoveType);
                  GameMoveType localGameMoveType = localGameMoveEvent.GameMoveType;

                  GameMoveType remoteGameMoveType = GameMoveType.Null;
                  if (_gameSceneManager.MultiplayerSession.IsHumanVsBotMode)
                  {
                     remoteGameMoveType = TBFHelper.GetRandomGameMoveType();
                  }
                  else
                  {
                     long remotePlayerDbid = _gameSceneManager.MultiplayerSession.GetPlayerDbidForIndex(TBFConstants.PlayerIndexRemote);

                     GameMoveEvent remoteGameEvent;
                     _gameSceneManager.GameProgressData.GameMoveEventsThisRoundByPlayerDbid.TryGetValue(remotePlayerDbid, out remoteGameEvent);
                     remoteGameMoveType = remoteGameEvent.GameMoveType;
                  }

                  _gameSceneManager.GameUIView.AvatarViews[TBFConstants.PlayerIndexLocal].PlayAnimationByGameMoveType(localGameMoveType);
                  _gameSceneManager.GameUIView.AvatarViews[TBFConstants.PlayerIndexRemote].PlayAnimationByGameMoveType(remoteGameMoveType);

                  _gameSceneManager.SetStatusText(string.Format(TBFConstants.StatusText_GameState_Moved,
                     _gameSceneManager.GameProgressData.GameRoundCurrent), BufferedTextMode.Queue);

                  //Enough players did a move? Then advance game state
                  if (_gameSceneManager.GameProgressData.GameMoveEventsThisRoundByPlayerDbid.Count ==
                     _gameSceneManager.MultiplayerSession.TargetPlayerCount)
                  {
                     SetGameState(GameState.Evaluating);
                  }

                  break;
               case GameState.Evaluating:

                  _gameSceneManager.GameProgressData.EvaluateGameMoveEventsThisRound();
                  await SetGameState(GameState.Evaluated);

                  break;
               case GameState.Evaluated:

                  _gameSceneManager.SetStatusText(string.Format(TBFConstants.StatusText_GameState_Evaluated,
                     _gameSceneManager.GameProgressData.GameRoundCurrent,
                     _gameSceneManager.GameProgressData.GetRoundWinnerPlayerDbid()), BufferedTextMode.Queue);

                  //Wait for animations to finish
                  await Task.Delay((int)_gameSceneManager.Configuration.DelayGameBeforeGameOver *
                     TBFConstants.MillisecondMultiplier);

                  _gameSceneManager.GameUIView.AvatarUIViews[TBFConstants.PlayerIndexLocal].HealthBarView.Health = 50;
                  _gameSceneManager.GameUIView.AvatarUIViews[TBFConstants.PlayerIndexRemote].HealthBarView.Health = 50;

                  if (_gameSceneManager.GameProgressData.GameHasWinner())
                  {
                     await SetGameState(GameState.Ending);
                  }
                  else
                  {

                     await SetGameState(GameState.RoundStarting);
                  }

                  break;
               case GameState.Ending:

                  _gameSceneManager.SetStatusText(string.Format(TBFConstants.StatusText_GameState_Ending,
                     _gameSceneManager.GameProgressData.GameRoundCurrent,
                     _gameSceneManager.GameProgressData.GetGameWinnerPlayerDbid()), BufferedTextMode.Queue);

                  if (_gameSceneManager.MultiplayerSession.IsLocalPlayerDbid(_gameSceneManager.GameProgressData.GetGameWinnerPlayerDbid()))
                  {
                     //Local winner
                     SoundManager.Instance.PlayAudioClip(SoundConstants.GameOverWin);
                     _gameSceneManager.GameUIView.AvatarViews[TBFConstants.PlayerIndexLocal].PlayAnimationWin();
                     _gameSceneManager.GameUIView.AvatarViews[TBFConstants.PlayerIndexRemote].PlayAnimationIdle();
                  }
                  else
                  {
                     //Remote winner
                     SoundManager.Instance.PlayAudioClip(SoundConstants.GameOverLoss);
                     _gameSceneManager.GameUIView.AvatarViews[TBFConstants.PlayerIndexLocal].PlayAnimationIdle();
                     _gameSceneManager.GameUIView.AvatarViews[TBFConstants.PlayerIndexRemote].PlayAnimationWin();
                  }

                  break;
               default:
                  SwitchDefaultException.Throw(_gameState);
                  break;
            }

         }
         // Handle any exceptions that occur in the async operations above
         catch (Exception e)
         {
            Debug.LogError(e.Message);
         }

      }
   }
}
