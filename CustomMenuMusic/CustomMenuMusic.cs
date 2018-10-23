using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DespacitoPlugin
{
    class CustomMenuMusic : MonoBehaviour
    {
        public static CustomMenuMusic instance;

        AudioClip _menuMusic;
        SongPreviewPlayer _previewPlayer;
        string musicPath;
        string[] filepaths = new string[0];

        public static void OnLoad()
        {
            if(instance == null)
            {
                instance = new GameObject("CustomMenuMusic").AddComponent<CustomMenuMusic>();
            }
        }

        private string[] GetAllSongsPath()
        {
            if (!Directory.Exists("CustomMenuSongs"))
            {
                Directory.CreateDirectory("CustomMenuSongs");
            }

            string[] filePaths = Directory.GetFiles("CustomMenuSongs", "*.ogg");

            return filePaths;
        }

        private bool CheckOptions()
        {
            return IllusionPlugin.ModPrefs.GetBool("CustomMenuMusic", "UserPrefSongs", false, true); ;
        }

        private string[] GetAllCustomSongs()
        {
            string[] filePaths = DirSearch("CustomSongs").ToArray();

            return filePaths;
        }


        private List<String> DirSearch(string sDir)
        {
            List<String> files = new List<String>();
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(Directory.GetFiles(d, "*.ogg"));

                    foreach (string f in Directory.GetDirectories(d))
                    {
                        files.AddRange(Directory.GetFiles(f, "*.ogg"));
                    }
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine("[CustomMenuMusic] " + excpt);
            }

            return files;
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
                        
            SceneManager.sceneLoaded += sceneLoaded;

        }


        // Token: 0x06000004 RID: 4 RVA: 0x00002082 File Offset: 0x00000282
        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene arg1)
        {

            StartCoroutine(LoadAudioClip());

        }

        private void sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name == "Menu")
            {
                _previewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First();
            }
            StartCoroutine(LoadAudioClip());

        }
        private void GetSongsList()
        {
            if (CheckOptions())
            {
                filepaths = GetAllSongsPath();
            }
            else
            {
                filepaths = GetAllCustomSongs();
            }

            Console.WriteLine("[CustomMenuMusic]" + "found " + filepaths.Length + " songs");

        }

        private void GetNewSong()
        {
            if (filepaths.Length == 0)
                GetSongsList();

            var a = UnityEngine.Random.Range(0, filepaths.Length);
            musicPath = filepaths[a];

        }
      
        IEnumerator LoadAudioClip()
        {

            GetNewSong();

            Console.WriteLine("[CustomMenuMusic] Loading file @ " + musicPath);
            WWW data = new WWW(Environment.CurrentDirectory + "\\" + musicPath);
            yield return data;
            try
            {
                _menuMusic = data.GetAudioClipCompressed(false, AudioType.OGGVORBIS) as AudioClip;
                if (_menuMusic != null)
                {
                    _menuMusic.name = Path.GetFileName(musicPath);
                }
                else
                {
                    Console.WriteLine("[CustomMenuMusic] No audio found!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[CustomMenuMusic] Can't load audio! Exception: " + e);
            }
             

            if (_previewPlayer != null && _menuMusic != null)
            {
                Console.WriteLine("[CustomMenuMusic] Applying custom menu music...");
                _previewPlayer.SetPrivateField("_defaultAudioClip", _menuMusic);
                _previewPlayer.CrossfadeToDefault();
                
            }
        }
    }
}
