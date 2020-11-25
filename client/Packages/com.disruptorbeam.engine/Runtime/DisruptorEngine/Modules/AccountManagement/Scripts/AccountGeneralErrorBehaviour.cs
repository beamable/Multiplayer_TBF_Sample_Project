using Beamable.UI.Scripts;

namespace Beamable.Modules.AccountManagement
{
   public class AccountGeneralErrorBehaviour : MenuBase
   {
      public TextReference ErrorText;

      public void Show(string error)
      {
         var menu = Manager.Show<AccountGeneralErrorBehaviour>();
         ErrorText.Value = error;
      }
   }
}