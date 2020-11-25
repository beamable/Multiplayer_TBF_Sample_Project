using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Beamable.Modules.Tournaments
{

    public class TournamentNumbersBehaviour : MonoBehaviour
    {
        public TextMeshProUGUI Text;

        public void Set(int number)
        {
            var isActive = number > 0;
            gameObject.SetActive(isActive);
            Text.text = "" + number;
        }
    }

}