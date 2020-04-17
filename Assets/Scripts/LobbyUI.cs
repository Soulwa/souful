using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using soulful.Utils;

namespace soulful
{
    public class BeatmapLoadedArgs : EventArgs
    {
        public Beatmap beatmap;
        public AudioClip clip;
        public string pathToBeatmap;
    }

    public class SongLoadedArgs : EventArgs
    {
        public SongInfo song;
        public AudioClip clip;
    }

    public class LobbyUI : MonoBehaviour
    {
        [SerializeField]
        private Button loadBeatmapButton;
        [SerializeField]
        private Button loadSongButton;
        [SerializeField]
        private Button shareBeatmapButton;
        [SerializeField]
        private TextMeshProUGUI message;
        [SerializeField]
        private TextMeshProUGUI songName;
        [SerializeField]
        private TMP_InputField bpm;
        

        [SerializeField]
        private CanvasGroup lobbyGroup;
        [SerializeField]
        private CanvasGroup gameInstructionsGroup;

        [SerializeField]
        private LobbyManager targetManager;

        public event EventHandler<BeatmapLoadedArgs> beatmapLoaded;
        public event EventHandler<SongLoadedArgs> songLoaded;

        private void Start()
        {
            loadBeatmapButton.onClick.AddListener(LoadBeatmap);
            loadSongButton.onClick.AddListener(LoadSong);
        }

        private async void LoadBeatmap()
        {
            string beatmapDirectoryPath = RetrieveBeatmapDirectory();
            string beatmapPath = Directory.GetFiles(beatmapDirectoryPath, "*.fnk")[0];

            Beatmap bm = BeatmapSerializer.LoadBeatmap(beatmapPath);
            string songPath = Path.Combine(SoulfulSettings.beatmapDataPath, bm.songInfo.pathToMusic);

            if (bm != null)
            {
                AudioClip clip = await GetAudioClipFromSong(songPath);
                BeatmapLoadedArgs e = new BeatmapLoadedArgs
                {
                    beatmap = bm,
                    clip = clip,
                    pathToBeatmap = beatmapDirectoryPath,
                };
                beatmapLoaded(this, e);
                UpdateSongName(bm.songInfo.name);
            }
        }

        private async void LoadSong()
        {
            SongInfo si = RetrieveNewSong();
            AudioClip clip = await GetAudioClipFromSong(si.pathToMusic);
            if (si != null)
            {
                SongLoadedArgs e = new SongLoadedArgs
                {
                    song = si,
                    clip = clip,
                };
                songLoaded(this, e);
                UpdateSongName(si.name);
            }
        }

        private string RetrieveBeatmapDirectory()
        {
            string beatmapDirectoryPath = FileBrowser.GetBeatmapPathFromFileDialog();

            if (beatmapDirectoryPath != null)
            {
                string unzippedBeatmapPath = BeatmapCompressor.UnzipBeatmap(beatmapDirectoryPath);
                return unzippedBeatmapPath;
            }
            else
            {
                FlashMessage("no beatmap loaded");
                return null;
            }
        }

        private SongInfo RetrieveNewSong()
        {
            string pathToAudio = FileBrowser.GetAudioPathFromFileDialog();
            string songName = Path.GetFileNameWithoutExtension(pathToAudio);
            return new SongInfo
            {
                name = songName,
                pathToMusic = pathToAudio
            };
        }

        private async Task<AudioClip> GetAudioClipFromSong(string pathToAudio)
        {
            AudioType audioType = Path.GetExtension(pathToAudio) == ".wav" ? AudioType.WAV : AudioType.OGGVORBIS;

            if (pathToAudio != null)
            {
                AudioClip clip = await SongLoader.LoadAudioClipFromMusic(pathToAudio, audioType);
                if (clip != null)
                {
                    return clip;
                }
                else
                {
                    FlashMessage("error getting audio data. try again");
                    return null;
                }
            }
            else
            {
                FlashMessage("no song selected! try again");
                return null;
            }
        }

        // slightly hacky, better way to determine bpm? eventually implement tapping?
        public int TryGetBPM()
        {
            int bpm;
            try
            {
                bpm = int.Parse(this.bpm.text);
            }
            catch (FormatException e)
            {
                Debug.LogException(e);
                foreach (char c in this.bpm.text)
                {
                    Debug.Log(c);
                }
                return 0;
            }
            if (bpm < 1) return 0;
            return bpm;
        }

        public void FlashMessage(string msg)
        {
            message.text = msg;
        }

        public void UpdateSongName(string songName)
        {
            this.songName.text = songName;
        }

        public void Show()
        {
            lobbyGroup.alpha = 1f;
            lobbyGroup.interactable = true;
            gameInstructionsGroup.alpha = 1f;
            gameInstructionsGroup.interactable = true;
        }

        public void Hide()
        {
            lobbyGroup.alpha = 0f;
            lobbyGroup.interactable = false;
            gameInstructionsGroup.alpha = 0f;
            gameInstructionsGroup.interactable = false;
        }

        public void SetSongName(string name)
        {
            songName.text = name;
        }
    }
}
