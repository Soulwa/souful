using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace soulful
{
    // this should be an abstract class or interface since I want a random generator and also a recorder.
    // rng version has random spawn instructions, other one takes input as arg to produce?
    // share the generate beatmap- one is reading input constantly or something, create whole beatmap once that's an object
    public class BeatmapGenerator
    {
        private SpawnInstruction RandomSpawnInstruction(float[] chances, float beat)
        {
            if (chances.Length != 4)
            {
                throw new ArgumentException("spawn instruction chances must be an array of length 4");
            }

            chances.Shuffle();
            SpawnInstruction instruction = new SpawnInstruction();

            if (chances[0] > RNG.rng.NextDouble())
            {
                instruction.up = true;
            }
            if (chances[1] > RNG.rng.NextDouble())
            {
                instruction.down = true;
            }
            if (chances[2] > RNG.rng.NextDouble())
            {
                instruction.left = true;
            }
            if (chances[3] > RNG.rng.NextDouble())
            {
                instruction.right = true;
            }

            instruction.targetBeat = beat;

            return instruction;
        }

        public List<SpawnInstruction> GenerateBeatmap(float _beats)
        {
            int beats = (int)_beats;
            float[] difficulty = new float[] { 1.0f, 0f, 0f, 0f };
            List<SpawnInstruction> instructions = new List<SpawnInstruction>();

            for (int i = 0; i < beats; i++)
            {
                instructions.Add(RandomSpawnInstruction(difficulty, (float)i + 1));
            }

            return instructions;
        }
    }
}
