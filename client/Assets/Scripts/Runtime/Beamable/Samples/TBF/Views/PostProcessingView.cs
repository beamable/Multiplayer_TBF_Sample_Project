using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Beamable.Samples.TBF.Views
{
   /// <summary>
   /// Handles the audio/graphics rendering logic: Post processing <see cref="Volume"/>. 
   /// </summary>
   [ExecuteAlways]
   public class PostProcessingView : MonoBehaviour
   {
      //  Properties -----------------------------------
      public float Intensity 
      {  
         get 
         {
            return _intensity; 
         }
         set
         {
            _intensity = value;
            Render();
         }
      }

         
      //  Fields ---------------------------------------
      [SerializeField]
      private Volume _volume = null;

      private float _intensity = 1;

      //  Unity Methods   ------------------------------
      void OnEnable()
      {
         // This trickery is optional and for ease-of development.
         // Game makers can see the results of SOME changes during edit-time
#if UNITY_EDITOR
         EditorApplication.update += UpdateAlways;
#endif
      }


      void OnDisable()
      {
#if UNITY_EDITOR
         EditorApplication.update -= UpdateAlways;
#endif
      }


      protected void Update()
      {
         UpdateAlways();
      }


      //  Other Methods --------------------------------
      private void Render()
      {
         LensDistortion lensDistortion = null;
         _volume.profile.TryGet<LensDistortion>(out lensDistortion);

         if (lensDistortion != null)
         {
            lensDistortion.intensity.value = _intensity;
         }
      }


      //  Event Handlers -------------------------------
      private void UpdateAlways()
      {
         Render();
      }
   }
}