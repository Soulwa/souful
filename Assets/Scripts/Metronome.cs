using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

    public class Metronome : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private BeatUI beatTextPrefab;
        private int beatCountdown;
        private BeatUI activeBeatText;

        private float currentSongBPM;
        private float currentSecPerBeat;
        private float currentSongPos;
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

            PrestartSong();
        }

        public void Init(AudioSource audioSource, float bpm)
        {
            this.audioSource = audioSource;
            this.currentSongBPM = bpm;

            currentSecPerBeat = 60f / currentSongBPM;
        }

        private void Update()
        {
            if (songState == SongState.PREPLAYING)
            {
                currentPreSongPos = (float)AudioSettings.dspTime - dspPreSongTime;
                currentSongPosInBeats = (-secondsBeforeStart + currentPreSongPos) / currentSecPerBeat;

                if (currentSongPosInBeats > -4f)
                {
                    activeBeatText.SetBeat((int)(currentSongPosInBeats + 5));
                }

                if (currentSongPosInBeats > 0f)
                {
                    currentSongPosInBeats = 0f;
                }

                if (currentSongPosInBeats == 0f)
                {
                    activeBeatText.Clear();
                    StartSong();
                }
            }

            if (songState == SongState.PLAYING && !audioSource.isPlaying)
            {
                songState = SongState.STOPPED;
                CloseSong();
            }

            if (songState == SongState.PLAYING)
            {
                currentSongPos = (float)AudioSettings.dspTime - dspSongTime;
                currentSongPosInBeats = currentSongPos / currentSecPerBeat;
            }

            currentBeat?.Invoke(this, currentSongPosInBeats);
        }

        private void PrestartSong()
        {
            Debug.Log("presong started");
            currentSongPosInBeats = -secondsBeforeStart / currentSecPerBeat; // waiting period
            dspPreSongTime = (float)AudioSettings.dspTime;

            songState = SongState.PREPLAYING;
            songPrestarted?.Invoke(this, EventArgs.Empty);
        }

        private void StartSong()
        {
            Debug.Log("song started!");
            if (audioSource.clip == null)
            {
                throw new MissingReferenceException("No song currently loaded.");
            }

            dspSongTime = (float)AudioSettings.dspTime;
            audioSource.Play();

            songState = SongState.PLAYING;
            songStarted?.Invoke(this, EventArgs.Empty);
        }

        private void CloseSong()
        {
            Debug.Log("song ended");
            currentSongPos = 0;
            currentSongPosInBeats = 0;
            Destroy(activeBeatText);

            songState = SongState.STOPPED;
            songEnded?.Invoke(this, EventArgs.Empty);
            // Destroy(gameObject); not sure if safely invokes before destroying...
        }
    }
}