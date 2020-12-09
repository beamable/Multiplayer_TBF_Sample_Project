using Beamable.Samples.TBF.Audio;
using Beamable.Samples.TBF.Data;
using Beamable.Samples.TBF.Exceptions;
using System;
using UnityEngine;

namespace Beamable.Samples.TBF.Views
{
   /// <summary>
   /// Handles the audio/graphics rendering logic: Avatar
   /// </summary>
   public class AvatarView : MonoBehaviour
   {
      //  Properties -----------------------------------

      //  Fields ---------------------------------------
      [SerializeField]
      private Animator _animator = null;

      [SerializeField]
      private Configuration _configuration = null;

      //  Other Methods --------------------------------
      public void PlayAnimationIdle()
      {
         _animator.SetTrigger(TBFConstants.Avatar_Idle);
      }

      public void PlayAnimationWin()
      {
         _animator.SetTrigger(TBFConstants.Avatar_Death);
      }

      public void PlayAnimationByGameMoveType(GameMoveType gameMoveType)
      {
         switch (gameMoveType)
         {
            case GameMoveType.High:
               _animator.SetTrigger(TBFConstants.Avatar_Attack_01);
               PlayAudioClipDelayed(SoundConstants.Attack_01, _configuration.DelayBeforeSoundAttack_01a);
               PlayAudioClipDelayed(SoundConstants.Attack_01, _configuration.DelayBeforeSoundAttack_01b);
               break;
            case GameMoveType.Medium:
               _animator.SetTrigger(TBFConstants.Avatar_Attack_02);
               PlayAudioClipDelayed(SoundConstants.Attack_02, _configuration.DelayBeforeSoundAttack_02a);
               PlayAudioClipDelayed(SoundConstants.Attack_02, _configuration.DelayBeforeSoundAttack_02b);
               break;
            case GameMoveType.Low:
               Debug.Log("doing: " + TBFConstants.Avatar_Attack_03);
               _animator.SetTrigger(TBFConstants.Avatar_Attack_03);
               PlayAudioClipDelayed(SoundConstants.Attack_03, _configuration.DelayBeforeSoundAttack_03);
               break;
            default:
               SwitchDefaultException.Throw(gameMoveType);
               break;
         }
      }

      private void PlayAudioClipDelayed(string attack_01, object delayBeforeSoundAttack_01)
      {
         throw new NotImplementedException();
      }

      private void PlayAudioClipDelayed(string audioClipName, float delay)
      {
         SoundManager.Instance.PlayAudioClipDelayed(audioClipName, delay);
      }



      //  Event Handlers -------------------------------
   }
}