using Beamable.Samples.TBF.Data;
using Beamable.Samples.TBF.Multiplayer;
using Beamable.Samples.TBF.Views;
using DisruptorBeam;
using DisruptorBeam.Content;
using System;
using System.Collections;
using System.Collections.Generic;
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
         _gameUIView.MoveButtonsCanvasGroup.interactable = false;

         SetupBeamable();
      }

      protected void Update()
      {
         _multiplayerSession?.Tick();
      }

      //  Other Methods --------------------------------
      private async void SetupBeamable()
      {
         await DisruptorEngine.Instance.Then(de =>
         {
            try
            {
               _disruptorEngine = de;

               //TODO: Fetch the found matchmaking info from the previous scene
               long localPlayerDbid = _disruptorEngine.User.id;
               string roomId = TBFMatchmaking.RoomId;
               int targetPlayerCount = 1;

               _multiplayerSession = new TBFMultiplayerSession(localPlayerDbid,
                  targetPlayerCount, roomId);

               _multiplayerSession.OnConnect += MultiplayerSession_OnConnect;
               _multiplayerSession.OnDisconnect += MultiplayerSession_OnDisconnect;
               _multiplayerSession.Initialize();


            }
            catch (Exception)
            {
               _gameUIView.StatusText.text = TBFHelper.InternetOfflineInstructionsText;
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
            float pregameRemaining = delayGameBeforeMove - elapsedGameBeforeMove;

            //Show as "3.2 seconds"
            _gameUIView.StatusText.text = $"Prepare to click!\nStarting in {TBFHelper.GetRoundedTime(pregameRemaining)}...";
            yield return new WaitForEndOfFrame();
         }

         //Countdown During Move
         float delayGameMaxDuringMove = _configuration.DelayGameMaxDuringMove;
         float elapsedGameDuringMove = 0;
         while (elapsedGameDuringMove <= delayGameMaxDuringMove)
         {
            elapsedGameDuringMove += Time.deltaTime;
            float pregameRemaining = delayGameMaxDuringMove - elapsedGameDuringMove;

            //Show as "3.2 seconds"
            _gameUIView.StatusText.text = $"Prepare to click!\nStarting in {TBFHelper.GetRoundedTime(pregameRemaining)}...";
            yield return new WaitForEndOfFrame();
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

      private void MultiplayerSession_OnConnect(long playerDbid)
      {
         if (_multiplayerSession.PlayerDbids.Count == _multiplayerSession.TargetPlayerCount)
         {
            Debug.Log("waiting");
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