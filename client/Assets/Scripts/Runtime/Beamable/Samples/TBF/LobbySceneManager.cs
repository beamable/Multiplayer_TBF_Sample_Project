using Beamable.Content;
using Beamable.Examples.Features.Multiplayer;
using Beamable.Samples.TBF.Data;
using Beamable.Samples.TBF.Multiplayer;
using Beamable.Samples.TBF.Views;
using System;
using UnityEngine;

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

      private IBeamableAPI _beamableAPI = null;
      private TBFMatchmaking matchmaking;

      //  Unity Methods   ------------------------------
      protected void Start()
      {
         _lobbyUIView.BackButton.onClick.AddListener(BackButton_OnClicked);

         if (RuntimeDataStorage.Instance.TargetPlayerCount == RuntimeDataStorage.UnsetPlayerCount)
         {
            Debug.Log($"Scene '{gameObject.scene.name}' was loaded directly. That is ok. Setting defaults.");
            RuntimeDataStorage.Instance.TargetPlayerCount = 1;
         }

         _lobbyUIView.StatusBodyText.text = string.Format(TBFConstants.StatusText_Waiting, 0, 
            RuntimeDataStorage.Instance.TargetPlayerCount);

         SetupBeamable();
      }

      //  Other Methods   ------------------------------
      private async void SetupBeamable()
      {
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
            
         await Beamable.API.Instance.Then(async de =>
         {
            try
            {
               _beamableAPI = de;

               RuntimeDataStorage.Instance.IsMatchmakingComplete = false;

               matchmaking = new TBFMatchmaking(de.Matchmaking, simGameType, _beamableAPI.User.id);
               matchmaking.OnProgress += MyMatchmaking_OnProgress;
               matchmaking.OnComplete += MyMatchmaking_OnComplete;
               await matchmaking.Start();

            }
            catch (Exception)
            {
               _lobbyUIView.StatusBodyText.text = TBFHelper.InternetOfflineInstructionsText;
            }
         });
      }


      //  Event Handlers -------------------------------
      private void BackButton_OnClicked()
      {
         matchmaking.Stop();

         StartCoroutine(TBFHelper.LoadScene_Coroutine(_configuration.IntroSceneName,
            _configuration.DelayBeforeLoadScene));
      }


      private void MyMatchmaking_OnProgress(MyMatchmakingResult myMatchmakingResult)
      {
         Debug.Log($"MyMatchmaking_OnProgress() " +
            $"Players={myMatchmakingResult.Players.Count}/{myMatchmakingResult.TargetPlayerCount} " +
            $"RoomId={myMatchmakingResult.RoomId}");

         _lobbyUIView.StatusBodyText.text = string.Format(TBFConstants.StatusText_Waiting,
            myMatchmakingResult.Players.Count,
            myMatchmakingResult.TargetPlayerCount);
      }


      private void MyMatchmaking_OnComplete(MyMatchmakingResult myMatchmakingResult)
      {
         if (!RuntimeDataStorage.Instance.IsMatchmakingComplete)
         {
            if (myMatchmakingResult.IsError)
            {
               Debug.Log($"MyMatchmaking_OnComplete() " +
                  $"Error={myMatchmakingResult.ErrorMessage}.");
               return;
            }

            Debug.Log($"MyMatchmaking_OnComplete() " +
               $"Players={myMatchmakingResult.Players.Count}/{myMatchmakingResult.TargetPlayerCount} " +
               $"RoomId={myMatchmakingResult.RoomId}");

            //Store successful info here for use in another scene
            RuntimeDataStorage.Instance.IsMatchmakingComplete = true;
            RuntimeDataStorage.Instance.LocalPlayerDbid = myMatchmakingResult.LocalPlayerDbid;
            RuntimeDataStorage.Instance.RoomId = myMatchmakingResult.RoomId;

            //Load another scene
            StartCoroutine(TBFHelper.LoadScene_Coroutine(_configuration.GameSceneName,
               _configuration.DelayBeforeLoadScene));
         }
         else
         {
            throw new Exception("Codepath is never intended.");
         }
      }
   }
}