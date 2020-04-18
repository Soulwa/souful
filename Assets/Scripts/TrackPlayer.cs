using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using soulful.Utils;

namespace soulful
{
    public enum SongState
    {
        NONE,
        PREPLAYING,
        PLAYING,
        STOPPED,
    }

    public interface IBeatSynced
    {
        void OnCurrentBeat(object sender, float currentBeat);
    }

    public class TrackPlayer : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private BeatUI beatTextPrefab;
        private int beatCountdown;
        private BeatUI activeBeatText;

        [SerializeField]
        private PopupUI nameSongPopup;
        private PopupUI activeNameSongPopup;

        private Beatmap currentBuffer;
        private int noteCounter = 0;

        private Beatmap savedBuffer;
        private float savedBeat = 0;
        private float rewindTimeInSec = 0;
        private int savedNotes;

        public event EventHandler finishedRecording;

        private float currentSongBPM;
        private float currentSecPerBeat;
        private float currentSongPos;
        [SerializeField]
        private float currentSongPosInBeats;
        private float dspSongTime;

        private float secondsBeforeStart;
        private float dspPreSongTime;
        private float currentPreSongPos;

        private SongState songState;

        public event EventHandler<float> currentBeat;
        public event EventHandler songStarted;
        public event EventHandler songEnded;
        public event EventHandler songPrestarted;

        private void Awake()
        {
            secondsBeforeStart = SoulfulSettings.SecondsBeforeSongStart;
        }

        private void Start()
        {
            audioSource = FindObjectOfType<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("audio is null!");
            }
            activeBeatText = Instantiate(beatTextPrefab);
        }

        public void PrepareBeatmap()
        {
            currentBuffer.Initialize();
            noteCounter = 0;
            Debug.Log("beatmap ready");
        }

        public void RecordToBeatmap(float currentBeat, bool up, bool down, bool left, bool right)
        {
            if (currentBeat < 0)
            {
                return;
            }
            if (!up && !down && !left && !right)
            {
                return;
            }

            SpawnInstruction currentInstruction = new SpawnInstruction();

            currentInstruction.up = up;
            currentInstruction.down = down;
            currentInstruction.left = left;
            currentInstruction.right = right;

            currentInstruction.targetBeat = currentBeat;

            if (up)
            {
                noteCounter++;
            }
            if (down)
            {
                noteCounter++;
            }
            if (left)
            {
                noteCounter++;
            }
            if (right)
            {
                noteCounter++;
            }

            currentBuffer.instructions.Add(currentInstruction);
            currentBuffer.notes = noteCounter;
        }

        private void SaveRecording(string beatmapPath, string fileName)
        {
            Debug.Log("saving beatmap");
            string fullPath = Path.Combine(beatmapPath, fileName);
            BeatmapCompressor.CompressBeatmapWithMusic(currentBuffer, fullPath);
        }

        public void FinishRecordingAndSave(string beatmapPath, string fileName)
        {
            SaveRecording(beatmapPath, fileName + ".fnk");
            finishedRecording?.Invoke(this, EventArgs.Empty);
        }

        public void PopupUI_OnNameChosen(object sender, string songName)
        {
            Debug.LogWarning("name chosen fired");
            activeNameSongPopup.nameChosen -= PopupUI_OnNameChosen;
            activeNameSongPopup.Hide();
            Destroy(activeNameSongPopup.gameObject);

            currentBuffer.songInfo.name = songName;
            FinishRecordingAndSave(SoulfulSettings.beatmapDataPath, songName);
        }

        public void Init(AudioSource audioSource, float bpm, bool recording, Beatmap bufferData = null)
        {
            this.audioSource = audioSource;
            this.currentSongBPM = bpm;

            currentSecPerBeat = 60f / currentSongBPM;
            if (recording)
            {
                this.currentBuffer = bufferData;
                PrepareBeatmap();
                activeNameSongPopup = Instantiate(nameSongPopup);
                activeNameSongPopup.Hide();
                activeNameSongPopup.nameChosen += PopupUI_OnNameChosen;
            }
            PrestartSong();
        }

        private void Update()
        {
            if (songState == SongState.PREPLAYING)
            {
                //calculate time into the song
                currentPreSongPos = (float)AudioSettings.dspTime - dspPreSongTime;
                currentSongPosInBeats = (-secondsBeforeStart + currentPreSongPos) / currentSecPerBeat;

                // flash the countdown for starting the song
                if (currentSongPosInBeats > -4f)
                {
                    activeBeatText.SetBeat((int)(currentSongPosInBeats + 5));
                }

                // align the beats properly with the song... might be dangerous
                if (currentSongPosInBeats >= 0f)
                {
                    currentSongPosInBeats = 0f;
                    activeBeatText.Clear();
                    StartSong();
                }
            }

            // close the song if it's all done playing
            if (songState == SongState.PLAYING && !audioSource.isPlaying)
            {
                songState = SongState.STOPPED;
                CloseSong();
            }

            if (songState == SongState.PLAYING)
            {
                // update the position of the song
                currentSongPos = (float)AudioSettings.dspTime - dspSongTime - rewindTimeInSec;
                currentSongPosInBeats = currentSongPos / currentSecPerBeat;

                // if we have a beatmap loaded to record, record
                if (currentBuffer != null)
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        savedBuffer = currentBuffer.DeepCopy();
                        savedBeat = currentSongPosInBeats;
                        savedNotes = noteCounter;
                    }
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        if (savedBeat == 0f)
                        {
                            // no
                        }
                        else
                        {
                            currentBuffer = savedBuffer.DeepCopy();
                            noteCounter = savedNotes;
                            rewindTimeInSec += (currentSongPosInBeats - savedBeat) * currentSecPerBeat;
                            currentSongPosInBeats = savedBeat;
                            currentSongPos = (float)AudioSettings.dspTime - dspSongTime - rewindTimeInSec;
                            audioSource.time = currentSongPos;
                        }
                    }

                    RecordToBeatmap(currentSongPosInBeats, Input.GetButtonDown("Up"), Input.GetButtonDown("Down"), Input.GetButtonDown("Left"), Input.GetButtonDown("Right"));
                }
            }

            currentBeat?.Invoke(this, currentSongPosInBeats);
        }

        // preplays currently loaded audio track
        private void PrestartSong()
        {
            currentSongPosInBeats = -secondsBeforeStart / currentSecPerBeat; // waiting period
            dspPreSongTime = (float)AudioSettings.dspTime;

            songState = SongState.PREPLAYING;
            songPrestarted?.Invoke(this, EventArgs.Empty);
        }

        //plays currently loaded audio track
        private void StartSong()
        {
            if (audioSource.clip == null)
            {
                throw new MissingReferenceException("No song currently loaded.");
            }

            dspSongTime = (float)AudioSettings.dspTime;
            audioSource.Play();

            songState = SongState.PLAYING;
            songStarted?.Invoke(this, EventArgs.Empty);
        }

        public void RewindSong(int measures, int padding)
        {

        }

        // finishes the current song playing, cleanup
        private void CloseSong()
        {
            Debug.Log("song ended");
            currentSongPos = 0;
            currentSongPosInBeats = 0;

            songState = SongState.STOPPED;
            audioSource.time = 0;
            songEnded?.Invoke(this, EventArgs.Empty);
            Destroy(activeBeatText.gameObject);

            if (currentBuffer != null)
            {
                activeNameSongPopup.Show();
            }
        }
    }
}