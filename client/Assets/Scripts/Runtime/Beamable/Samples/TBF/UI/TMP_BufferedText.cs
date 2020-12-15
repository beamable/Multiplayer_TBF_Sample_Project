﻿using Beamable.Samples.TBF.Exceptions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Beamable.Samples.TBF.UI
{
   /// <summary>
   /// Wraps a <see cref="TMP_Text"/>. Allows game makers to 
   /// queue multiple display texts to the same UI text field 
   /// and each will be displayed in series over time.
   /// </summary>
   public class TMP_BufferedText : MonoBehaviour
   {
      //  Properties --------------------------------------

      /// <summary>
      /// Determines how many remain. 
      /// </summary>
      public int BufferedTextCount { get { return _bufferedTextQueue.Count; } }

      //  Fields ---------------------------------------
      public enum BufferedTextMode
      {
         Null,
         Immediate,
         Queue,
      }

      [SerializeField]
      private TMP_Text _text = null;

      [Header("Cosmetic Delays")]
      [Range(0, 3)]
      [SerializeField]
      public float _minDisplayDuration = 3;

      private Queue<string> _bufferedTextQueue = new Queue<string>();
      private float _bufferedTextElapsedTime = 0;

      //  Unity Methods   ------------------------------
      protected void Update()
      {
         // Every x seconds, show the next text
         _bufferedTextElapsedTime += Time.deltaTime;
         if (_bufferedTextElapsedTime > _minDisplayDuration)
         {
            _bufferedTextElapsedTime = 0f;
            if (_bufferedTextQueue.Count > 0)
            {
               string message = _bufferedTextQueue.Dequeue();
               SetTextInternal(message);
               
            }
         }
      }


      //  Other Methods   ------------------------------

      /// <summary>
      /// Queue and display text in the UI.
      /// </summary>
      /// <param name="displayText"></param>
      /// <param name="bufferedTextMode"></param>
      public void SetText(string displayText, BufferedTextMode bufferedTextMode)
      {
         switch (bufferedTextMode)
         {
            case BufferedTextMode.Immediate:

               //Remove all other texts
               _bufferedTextQueue.Clear();

               //Display now
               SetTextInternal(displayText);

               break;
            case BufferedTextMode.Queue:
               //Queue so it lives a minimum lifetime regardless
               //of subsequent "SetText" calls.
               _bufferedTextQueue.Enqueue(displayText);
               break;
            default:
               SwitchDefaultException.Throw(bufferedTextMode);
               break;
         }
      }


      private void SetTextInternal(string displayText)
      {
         _text.text = displayText;
      }
   }
}