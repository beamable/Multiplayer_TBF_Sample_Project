using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Gradient = UnityEngine.UI.Extensions.Gradient;

namespace Beamable.UI.Scripts
{
   public class DisruptorWindow : UIBehaviour
   {
      public Image HeaderImage;
      public Gradient HeaderGradient;
      public LayoutElement HeaderElement;
      public RectTransform WindowTransform;
   }
}