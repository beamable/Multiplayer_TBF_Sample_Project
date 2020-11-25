using Beamable.Modules.Theme.Palettes;
using Beamable.UI.Scripts;

namespace Beamable.Modules.Theme.Appliers
{
   [System.Serializable]
   public class ButtonStyleApplier : StyleApplier<DisruptorButton>
   {
      public ButtonBinding Button;

      private ImageStyleApplier _imageApplier = new ImageStyleApplier();
      private SelectableStyleApplier _selectableApplier = new SelectableStyleApplier();
      private TextStyleApplier _textApplier = new TextStyleApplier();
      private GradientStyleApplier _gradientApplier = new GradientStyleApplier();
      private SoundStyleApplier _soundApplier = new SoundStyleApplier();

      public override void Apply(ThemeObject theme, DisruptorButton component)
      {
         var buttonStyle = theme.GetPaletteStyle(Button);
         if (buttonStyle == null) return;

         if (component.Button != null)
         {
            _selectableApplier.SelectableBinding = buttonStyle.Selectable;
            _selectableApplier.Apply(theme, component.Button);
            if (component.Button.image != null)
            {
               _imageApplier.ImageBinding = buttonStyle.Image;
               _imageApplier.ColorBinding = buttonStyle.ImageColor;
               _imageApplier.Apply(theme, component.Button.image);
            }
         }

         if (component.SoundBehaviour != null)
         {
            _soundApplier.Sound = buttonStyle.ClickSound;
            _soundApplier.Apply(theme, component.SoundBehaviour);
         }

         if (component.Text != null)
         {
            if (buttonStyle.OverrideAlignment)
            {
               component.Text.alignment = buttonStyle.Alignment;
            }
            _textApplier.ColorBinding = buttonStyle.TextColor;
            _textApplier.TextBinding = buttonStyle.Text;
            _textApplier.Apply(theme, component.Text);
         }

         if (component.Gradient != null)
         {
            _gradientApplier.GradientBinding = buttonStyle.Gradient;
            _gradientApplier.Apply(theme, component.Gradient);
         }

      }
   }
}