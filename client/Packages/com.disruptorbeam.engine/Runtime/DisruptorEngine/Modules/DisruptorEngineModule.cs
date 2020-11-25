using UnityEngine;
using UnityEngine.EventSystems;

namespace Beamable.Modules
{
   public class DisruptorEngineModule : MonoBehaviour
   {
      void Start()
      {
         if (EventSystem.current == null)
         {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
         }
      }
   }
}