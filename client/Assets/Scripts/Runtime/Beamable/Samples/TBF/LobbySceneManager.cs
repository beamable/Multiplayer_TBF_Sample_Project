using Beamable.Samples.TBF.Audio;
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

      private IBeamableAPI _beamableAPI = null;

      //  Unity Methods   ------------------------------
      protected void Start()
      {
         _lobbyUIView.BackButton.onClick.AddListener(BackButton_OnClicked);
         _lobbyUIView.StatusBodyText.text = TBFConstants.StatusText_Waiting;

         SetupBeamable();
      }

      //  Other Methods   ------------------------------
      private async void SetupBeamable()
      {
         await Beamable.API.Instance.Then(de =>
         {
            try
            {
               _beamableAPI = de;

               RuntimeDataStorage.Instance.IsMatchmakingComplete = false;

               //TODO: Remove
               MatchmakingService_OnComplete();

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
         StartCoroutine(TBFHelper.LoadScene_Coroutine(_configuration.IntroSceneName,
            _configuration.DelayBeforeLoadScene));

         //TODO: Cancel matchmaking
      }


      private void MatchmakingService_OnComplete()
      {

         Debug.Log("Lobby1, IsMatchmakingComplete: " + RuntimeDataStorage.Instance.IsMatchmakingComplete);

         if (!RuntimeDataStorage.Instance.IsMatchmakingComplete)
         {
            RuntimeDataStorage.Instance.IsMatchmakingComplete = true;

            Debug.Log("Lobby2, IsMatchmakingComplete: " + RuntimeDataStorage.Instance.IsMatchmakingComplete);

            RuntimeDataStorage.Instance.LocalPlayerDbid = _beamableAPI.User.id;
            RuntimeDataStorage.Instance.TargetPlayerCount = 1;
            RuntimeDataStorage.Instance.RoomId = TBFMatchmaking.GetRandomRoomId();

            StartCoroutine(TBFHelper.LoadScene_Coroutine(_configuration.GameSceneName,
               _configuration.DelayBeforeLoadScene));
         }
      }
   }
}