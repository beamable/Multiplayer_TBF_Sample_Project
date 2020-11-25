using Beamable.Platform.SDK.Auth;
using Beamable.UI.Scripts;

public class AccountExistsSelect : MenuBase
{
   public AccountDisplayItem AccountDisplayItem;

   public void SetForUser(User user)
   {
      AccountDisplayItem.gameObject.SetActive(false);
      AccountDisplayItem.StartLoading(user, false, null).Then(_ => AccountDisplayItem.Apply());
   }

   public void SetForUserImmediate(AccountDisplayItem other)
   {
      AccountDisplayItem.StartLoading(other).Then(_ => AccountDisplayItem.Apply());
   }

   // This is referenced in the account flow prefab.
   public void OnCancel()
   {
      Hide();
   }
}
