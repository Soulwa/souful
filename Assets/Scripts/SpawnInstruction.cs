using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace soulful
{
    [Serializable]
    public struct SpawnInstruction
    {
        public bool up, down, left, right;
        public float targetBeat;
        // public float targetSongTime;
    }
}