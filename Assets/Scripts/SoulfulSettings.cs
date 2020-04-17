using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace soulful
{
    public class SoulfulSettings : MonoBehaviour
    {
        private static SoulfulSettings instance;

        [SerializeField]
        private float beatsToSpawnInAdvance;
        [SerializeField]
        private float beatOffset;
        [SerializeField]
        private float secondsBeforeSongStart;

        public static string beatmapDataPath;
        public static float BeatOffset { get { return instance.beatsToSpawnInAdvance; } }
        public static float BeatsToSpawnInAdvance { get { return instance.beatsToSpawnInAdvance; } }
        public static float SecondsBeforeSongStart { get { return instance.secondsBeforeSongStart; } }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            beatmapDataPath = Path.Combine(Application.dataPath, "beatmaps");
            Debug.Log(beatmapDataPath);
            // maybe shouldn't do this here... oh well
            if (!Directory.Exists(beatmapDataPath))
            {
                Directory.CreateDirectory(beatmapDataPath);
            }
        }
    }
}