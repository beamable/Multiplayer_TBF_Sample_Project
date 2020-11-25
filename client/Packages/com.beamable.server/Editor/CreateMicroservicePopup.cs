using UnityEngine;
using UnityEditor;

namespace Beamable.Server.Editor
{
    public class CreateMicroservicePopup : EditorWindow
    {
      string newMicroserviceName = "NewMicroservice";

      [MenuItem(
         ContentConstants.MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_MICROSERVICES + "/" +
         "<Create New...>",
         priority = ContentConstants.MENU_ITEM_PATH_WINDOW_PRIORITY_3)]
        static void Init()
        {
            CreateMicroservicePopup window = ScriptableObject.CreateInstance<CreateMicroservicePopup>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 80);
            window.ShowPopup();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Enter Name of new Microservice (only A-Z):", EditorStyles.wordWrappedLabel);
            newMicroserviceName = GUILayout.TextField(newMicroserviceName, 100);
            if (GUILayout.Button("OK"))
            {
                MicroserviceEditor.CreateNewMicroservice(newMicroserviceName);
                this.Close();
            }

            if (GUILayout.Button("Cancel"))
            {
                this.Close();
            }
        }
    }
}