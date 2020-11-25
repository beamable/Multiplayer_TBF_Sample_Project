using Beamable.Samples.TBF.Data;
using Beamable.Samples.TBF.Multiplayer;
using Beamable.Samples.TBF.Views;
using DisruptorBeam;
using DisruptorBeam.Content;
using System;
using System.Collections;
using UnityEngine;

namespace Beamable.Samples.TBF
{
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
      private IDisruptorEngine _disruptorEngine = null;
      private TBFMultiplayerSession _multiplayerSession;


      //  Unity Methods   ------------------------------
      protected void Start ()
      {
         _gameUIView.BackButton.onClick.AddListener(BackButton_OnClicked);

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
         _multiplayerSession?.Tick();
      }

      //  Other Methods --------------------------------
      private async void SetupBeamable()
      {
         SetStatusText(TBFConstants.StatusText_Beamable_Loading);
         await DisruptorEngine.Instance.Then(de =>
         {
            SetStatusText(TBFConstants.StatusText_Beamable_Loaded);
            try
            {
               _disruptorEngine = de;

               //TODO: Fetch the found matchmaking info from the previous scene
               long localPlayerDbid = _disruptorEngine.User.id;
               string roomId = TBFMatchmaking.RoomId;
               int targetPlayerCount = 1;

               _multiplayerSession = new TBFMultiplayerSession(localPlayerDbid,
                  targetPlayerCount, roomId);

               SetStatusText(TBFConstants.StatusText_Multiplayer_Initializing);
               _multiplayerSession.OnInit += MultiplayerSession_OnInit;
               _multiplayerSession.OnConnect += MultiplayerSession_OnConnect;
               _multiplayerSession.OnDisconnect += MultiplayerSession_OnDisconnect;
               _multiplayerSession.Initialize();
               Debug.Log("no Error");

            }
            catch (Exception)
            {
               Debug.Log("Error");
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


      //  Event Handlers -------------------------------

      private void BackButton_OnClicked()
      {
         //TEMP - restart game
         //RestartGame();
   
         //TODO: Disconnect the player?

         StartCoroutine(TBFHelper.LoadScene_Coroutine(_configuration.IntroSceneName,
            _configuration.DelayBeforeLoadScene));
      }

      private void MultiplayerSession_OnInit(long body)
      {
         SetStatusText(TBFConstants.StatusText_Multiplayer_Initialized);
      }

      private void MultiplayerSession_OnConnect(long playerDbid)
      {
         if (_multiplayerSession.PlayerDbids.Count == _multiplayerSession.TargetPlayerCount)
         {
            SetStatusText(string.Format(TBFConstants.StatusText_Multiplayer_OnConnect,
                  _multiplayerSession.PlayerDbids.Count.ToString(), 
                  _multiplayerSession.TargetPlayerCount));
         }
         else
         {
            RestartGame();
         }
            
      }

      private void MultiplayerSession_OnDisconnect(long playerDbid)
      {
         throw new NotImplementedException();
      }


   }
}