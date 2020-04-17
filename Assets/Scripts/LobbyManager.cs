using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using soulful.Utils;

namespace soulful
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField]
        private LobbyUI lobbyUI;
        [SerializeField]
        private TextMeshProUGUI songName;
        [SerializeField]
        private PopupUI songNamePopup;

        [SerializeField]
        private AudioSource audioSource;
        private AudioClip selectedMusic;

        // might come up with a better idea for how to create these things
        [SerializeField]
        private Metronome metronomePrefab = null;
        [SerializeField]
        private NoteSpawner noteSpawnerPrefab = null;
        [SerializeField]
        private Conductor conductorPrefab = null;

        [SerializeField]
        private Recorder recorder = null;
        private Conductor currentConductor = null;
        private Metronome currentMetronome = null;
        private NoteSpawner currentNoteSpawner = null;
        
        private SongInfo selectedSong;
        private Beatmap selectedBeatmap;
        private string selectedBeatmapPath;

        private bool inGame;
        private bool recording;

        private void Start()
        {
            lobbyUI.beatmapLoaded += LobbyUI_OnBeatmapLoaded;
            lobbyUI.songLoaded += LobbyUI_OnSongLoaded;
        }

        private void WipeCurrentlyLoaded()
        {
            selectedBeatmap = null;
            selectedBeatmapPath = null;
            selectedSong = null;
            selectedMusic = null;
        }

        private void WipeSongLoaded()
        {
            selectedSong = null;
        }

        private void WipeBeatmapLoaded()
        {
            selectedBeatmap = null;
            selectedBeatmapPath = null;
        }

        private void LobbyUI_OnBeatmapLoaded(object sender, BeatmapLoadedArgs e)
        {
            if (e.beatmap != null && e.clip != null && e.pathToBeatmap != null)
            {
                WipeCurrentlyLoaded();
                selectedBeatmap = e.beatmap;
                selectedMusic = e.clip;
                selectedBeatmapPath = e.pathToBeatmap;
                Debug.Log(selectedMusic == null);
                Debug.Log("beatmap loaded");
            }
        }

        private void LobbyUI_OnSongLoaded(object sender, SongLoadedArgs e)
        {
            if (e.song != null && e.clip != null)
            {
                WipeCurrentlyLoaded();
                selectedSong = e.song;
                selectedMusic = e.clip;
                Debug.Log("song loaded");
            }
        }

        private void Update()
        {
            // rn a little messy bc everything depends on one other object to create it- could turn note spawn into request, but metronome is associated w note creation unless event is "global"
            // need better classes to help instantiate beatsynced & metronome
            if (Input.GetKeyDown(KeyCode.Space) && !inGame && !recording)
            {
                Debug.Log("started playing");

                // copy the selected beatmap so not modified somehow? need to clarify on c# class passde by value
                Beatmap currentBeatmap = selectedBeatmap;
                if (currentBeatmap == null || selectedBeatmapPath == null)
                {
                    lobbyUI.FlashMessage("no beatmap selected!");
                }
                else
                {
                    inGame = true;

                    audioSource.clip = selectedMusic;

                    lobbyUI.Hide();

                    currentMetronome = Instantiate(metronomePrefab);
                    currentMetronome.Init(audioSource, selectedBeatmap.songInfo.bpm);

                    currentNoteSpawner = Instantiate(noteSpawnerPrefab);
                    currentNoteSpawner.Init(currentMetronome);

                    currentConductor = Instantiate(conductorPrefab);
                    currentConductor.Init(currentNoteSpawner, currentBeatmap);

                    currentMetronome.currentBeat += currentConductor.OnCurrentBeat;
                    currentMetronome.songEnded += currentConductor.Metronome_OnSongOver;
                    currentMetronome.songEnded += Metronome_OnCloseSong;
                }
            }
            else if (Input.GetKeyDown(KeyCode.R) && !recording && !inGame)
            {
                Debug.Log("started recording");

                int bpm = lobbyUI.TryGetBPM();
                if (bpm < 1)
                {
                    lobbyUI.FlashMessage("invalid bpm");
                }
                else if (selectedSong == null || selectedMusic == null)
                {
                    lobbyUI.FlashMessage("no song selected");
                }
                else
                {
                    recording = true;
                    lobbyUI.Hide();

                    audioSource.clip = selectedMusic;

                    Beatmap newBeatmap = new Beatmap
                    {
                        songInfo = new SongInfo
                        {
                            bpm = bpm,
                            pathToMusic = selectedSong.pathToMusic,
                            name = selectedSong.name,
                        }
                    };

                    recorder = new Recorder(newBeatmap, songNamePopup);
                    recorder.PrepareBeatmap();
                    recorder.finishedRecording += Recorder_OnDoneRecording;

                    currentMetronome = Instantiate(metronomePrefab);
                    currentMetronome.Init(audioSource, newBeatmap.songInfo.bpm);

                    currentMetronome.currentBeat += recorder.OnCurrentBeat;
                    currentMetronome.songEnded += Metronome_OnCloseSong;
                    currentMetronome.songEnded += recorder.Metronome_OnSongOver;
                }
            }
        }

        private void Metronome_OnCloseSong(object sender, EventArgs e)
        {
            Debug.Log("ending song state");
            lobbyUI.Show(); // this should be a callback in the UI prob
            inGame = false;
            if (selectedBeatmapPath != null)
            {
                Debug.Log(selectedBeatmapPath);
                BeatmapCompressor.DeleteTempBeatmap(selectedBeatmapPath);
            }

            if (currentConductor != null)
            {
                currentMetronome.currentBeat -= currentConductor.OnCurrentBeat;
                Destroy(currentConductor.gameObject);
                currentConductor = null;
            }
            if (currentNoteSpawner != null)
            {
                Destroy(currentNoteSpawner.gameObject);
                currentNoteSpawner = null;
            }
            if (recorder != null)
            {
                currentMetronome.currentBeat -= recorder.OnCurrentBeat;
                currentMetronome.songEnded -= recorder.Metronome_OnSongOver;
            }

            currentMetronome.songEnded -= Metronome_OnCloseSong;
            Destroy(currentMetronome.gameObject);
        }

        private void Recorder_OnDoneRecording(object sender, EventArgs _)
        {
            recording = false;
            Debug.Log("done recording");
            currentMetronome.currentBeat -= recorder.OnCurrentBeat;
            recorder.finishedRecording -= Recorder_OnDoneRecording;
        }
    }
}
