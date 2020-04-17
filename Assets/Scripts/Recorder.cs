using soulful.Utils;
using System;
using System.IO;
using UnityEngine;

namespace soulful
{
    public class Recorder : IBeatSynced
    {
        [SerializeField]
        private PopupUI nameSongPopup;

        private volatile Beatmap currentBuffer;
        private int noteCounter = 0;

        public event EventHandler finishedRecording;

        public Recorder (Beatmap beatmap, PopupUI nameSongPopup)
        {
            currentBuffer = beatmap;
            this.nameSongPopup = nameSongPopup;
            nameSongPopup.nameChosen += PopupUI_OnNameChosen;
        }

        public void PrepareBeatmap()
        {
            currentBuffer.Initialize();
            noteCounter = 0;
            Debug.Log("beatmap ready");
        }

        public void OnCurrentBeat(object sender, float currentBeat)
        {
            if (currentBeat >= 0)
            {
                RecordToBeatmap(currentBeat, Input.GetButtonDown("Up"), Input.GetButtonDown("Down"), Input.GetButtonDown("Left"), Input.GetButtonDown("Right"));
            }
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
            Debug.Log("beatmap saved");
        }

        public void FinishRecordingAndSave(string beatmapPath, string fileName)
        {
            Debug.Log("abs song path: " + currentBuffer.songInfo.pathToMusic);
            SaveRecording(beatmapPath, fileName + ".fnk");
            finishedRecording?.Invoke(this, EventArgs.Empty);
        }

        public void Metronome_OnSongOver(object sender, EventArgs _)
        {
            nameSongPopup.Show();
        }

        public void PopupUI_OnNameChosen(object sender, string songName)
        {
            Debug.LogWarning("name chosen fired");
            nameSongPopup.nameChosen -= PopupUI_OnNameChosen;
            nameSongPopup.Hide();

            currentBuffer.songInfo.name = songName;
            FinishRecordingAndSave(SoulfulSettings.beatmapDataPath, songName);
        }
    }
}