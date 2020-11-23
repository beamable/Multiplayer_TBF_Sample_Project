using Beamable.Samples.TBF;
using Beamable.Samples.TBF.Audio;
using Beamable.Samples.TBF.Data;
using Beamable.Samples.TBF.Views;
using DisruptorBeam;
using DisruptorBeam.Content;
using DisruptorBeam.Stats;
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

      [SerializeField]
      private LeaderboardRef _leaderboardRef = null;

      [SerializeField]
      private StatBehaviour _currentScoreStatBehaviour = null;

      [SerializeField]
      private StatBehaviour _highScoreStatBehaviour = null;

      private bool _hasFalseStart = false;
      private Coroutine _runGameCoroutine;
      private float _gameTimeRemaining = 0;
      private LeaderboardContent _leaderboardContent;
      private IDisruptorEngine _disruptorEngine = null;

      /// <summary>
      /// Calculated each time the main menu opens.
      /// </summary>
      private int _lastGlobalHighScore = -1;

      //  Unity Methods   ------------------------------
      protected void Start ()
      {
         _gameUIView.BackButton.onClick.AddListener(BackButton_OnClicked);

         //Setup Stat - High Score set to default, unless higher backend score exists
         //This is used for the cosmetics of the tree & audio
         _highScoreStatBehaviour.OnStatReceived.AddListener(HighScoreStatBehaviour_OnStatReceived);

         //Setup Stat - Score
         _currentScoreStatBehaviour.OnStatReceived.AddListener(CurrentScoreStatBehaviour_OnStatReceived);

         SetupBeamable();
      }


      //  Other Methods --------------------------------
      private async void SetupBeamable()
      {
         _leaderboardContent = await _leaderboardRef.Resolve();

         await DisruptorEngine.Instance.Then(de =>
         {
            try
            {
               _disruptorEngine = de;
               RestartGame();

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
         _hasFalseStart = false;
         _currentScoreStatBehaviour.SetCurrentValue("0");

         //Countdown Pregame
         float pregameDuration = 11;
         float pregameElapsed = 0;
         while (pregameElapsed <= pregameDuration && !_hasFalseStart)
         {
            pregameElapsed += Time.deltaTime;
            float pregameRemaining = pregameDuration - pregameElapsed;

            //Show as "3.2 seconds"
            _gameUIView.StatusText.text = $"Prepare to click!\nStarting in {TBFHelper.GetRoundedTime(pregameRemaining)}...";
            yield return new WaitForEndOfFrame();
         }

         float gameDuration = 11;
         if (!_hasFalseStart)
         {
            //Motivation
            SoundManager.Instance.PlayAudioClip(SoundConstants.Coin01);

            //Countdown Game
            float gameElapsed = 0;
            while (gameElapsed <= gameDuration && !_hasFalseStart)
            {
               gameElapsed += Time.deltaTime;
               _gameTimeRemaining = gameDuration - gameElapsed;

               //Show as "3.2 seconds"
               yield return new WaitForEndOfFrame();
            }
         }

         //Gameover state
         if (_hasFalseStart)
         {
            SoundManager.Instance.PlayAudioClip(SoundConstants.GameOverLoss);
            _gameUIView.StatusText.text = $"<color=#91291B>False start!</color>\nWait before clicking.\nTry again.";
         }
         else
         {
            SoundManager.Instance.PlayAudioClip(SoundConstants.GameOverWin);
            _gameUIView.StatusText.text = $"{_currentScoreStatBehaviour.Value} clicks" +
               $" in {gameDuration} seconds!\nCheck the Leaderboard.";

            double finalScore = TBFHelper.GetRoundedScore(_currentScoreStatBehaviour.Value);
            _disruptorEngine.LeaderboardService.SetScore(_leaderboardContent.Id, finalScore);
         }
      }


      //  Event Handlers -------------------------------
      private void ClickMeButton_OnClicked()
      {
         switch ("blah")
         {
            default:
               throw new Exception("Not possible");
         }
      }


      private void BackButton_OnClicked()
      {
         //TEMP - restart game
         //RestartGame();

         StartCoroutine(TBFHelper.LoadScene_Coroutine(_configuration.IntroSceneName,
            _configuration.DelayBeforeLoadScene));
      }


      private void CurrentScoreStatBehaviour_OnStatReceived(string value)
      {
         if (_lastGlobalHighScore == TBFConstants.UnsetValue)
         {
            return;
         }

         //Debug.Log("_treeView.GrowthPercentage() : " + _treeView.GrowthPercentage);
      }


      private void HighScoreStatBehaviour_OnStatReceived(string value)
      {
         Debug.Log("HighScoreStatBehaviour_OnStatReceived(): " + _lastGlobalHighScore);
      }
   }
}