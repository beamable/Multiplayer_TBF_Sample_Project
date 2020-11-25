using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Editor.Toolbox
{
   //[CreateAssetMenu(fileName = "ToolboxConfiguration", menuName = "Disruptor Beam/Toolbox Configuration", order = 0)]
   public class ToolboxConfiguration : ScriptableObject
   {
      public bool ForceUpdate; // TODO REMOVE THIS HACK USED FOR REFRESH IN EDITOR...

      public List<ToolboxCategory> Categories;
   }

   [System.Serializable]
   public class ToolboxWidget
   {
      public string Name;
      public string Description;
      public Texture Icon;
      public GameObject DFab; // TODO make this more type specific
   }

   [System.Serializable]
   public class ToolboxCategory
   {
      public string Name;
      public string Description;
      public bool OpenByDefault;
      public Texture Icon;
      public List<ToolboxWidget> Widgets;
   }
}