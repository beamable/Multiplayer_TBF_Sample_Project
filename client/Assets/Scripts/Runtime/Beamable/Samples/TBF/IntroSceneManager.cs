using Beamable.Samples.TBF.Data;
using Beamable.Samples.TBF.Views;
using DisruptorBeam;
using System;
using UnityEngine;

namespace Beamable.Samples.TBF
{
   /// <summary>
   /// Handles the main scene logic: Intro
   /// </summary>
   public class IntroSceneManager : MonoBehaviour
   {
      //  Fields ---------------------------------------

      /// <summary>
      /// Determines if we are demo mode. Demo mode does several operations
      /// which are not recommended in a production project including 
      /// creating mock data for the game.
      /// </summary>
      private static bool IsDemoMode = true;

      [SerializeField]
      private IntroUIView _introUIView = null;

      [SerializeField]
      private Configuration _configuration = null;

      private IDisruptorEngine _disruptorEngine = null;
      private bool _isConnected = false;
      private bool _isBeamableSDKInstalled = false;
      private string _isBeamableSDKInstalledErrorMessage = "";

      //  Unity Methods   ------------------------------
      protected void Start()
      {
         _introUIView.AboutBodyText = "";
         _introUIView.StartGameButton.onClick.AddListener(StartGameButton_OnClicked);
         SetupBeamable();
      }


      protected void OnDestroy()
      {
         DisruptorEngine.Instance.Then(de =>
         {
            _disruptorEngine = null;
            de.ConnectivityService.OnConnectivityChanged -= ConnectivityService_OnConnectivityChanged;
         });
      }


      //  Other Methods --------------------------------

      /// <summary>
      /// Login with Beamable and fetch user/session information
      /// </summary>
      private void SetupBeamable()
      {
         // Attempt Connection to Beamable
         DisruptorEngine.Instance.Then(de =>
         {
            try
            {
               _disruptorEngine = de;
               _isBeamableSDKInstalled = true;

               // Handle any changes to the internet connectivity
               _disruptorEngine.ConnectivityService.OnConnectivityChanged += ConnectivityService_OnConnectivityChanged;
               ConnectivityService_OnConnectivityChanged(_disruptorEngine.ConnectivityService.HasConnectivity);

               if (IsDemoMode)
               {
                  //Set my player's name
                  //MockDataCreator.SetCurrentUserAlias(_disruptorEngine.Stats, "This_is_you:)");
               }
            }
            catch (Exception e)
            {
               // Failed to connect (e.g. not logged in)
               _isBeamableSDKInstalled = false;
               _isBeamableSDKInstalledErrorMessage = e.Message;
               ConnectivityService_OnConnectivityChanged(false);
            }
         });
      }


      /// <summary>
      /// Render the user-facing text with success or helpful errors.
      /// </summary>
      private void RenderUI()
      {
         long dbid = 0;
         if (_isConnected)
         {
            dbid = _disruptorEngine.User.id;
         }

         string aboutBodyText = TBFHelper.GetIntroAboutBodyText(
            _isConnected, 
            dbid, 
            _isBeamableSDKInstalled, 
            _isBeamableSDKInstalledErrorMessage);

         _introUIView.AboutBodyText = aboutBodyText;
         _introUIView.StartGameButton.interactable = _isConnected;
      }


      //  Event Handlers -------------------------------
      private void ConnectivityService_OnConnectivityChanged(bool isConnected)
      {
         _isConnected = isConnected;
         RenderUI();
      }

      private void StartGameButton_OnClicked()
      {
         _introUIView.StartGameButton.interactable = false;

         StartCoroutine(TBFHelper.LoadScene_Coroutine(_configuration.LobbySceneName, 
            _configuration.DelayBeforeLoadScene));
      }
   }
}