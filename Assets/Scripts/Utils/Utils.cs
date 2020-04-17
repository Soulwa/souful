using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using SFB;

namespace soulful.Utils
{
    public class BeatmapSerializer
    {
        public static void SaveBeatmap(Beatmap beatmap, string path)
        {
            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = File.Create(path))
            {
                bf.Serialize(fs, beatmap);
            }
        }

        public static Beatmap LoadBeatmap(string path)
        {
            BinaryFormatter bf = new BinaryFormatter();
            Debug.Log(path);

            using (FileStream fs = File.Open(path, FileMode.OpenOrCreate))
            {
                try
                {
                    Beatmap bm = (Beatmap)bf.Deserialize(fs);
                    return bm;
                }
                catch (SerializationException e)
                {
                    Debug.LogException(e);
                    return null;
                }
            }
        }
    }

    public class BeatmapCompressor
    {
        public static void CompressBeatmapWithMusic(Beatmap beatmap, string beatmapPath)
        {
            string absoluteMusicPath = beatmap.songInfo.pathToMusic;
            string beatmapDirectoryPath = Path.GetDirectoryName(beatmapPath);           // beatmap directory
            string beatmapFileName = Path.GetFileName(beatmapPath);                     // name of beatmap file
            string zipDirectoryName = Path.GetFileNameWithoutExtension(beatmapPath);    // name of directory to be zipped

            // make a new directory, we're gonna zip this in a sec
            string zipDirectoryPath = Directory.CreateDirectory(Path.Combine(beatmapDirectoryPath, zipDirectoryName)).FullName;

            // create the path for our zipped directory- beatmap directory path + {name of beatmap}.zip
            string zippedPath = Path.Combine(beatmapDirectoryPath, Path.ChangeExtension(beatmapFileName, ".zip"));

            // add the music to the directory
            File.Copy(absoluteMusicPath, Path.Combine(zipDirectoryPath, Path.GetFileName(absoluteMusicPath)));
            Debug.Log("copied " + absoluteMusicPath + " to " + Path.Combine(zipDirectoryPath, Path.GetFileName(absoluteMusicPath)));

            // change the beatmap's music path to a relative path to reflect the new music
            beatmap.songInfo.pathToMusic = Path.Combine(zipDirectoryName, Path.GetFileName(absoluteMusicPath));

            // write the beatmap to a file
            BeatmapSerializer.SaveBeatmap(beatmap, Path.Combine(zipDirectoryPath, Path.GetFileName(beatmapPath)));

            // now we can zip! nice!
            ZipFile.CreateFromDirectory(zipDirectoryPath, zippedPath);

            // finally let's remove the directory we just made
            Directory.Delete(zipDirectoryPath, true);

        }

        // returns path to unzipped directory
        public static string UnzipBeatmap(string path)
        {
            string newPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));

            // beatmap already loaded?
            if (!Directory.Exists(newPath))
            {
                ZipFile.ExtractToDirectory(path, newPath);
            }

            return newPath;
        }

        public static void DeleteTempBeatmap(string path)
        {
            Directory.Delete(path, true);
        }

    }

    public class SongLoader
    {
        // hacky as fuck
        public async static Task<AudioClip> LoadAudioClipFromMusic(string path, AudioType audioType)
        {
            AudioClip clip = null;

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(Path.Combine("file:///", path), audioType))
            {
                www.SendWebRequest();

                try
                {
                    while (!www.isDone)
                    {
                        await Task.Delay(5);
                    }

                    if (www.isHttpError || www.isNetworkError)
                    {
                        Debug.LogError(www.error);
                        return null;
                    }
                    else
                    {
                        clip = DownloadHandlerAudioClip.GetContent(www);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return null;
                }
            }

            return clip;
        }
    }

    public class FileBrowser
    {
        public static string GetBeatmapPathFromFileDialog()
        {
            string[] paths = StandaloneFileBrowser.OpenFilePanel("get beatmaps", SoulfulSettings.beatmapDataPath, "zip", false);

            if (paths.Length == 0)
            {
                Debug.LogError("no beatmap selected");
                return null;
            }
            else
            {
                string pathToBeatmap = paths[0];
                return pathToBeatmap;
            }
        }

        public static string GetAudioPathFromFileDialog()
        {
            string[] paths = StandaloneFileBrowser.OpenFilePanel("get songs", System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                new ExtensionFilter[] { new ExtensionFilter("audio", "ogg", "wav") }, false);

            if (paths.Length == 0)
            {
                Debug.LogError("no song selected");
                return null;
            }
            else
            {
                return paths[0];
            }
        }
    }
}