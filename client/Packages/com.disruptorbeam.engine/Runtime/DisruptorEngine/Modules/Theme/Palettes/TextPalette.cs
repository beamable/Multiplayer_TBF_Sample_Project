using TMPro;
using UnityEngine;

namespace Beamable.Modules.Theme.Palettes
{
   [System.Serializable]
   public class TextStyle : PaletteStyle
   {
      public FontBinding Font;
      public float FontSize = 24f;
      public FontStyles[] FontStyles;
      public Color TintColor = Color.white;
      public StyleBlendMode BlendMode = StyleBlendMode.Multiply;
      public float LineSpacing = 0;
      public float CharacterSpacing = 0;

      public override PaletteStyle Clone()
      {
         return new TextStyle
         {
            Name = Name,
            Enabled = Enabled,
            Font = Font.Clone(),
            FontStyles = FontStyles,
            TintColor = TintColor,
            BlendMode = BlendMode,
            LineSpacing = LineSpacing,
            CharacterSpacing = CharacterSpacing
         };
      }
   }

   [System.Serializable]
   public class TextPalette : Palette<TextStyle> {
      public override TextStyle DefaultValue()
      {
         return new TextStyle
         {
            Name = "default",
            Enabled = true,
            FontSize = 18,
            CharacterSpacing = 0,
            LineSpacing = 0,
            FontStyles = new [] {FontStyles.Normal},
            Font = new FontBinding
            {
               Name = null
            }
         };
      }
   }



   [System.Serializable]
   public class TextBinding : TextPalette.PaletteBinding { }
}