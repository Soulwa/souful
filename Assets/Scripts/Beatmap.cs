using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace soulful
{
    [Serializable]
    public class Beatmap
    {
        public List<SpawnInstruction> instructions;
        public int notes;
        public SongInfo songInfo;

        public void Initialize()
        {
            instructions = new List<SpawnInstruction>();
        }

        public void Add(SpawnInstruction i)
        {
            instructions.Add(i);
        }

        public void Clear()
        {
            instructions.Clear();
        }

        public Beatmap DeepCopy()
        {
            return new Beatmap
            {
                instructions = instructions.Select(i => i).ToList(),
                notes = notes,
                songInfo = new SongInfo
                {
                    bpm = songInfo.bpm,
                    pathToMusic = songInfo.pathToMusic,
                    name = songInfo.name,
                },
            };
        }
    }

    [Serializable]
    public class SongInfo
    {
        public string name;
        public float bpm;
        public string pathToMusic;
    }
}