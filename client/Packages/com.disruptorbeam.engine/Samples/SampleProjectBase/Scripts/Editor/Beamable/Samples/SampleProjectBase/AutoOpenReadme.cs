using UnityEditor;
namespace Beamable.Samples.SampleProjectBase
{
	/// <summary>
	/// Ping a custom-formatted readme file and force-show in inspector. Parse the
	/// custom format to markdown-like display.
	///
	/// Inspired by Unity's "Learn" Sample Projects
	///
	/// NOTE: Want to create a NEW SAMPLE PROJECT that has its own Readme? Include a COPY of this class
	/// in a new namespace that fits the sample project.
	///
	/// </summary>
	[CustomEditor(typeof(Readme))]
	[InitializeOnLoad]
	public class AutoOpenReadme : ReadmeEditor
	{
		private static string SessionStateKeyWasAlreadyShown = "Beamable.Samples.SampleProjectBase.AutoOpenReadme.wasAlreadyShown";
		private static string FindAssetsFilter = "Readme t:Readme";
		private static string[] FindAssetsFolders = new string[] { "Packages" };

		static AutoOpenReadme()
		{
			EditorApplication.delayCall += SelectReadmeAutomatically;
		}

		private static void SelectReadmeAutomatically()
		{

//Disable feature for devs - SamR
#if BEAMABLE_DEVELOPER
			return;
#else
			if (!SessionState.GetBool(SessionStateKeyWasAlreadyShown, false))
			{
				SelectSpecificReadmeMenuItem();
				SessionState.SetBool(SessionStateKeyWasAlreadyShown, true);
			}
#endif
		}

//Disable for game makers - SamR
#if BEAMABLE_DEVELOPER
		[MenuItem(
			ContentConstants.MENU_ITEM_PATH_WINDOW_BEAMABLE_BEAMABLE_DEVELOPER_SAMPLES + "/" +
			ContentConstants.OPEN + " " +
			"`Beamable SDK For Unity` Readme",
			priority = ContentConstants.MENU_ITEM_PATH_WINDOW_PRIORITY_4)]
#endif
		private static Readme SelectSpecificReadmeMenuItem()
		{
			// Reset SessionState if/when MenuItem is used
			SessionState.SetBool(SessionStateKeyWasAlreadyShown, false);
			return ReadmeEditor.SelectReadme(FindAssetsFilter, FindAssetsFolders);

		}
	}
}
