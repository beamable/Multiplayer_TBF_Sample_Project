using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.RectTransform;

namespace Beamable.Samples.BBB.Views
{
   /// <summary>
   /// Handles the view concerns for UI elements related
   /// to health bar for a game character
   /// </summary>
   public class HealthBarView : MonoBehaviour
   {
      //  Properties -----------------------------------
      public int Health
      {
         set
         {
            value = Mathf.Clamp(value, 0, 100);

            SetFillImageWidthPercent (value);

            if (_isAlignedLeft)
            {
               _text.text = $"{_name} {value}%";
            }
            else
            {
               _text.text = $"{value}% {_name}";
            }
         }
      }

      //  Fields ---------------------------------------
      [SerializeField]
      private string _name = null;

      [SerializeField]
      private Color _backgroundColor = Color.white;

      [SerializeField]
      private Image _backgroundImage = null;

      [SerializeField]
      private Image _fillImage = null;

      [SerializeField]
      private TMP_Text _text = null;

      [SerializeField]
      private bool _isAlignedLeft = false;

      private Tween _tween = null;

      //  Unity Methods   ------------------------------
      protected void OnValidate()
      {
         if (!Application.isPlaying)
         {
            // DOTween works only at runtime
            Health = 100;
         }

         if (_backgroundImage != null)
         {
            _backgroundImage.color = _backgroundColor;
         }
      }

      //  Other Methods   ------------------------------
      private void SetFillImageWidthPercent(float targetPercent)
      {
         float maxWidth = _backgroundImage.rectTransform.sizeDelta.x;
         float fromWidth = _fillImage.rectTransform.sizeDelta.x;
         float toWidth = (maxWidth * targetPercent) / 100;

         if (_tween != null)
         {
            _tween.Kill();
         }

         _tween = DOTween.To(nextWidth =>
         {
            _fillImage.rectTransform.SetSizeWithCurrentAnchors(Axis.Horizontal, nextWidth);
         }, fromWidth, toWidth, 0.5f);
      }
   }
}