using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace soulful
{
    public class BeatUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI beatnum;

        public void SetBeat(int beat)
        {
            beatnum.text = beat.ToString();
        }

        public void Clear()
        {
            beatnum.text = "";
        }
    }
}