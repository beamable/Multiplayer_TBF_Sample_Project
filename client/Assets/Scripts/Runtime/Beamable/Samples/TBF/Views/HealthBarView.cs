using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Samples.TBF.Views
{
   /// <summary>
   /// Handles the view concerns for UI elements related
   /// to health bar for a game character
   /// </summary>
   public class HealthBarView : MonoBehaviour
   {
      //  Properties -----------------------------------
      public int Health { set { _health = Mathf.Clamp(value, 0, 100); Render(); } get { return _health; } }
      public string Title { set { _title = value; Render(); } get { return _title; } }
      public Color BackgroundColor { set { _backgroundColor = value; Render(); } get { return _backgroundColor; } }

      
      //  Fields ---------------------------------------
      [SerializeField]
      private string _title = "";

      [SerializeField]
      private Color _backgroundColor = Color.white;

      [SerializeField]
      private Image _backgroundImage = null;

      [SerializeField]
      private Slider _slider = null;

      [SerializeField]
      private TMP_Text _text = null;

      [SerializeField]
      private bool _isAlignedLeft = false;

      [SerializeField]
      private int _health = 100;

      private Tween _tween = null;


      //  Unity Methods   ------------------------------
      protected void OnValidate()
      {
         //cap values
         Health = _health;

         //Debug the rendering in edit mode
         //as the inspector values are manually changed
         Render();
      }


      //  Other Methods   ------------------------------
      private void Render()
      {
         if (_backgroundImage != null)
         {
            _backgroundImage.color = _backgroundColor;
         }

         SetFillImageWidthPercent(_health);

         if (_isAlignedLeft)
         {
            _text.text = $"{_title} {_health}%";
         }
         else
         {
            _text.text = $"{_health}% {_title}";
         }
      }


      private void SetFillImageWidthPercent(float targetPercent)
      {
         if (_tween != null)
         {
            _tween.Kill();
         }

         //DOTween works only at runtime
         if (Application.isPlaying)
         {
            _tween = DOTween.To(nextWidth =>
            {
               _slider.value = nextWidth;
            }, _slider.value, targetPercent, 0.5f);
         }
         else
         {
            _slider.value = targetPercent;
         }
      }
   }
}