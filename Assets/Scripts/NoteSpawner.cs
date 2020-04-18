using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace soulful
{
    public class NoteSpawner : MonoBehaviour
    {
        [SerializeField]
        private Note notePrefab;

        private TrackPlayer trackPlayer;

        public void Init(TrackPlayer metronome)
        {
            this.trackPlayer = metronome;
        }

        public Note SpawnNote()
        {
            Note note = Instantiate(notePrefab);
            trackPlayer.currentBeat += note.OnCurrentBeat;
            return note;
        }

        public void DespawnNote(Note note)
        {
            trackPlayer.currentBeat -= note.OnCurrentBeat;
            Destroy(note.gameObject);
        }
    }
}

