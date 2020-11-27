using Beamable.Samples.TBF.Exceptions;
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

      //  Other Methods --------------------------------
      public void DoIdle()
      {
         _animator.SetTrigger(TBFConstants.Avatar_Idle);
      }

      public void DoCover()
      {
         //TODO: Needed?
         _animator.SetTrigger(TBFConstants.Avatar_Cover);
      }

      public void Attack(GameMoveType gameMoveType)
      {
         switch (gameMoveType)
         {
            case GameMoveType.High:
               _animator.SetTrigger(TBFConstants.Avatar_Attack01);
               break;
            case GameMoveType.Medium:
               _animator.SetTrigger(TBFConstants.Avatar_Attack02);
               break;
            case GameMoveType.Low:
               _animator.SetTrigger(TBFConstants.Avatar_Attack03);
               break;
            default:
               SwitchDefaultException.Throw(gameMoveType);
               break;
         }
      }

      //  Event Handlers -------------------------------
   }
}