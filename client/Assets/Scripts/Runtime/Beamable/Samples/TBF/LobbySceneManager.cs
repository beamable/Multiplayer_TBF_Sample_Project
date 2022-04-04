using Beamable.Samples.TBF.Data;
using Beamable.Samples.TBF.Multiplayer;
using Beamable.Samples.TBF.Views;
using System;
using System.Collections;
using Beamable.Common.Content;
using Beamable.Samples.Core.Multiplayer;
using Beamable.Samples.Core.UI;
using UnityEngine;
using SimGameTypeRef = Beamable.Common.Content.SimGameTypeRef;

namespace Beamable.Samples.TBF
{
   /// <summary>
   /// Handles the main scene logic: Lobby
   /// </summary>
   public class LobbySceneManager : MonoBehaviour
   {
      //  Fields ---------------------------------------
      [SerializeField]
      private Configuration _configuration = null;

      [SerializeField]
      private LobbyUIView _lobbyUIView = null;

      /// <summary>
      /// This defines the matchmaking criteria including "NumberOfPlayers"
      /// </summary>
      [SerializeField]
      private SimGameTypeRef _onePlayerSimGameTypeRef;

      /// <summary>
      /// This defines the matchmaking criteria including "NumberOfPlayers"
      /// </summary>
      [SerializeField]
      private SimGameTypeRef _twoPlayerSimGameTypeRef;

      private BeamContext _beamContext;
      private TBFMatchmaking matchmaking;

      //  Unity Methods   ------------------------------
      protected void Start()
      {
         _lobbyUIView.BackButton.onClick.AddListener(BackButton_OnClicked);

         if (RuntimeDataStorage.Instance.TargetPlayerCount == RuntimeDataStorage.UnsetPlayerCount)
         {
            DebugLog($"Scene '{gameObject.scene.name}' was loaded directly. That is ok. Setting defaults.");
            RuntimeDataStorage.Instance.TargetPlayerCount = 1;
         }

         var text = string.Format(TBFConstants.StatusText_Joining, 0,
            RuntimeDataStorage.Instance.TargetPlayerCount);

         _lobbyUIView.BufferedText.SetText(text, TMP_BufferedText.BufferedTextMode.Immediate);

         SetupBeamable();
      }

      private Action _onDestroy;

      public void OnDestroy()
      {
         _onDestroy?.Invoke();
      }

      //  Other Methods   ------------------------------
      private async void SetupBeamable()
      {
         _beamContext = BeamContext.Default;
         await _beamContext.OnReady;
         
         SimGameType simGameType;

         if (RuntimeDataStorage.Instance.TargetPlayerCount == 1)
         {
            simGameType = await _onePlayerSimGameTypeRef.Resolve();
         }
         else if (RuntimeDataStorage.Instance.TargetPlayerCount == 2)
         {
            simGameType = await _twoPlayerSimGameTypeRef.Resolve();
         }
         else
         {
            throw new Exception("Codepath is never intended.");
         }

         RuntimeDataStorage.Instance.IsMatchmakingComplete = false;

         matchmaking = new TBFMatchmaking(_beamContext.Api.Experimental.MatchmakingService, simGameType,
            _beamContext.PlayerId);
         matchmaking.OnProgress.AddListener(MyMatchmaking_OnProgress);
         matchmaking.OnComplete.AddListener(MyMatchmaking_OnComplete);
         matchmaking.OnError.AddListener(MyMatchmaking_OnError);
         _onDestroy = async () =>
         {
            await matchmaking.CancelMatchmaking(); 
         };

         try
         {
            await matchmaking.StartMatchmaking();
         }
         catch (Exception e)
         {
            _lobbyUIView.BufferedText.SetText(TBFHelper.InternetOfflineInstructionsText,
               TMP_BufferedText.BufferedTextMode.Queue);
            Debug.LogError(e);
         }
      }

      private void DebugLog(string message)
      {
         if (TBFConstants.IsDebugLogging)
         {
            Debug.Log(message);
         }
      }


      //  Event Handlers -------------------------------
      private void BackButton_OnClicked()
      {
         matchmaking?.CancelMatchmaking();

         StartCoroutine(TBFHelper.LoadScene_Coroutine(_configuration.IntroSceneName,
            _configuration.DelayBeforeLoadScene));
      }


      private void MyMatchmaking_OnProgress(MyMatchmakingResult myMatchmakingResult)
      {
         DebugLog($"MyMatchmaking_OnProgress() " +
            $"Players={myMatchmakingResult.Players.Count}/{myMatchmakingResult.PlayerCountMax} " +
            $"MatchId={myMatchmakingResult.MatchId}");

         string text = string.Format(TBFConstants.StatusText_Joining,
            myMatchmakingResult.Players.Count,
            myMatchmakingResult.PlayerCountMax);

         _lobbyUIView.BufferedText.SetText(text, TMP_BufferedText.BufferedTextMode.Queue);
      }


      private void MyMatchmaking_OnComplete(MyMatchmakingResult myMatchmakingResult)
      {
         if (!RuntimeDataStorage.Instance.IsMatchmakingComplete)
         {
            string text = string.Format(TBFConstants.StatusText_Joined,
               myMatchmakingResult.Players.Count,
               myMatchmakingResult.PlayerCountMax);

            _lobbyUIView.BufferedText.SetText(text, TMP_BufferedText.BufferedTextMode.Queue);

            DebugLog($"MyMatchmaking_OnComplete() " +
               $"Players={myMatchmakingResult.Players.Count}/{myMatchmakingResult.PlayerCountMax} " +
               $"MatchId={myMatchmakingResult.MatchId}");

            //Store successful info here for use in another scene
            RuntimeDataStorage.Instance.IsMatchmakingComplete = true;
            RuntimeDataStorage.Instance.LocalPlayerDbid = myMatchmakingResult.LocalPlayer;
            RuntimeDataStorage.Instance.MatchId = myMatchmakingResult.MatchId;

            StartCoroutine(LoadScene_Coroutine());

         }
         else
         {
            throw new Exception("Codepath is never intended.");
         }
      }
      
      private void MyMatchmaking_OnError(MyMatchmakingResult myMatchmakingResult)
      {
         if (!RuntimeDataStorage.Instance.IsMatchmakingComplete)
         {
            string text = string.Format(TBFConstants.StatusText_Error,
               myMatchmakingResult.Players.Count,
               myMatchmakingResult.PlayerCountMax,
               matchmaking.MyMatchmakingResult.ErrorMessage);

            _lobbyUIView.BufferedText.SetText(text, TMP_BufferedText.BufferedTextMode.Immediate);

            DebugLog($"MyMatchmaking_OnComplete() " +
                     $"Players={myMatchmakingResult.Players.Count}/{myMatchmakingResult.PlayerCountMax} " +
                     $"ErrorMessage={matchmaking.MyMatchmakingResult.ErrorMessage}");

         }
         else
         {
            throw new Exception("Codepath is never intended.");
         }
      }

      private IEnumerator LoadScene_Coroutine()
      {
         //Wait for old messages to pass before changing scenes
         while (_lobbyUIView.BufferedText.HasRemainingQueueText)
         {
            yield return new WaitForEndOfFrame();
         }

         //Show final status message a little longer
         yield return new WaitForSeconds(0.5f);

         //Load another scene
         StartCoroutine(TBFHelper.LoadScene_Coroutine(_configuration.GameSceneName,
            _configuration.DelayBeforeLoadScene));
      }
   }
}
