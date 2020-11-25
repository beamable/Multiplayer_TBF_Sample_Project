
using UnityEngine;
using UnityEngine.UI.Extensions;
using Gradient = UnityEngine.UI.Extensions.Gradient;
using GradientMode = UnityEngine.UI.Extensions.GradientMode;

namespace Beamable.Modules.Theme.Objects
{
   [System.Serializable]
   public class GradientStyleObject : StyleObject<UnityEngine.UI.Extensions.Gradient>
   {

      public GradientMode _gradientMode = GradientMode.Global;

      public GradientDir _gradientDir = GradientDir.Vertical;

      public bool _overwriteAllColor = false;

      public Color _vertex1 = Color.white;

      public Color _vertex2 = Color.white;

      protected override void Apply(Gradient target)
      {
         target.GradientMode = _gradientMode;
         target.GradientDir = _gradientDir;
         target.OverwriteAllColor = _overwriteAllColor;
         target.Vertex1 = _vertex1;
         target.Vertex2 = _vertex2;
      }
   }
}