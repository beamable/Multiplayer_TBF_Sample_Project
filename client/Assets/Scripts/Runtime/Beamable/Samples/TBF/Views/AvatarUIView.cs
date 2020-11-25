﻿using UnityEngine;

namespace Beamable.Samples.TBF.Views
{
   /// <summary>
   /// Handles the audio/graphics rendering logic: Avatar UI
   /// </summary>
   public class AvatarUIView : MonoBehaviour
   {
      //  Properties -----------------------------------
      public HealthBarView HealthBarView { get { return _healthBarView; } }

      //  Fields ---------------------------------------
      [SerializeField]
      private HealthBarView _healthBarView = null;

      //  Unity Methods   ------------------------------
      protected void Start()
      {
      }
   }
}