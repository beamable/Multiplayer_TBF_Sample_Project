using Beamable.Samples.TBF.Animation;
using Beamable.Samples.TBF.Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Samples.TBF.Views
{
   /// <summary>
   /// Handles the audio/graphics rendering logic: Game
   /// </summary>
   public class GameUIView : MonoBehaviour
   {
      //  Properties -----------------------------------
      public TMP_Text StatusText { get { return _statusText; } }

      public AvatarUIView AvatarUIView_01 { get { return _avatarUIView_01; } }
      public AvatarUIView AvatarUIView_02 { get { return _avatarUIView_02; } }

      public Button BackButton { get { return _backButton; } }
      public Button MoveButton_01 { get { return _moveButton_01; } }
      public Button MoveButton_02 { get { return _moveButton_02; } }
      public Button MoveButton_03 { get { return _moveButton_03; } }

      public CanvasGroup MoveButtonsCanvasGroup { get { return _moveButtonsCanvasGroup; } }

      //  Fields ---------------------------------------
      [SerializeField]
      private Configuration _configuration = null;

      [SerializeField]
      private TMP_Text _statusText = null;

      [SerializeField]
      private Button _backButton = null;

      [SerializeField]
      private Button _moveButton_01 = null;

      [SerializeField]
      private Button _moveButton_02 = null;

      [SerializeField]
      private Button _moveButton_03 = null;

      [SerializeField]
      private CanvasGroup _moveButtonsCanvasGroup = null;

      [SerializeField]
      private AvatarUIView _avatarUIView_01 = null;

      [SerializeField]
      private AvatarUIView _avatarUIView_02 = null;

      [Header("Cosmetic Animation")]
      [SerializeField]
      private List<CanvasGroup> _canvasGroups = null;

      //  Unity Methods   ------------------------------
      protected void Start()
      {
         _statusText.text = "";
         TweenHelper.CanvasGroupsDoFade(_canvasGroups, 0, 1, 1, 0, _configuration.DelayFadeInUI);
      }
   }
}