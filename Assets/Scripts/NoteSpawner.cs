using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace soulful
{
    public class NoteSpawner : MonoBehaviour
    {
        [SerializeField]
        private Note notePrefab;

        private Metronome metronome;

        public void Init(Metronome metronome)
        {
            this.metronome = metronome;
        }

        public Note SpawnNote()
        {
            Note note = Instantiate(notePrefab);
            metronome.currentBeat += note.OnCurrentBeat;
            return note;
        }

        public void DespawnNote(Note note)
        {
            metronome.currentBeat -= note.OnCurrentBeat;
            Destroy(note.gameObject);
        }
    }
}

