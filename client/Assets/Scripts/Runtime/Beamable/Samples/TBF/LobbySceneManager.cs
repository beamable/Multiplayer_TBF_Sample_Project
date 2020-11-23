using Beamable.Samples.TBF.Data;
using Beamable.Samples.TBF.Views;
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


      //  Unity Methods   ------------------------------
      protected void Start()
      {
         _lobbyUIView.BackButton.onClick.AddListener(BackButton_OnClicked);

         _lobbyUIView.StatusBodyText.text = "Waiting...";
      }


      //  Event Handlers -------------------------------
      private void BackButton_OnClicked()
      {
         StartCoroutine(TBFHelper.LoadScene_Coroutine(_configuration.IntroSceneName,
            _configuration.DelayBeforeLoadScene));
      }


      private void MatchmakingService_OnComplete()
      {
         StartCoroutine(TBFHelper.LoadScene_Coroutine(_configuration.GameSceneName,
            _configuration.DelayBeforeLoadScene));
      }
   }
}