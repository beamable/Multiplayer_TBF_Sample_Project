using Beamable.Samples.TBF.Animation;
using Beamable.Samples.TBF.Data;
using Beamable.Samples.TBF.Exceptions;
using System;
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
      public enum StatusTextMode
      {
         Null,
         Immediate,
         Queue,
      }

      //  Properties -----------------------------------
      public List<AvatarView> AvatarViews { get { return _avatarViews; } }
      public List<AvatarUIView> AvatarUIViews { get { return _avatarUIViews; } }
      //
      public Button BackButton { get { return _backButton; } }
      public Button MoveButton_01 { get { return _moveButton_01; } }
      public Button MoveButton_02 { get { return _moveButton_02; } }
      public Button MoveButton_03 { get { return _moveButton_03; } }
      //
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
      private List<AvatarUIView> _avatarUIViews = null;

      [SerializeField]
      private List<AvatarView> _avatarViews = null;

      [Header("Cosmetic Animation")]
      [SerializeField]
      private List<CanvasGroup> _canvasGroups = null;

      private Queue<string> _statusMessageQueue = new Queue<string>();

      private float _statusMessageTimerElapsed = 0;

      //  Unity Methods   ------------------------------
      protected void Start()
      {
         for (int i = 0; i < _avatarUIViews.Count; i++)
         {
            _avatarUIViews[i].AvatarData = _configuration.AvatarDatas[i];
         }

         TweenHelper.CanvasGroupsDoFade(_canvasGroups, 0, 1, 1, 0, _configuration.DelayFadeInUI);
      }

      protected void Update()
      {
         // Every x seconds, show the next status message
         _statusMessageTimerElapsed += Time.deltaTime;
         Debug.Log(_statusMessageTimerElapsed);
         if (_statusMessageTimerElapsed > _configuration.StatusMessageMinDuration)
         {
            _statusMessageTimerElapsed = 0f;
            if (_statusMessageQueue.Count > 0)
            {
               string message = _statusMessageQueue.Dequeue();
               _statusText.text = message;
               Debug.Log("_statusText.text: " + _statusText.text);
            }
         }
      }


      //  Unity Methods   ------------------------------
      public void SetStatusText(string message, StatusTextMode statusTextMode)
      {
         switch (statusTextMode)
         {
            case StatusTextMode.Immediate:

               //set now
               _statusText.text = message;

               //and queue so it lives a minimum lifetime
               _statusMessageQueue.Clear();
               
               break;
            case StatusTextMode.Queue:
               _statusMessageQueue.Enqueue(message);
               break;
            default:
               SwitchDefaultException.Throw(statusTextMode);
               break;
         }
      }
   }
}