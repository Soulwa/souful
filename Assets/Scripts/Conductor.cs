using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace soulful
{
    public enum NoteHit
    {
        MISS,
        BRUH,
        DANG,
        NICE,
        FUNKY,
    }

    public class OnNoteHitArgs : EventArgs
    {
        public NoteHit status;
    }
   
    public class Conductor : MonoBehaviour, IBeatSynced
    {
        // private BeatmapGenerator beatmapGenerator;
        [SerializeField]
        private Beatmap beatmap;
        private int currentInstruction = 0;

        private Queue<Note>[] activeNotes = new Queue<Note>[4];

        [SerializeField]
        private NoteSpawner noteSpawner;
        [SerializeField]
        private float noteOffset;
        private float beatsInAdvance;

        // temporary storage so song stuff is actually updated
        [SerializeField]
        private SongUI songUIPrefab;
        private SongUI activeSongUI;
        private int maxNotes = 0;
        private int noteScore = 0;
        private int funkyScore = 0;
        private int combo = 0;

        [SerializeField]
        private Vector2 noteDestination = Vector2.zero;
        [SerializeField]
        private float dangOffset;
        [SerializeField]
        private float niceOffset;
        [SerializeField]
        private float funkyOffset;

        private EventHandler<EventArgs> onNoteHit;

        private void Awake()
        {
            beatsInAdvance = SoulfulSettings.BeatsToSpawnInAdvance;
            noteOffset = SoulfulSettings.BeatOffset;

            for (int i = 0; i < activeNotes.Length; i++)
            {
                activeNotes[i] = new Queue<Note>();
            }
        }

        private void Start()
        {
            activeSongUI = Instantiate(songUIPrefab);
            maxNotes = beatmap.notes;
            activeSongUI.SetNotesText(0, maxNotes);
            activeSongUI.SetFunkysText(0, maxNotes);
        }

        public void Init(NoteSpawner noteSpawner, Beatmap beatmap)
        {
            this.noteSpawner = noteSpawner;
            this.beatmap = beatmap;
        }

        public void OnCurrentBeat(object sender, float currentBeat)
        {
            if (currentInstruction < beatmap.instructions.Count)
            {
                if (currentBeat >= beatmap.instructions[currentInstruction].targetBeat - beatsInAdvance)
                {
                    Note[] notes = SpawnNotes(beatmap.instructions[currentInstruction], new Color(Mathf.Sin(Time.time), Mathf.Sin(Time.time + Mathf.PI * 2 / 3), Mathf.Sin(Time.time + Mathf.PI * 4 / 3)));
                    foreach (Note note in notes)
                    {
                        activeNotes[(int)note.direction].Enqueue(note);
                    }

                    currentInstruction++;
                }
            }
        }

        private void Update()
        {
            foreach(Queue<Note> q in activeNotes)
            {
                if (q.Count != 0 && !q.Peek().gameObject.activeSelf)
                {
                    Note _n = q.Dequeue();
                    noteSpawner.DespawnNote(_n);
                    Debug.LogError("miss");
                }
            }

            if (Input.GetButtonDown("Up")) 
            {
                RegisterNoteHit(Direction.UP);
            }
            if (Input.GetButtonDown("Down")) 
            {
                RegisterNoteHit(Direction.DOWN);
            }
            if (Input.GetButtonDown("Left"))
            {
                RegisterNoteHit(Direction.LEFT);
            }
            if (Input.GetButtonDown("Right"))
            {
                RegisterNoteHit(Direction.RIGHT);
            }
        }

        private void RegisterNoteHit(Direction dir)
        {
            if (activeNotes[(int)dir].Count != 0)
            {
                Note targetNote = activeNotes[(int)dir].Peek();

                //if (!targetNote.gameObject.activeSelf)
                //{
                //    targetNote = activeNotes[(int)dir].Dequeue();
                //    noteSpawner.DespawnNote(targetNote);
                //}

                if (Vector2.Distance(targetNote.transform.position, noteDestination) < dangOffset)
                {
                    if (Vector2.Distance(targetNote.transform.position, noteDestination) <= funkyOffset)
                    {
                        activeSongUI.CreateNoteHitText("FUNKY", dir);
                        funkyScore++;
                        noteScore++;
                        combo++;
                        activeSongUI.SetNotesText(noteScore, maxNotes);
                        activeSongUI.SetFunkysText(funkyScore, maxNotes);
                        activeSongUI.SetComboText(combo);
                    }
                    else if (Vector2.Distance(targetNote.transform.position, noteDestination) <= niceOffset)
                    {
                        activeSongUI.CreateNoteHitText("noice!", dir);
                        noteScore++;
                        combo++;
                        activeSongUI.SetNotesText(noteScore, maxNotes);
                        activeSongUI.SetComboText(combo);
                    }
                    else if (Vector2.Distance(targetNote.transform.position, noteDestination) <= dangOffset)
                    {
                        activeSongUI.CreateNoteHitText("dang", dir);
                        noteScore++;
                        combo++;
                        activeSongUI.SetNotesText(noteScore, maxNotes);
                        activeSongUI.SetComboText(combo);
                    }

                    targetNote = activeNotes[(int)dir].Dequeue();
                    noteSpawner.DespawnNote(targetNote);
                }
                else
                {
                    activeSongUI.CreateNoteHitText("BRUH!", dir);
                    combo = 0;
                    activeSongUI.SetComboText(combo);
                }
            }
            else
            {
                activeSongUI.CreateNoteHitText("BRUH!", dir);
                combo = 0;
                activeSongUI.SetComboText(combo);
            }
        }

        private Note CreateNote(Vector2 pos, float targetBeat, Direction fromDir, Color color)
        {
            Note note = noteSpawner.SpawnNote();
            note.Init(targetBeat, pos, fromDir, color);
            return note;
        }

        private Note[] SpawnNotes(SpawnInstruction instruction, Color color)
        {
            List<Note> notes = new List<Note>();

            if (instruction.up)
            {
                notes.Add(CreateNote(Vector2.up * noteOffset, instruction.targetBeat, Direction.UP, color));
            }

            if (instruction.down)
            {
                notes.Add(CreateNote(Vector2.down * noteOffset, instruction.targetBeat, Direction.DOWN, color));
            }

            if (instruction.left)
            {
                notes.Add(CreateNote(Vector2.left * noteOffset, instruction.targetBeat, Direction.LEFT, color));
            }

            if (instruction.right)
            {
                notes.Add(CreateNote(Vector2.right * noteOffset, instruction.targetBeat, Direction.RIGHT, color));
            }

            return notes.ToArray();
        }

        public void Metronome_OnSongOver(object sender, EventArgs _)
        {
            Destroy(activeSongUI.gameObject);
            Destroy(gameObject);
        }
    }
}