using Beamable.Editor.UI.Buss;

namespace Beamable.Editor.Content.Components
{
   public class ContentManagerComponent : BeamableVisualElement
   {
      public ContentManagerComponent(string name) : base($"{ContentManagerContants.COMP_PATH}/{name}/{name}")
      {

      }
   }
}