using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using CustomMenuMusic.Misc;
using Logger = CustomMenuMusic.Misc.Logger;


namespace CustomMenuMusic
{
    class CustomMenuMusic : MonoBehaviour
    {
        public static CustomMenuMusic instance;

        AudioClip _menuMusic;
        SongPreviewPlayer _previewPlayer = new SongPreviewPlayer();
        int CurrentSong;
        string musicPath;
        string optionName = "UseCustomMenuSongs";
        string[] AllSongsfilepaths = new string[0];

        public static void OnLoad()
        {
            if(instance == null)
            {
                instance = new GameObject("CustomMenuMusic").AddComponent<CustomMenuMusic>();
            }
        }

        public void Awake() // Get songs list on awake
        {
            DontDestroyOnLoad(this);

            SceneManager.activeSceneChanged += new UnityEngine.Events.UnityAction<Scene, Scene>(this.SceneManager_activeSceneChanged);

            if (!Directory.Exists("CustomMenuSongs"))
            {
                Directory.CreateDirectory("CustomMenuSongs");
            }

            UnityEngine.Random.InitState(System.Environment.TickCount);
            GetSongsList();
            
        }

        public void SceneManager_activeSceneChanged(Scene arg0, Scene arg1) // On menu opened, load the song
        {
            if (arg1.name == "MenuCore")
            {
                if (!_previewPlayer == Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First())
                {
                    _previewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First();
                    GetNewSong();
                }
            }
        }

        private void GetSongsList() // Initializes the song list
        {
            if (CheckOptions())
            {
                AllSongsfilepaths = GetAllCustomMenuSongs();
                if (AllSongsfilepaths.Length == 0)
                    AllSongsfilepaths = GetAllCustomSongs();
            }
            else
            {
                AllSongsfilepaths = GetAllCustomSongs();
            }

            Logger.Log("Found " + AllSongsfilepaths.Length + " songs.");

            if (AllSongsfilepaths.Length == 0); // Add despacito here if you dare

        }

        private bool CheckOptions()  //Checks ModPrefs for user options
        {
            return IllusionPlugin.ModPrefs.GetBool("CustomMenuMusic", optionName, true, true); ;
        }

        private string[] GetAllCustomMenuSongs()  //Get .ogg in CustomMenuSongs directory
        {
            if (!Directory.Exists("CustomMenuSongs"))
            {
                Directory.CreateDirectory("CustomMenuSongs");
            }

            string[] filePaths = Directory.GetFiles("CustomMenuSongs", "*.ogg");

            Logger.Log("CustomMenuSongs files found " + filePaths.Length);

            if (filePaths.Length == 0)
            {
                filePaths = GetAllCustomSongs();
            }

            return filePaths;
        }
  
        private string[] GetAllCustomSongs()  //Get .ogg in CustomSongs directory
        {
            string[] filePaths = DirSearch("CustomSongs").ToArray();

            return filePaths;
        }

        private List<String> DirSearch(string sDir)  //Search Directories for .ogg with depth of 2
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
                Logger.Log("[CustomMenuMusic] " + excpt);
            }

            return files;
        }

        private void GetNewSong() // Gets the first song when loading the menu
        {
            ShuffleSongs();
            CurrentSong = 0;
            musicPath = AllSongsfilepaths[CurrentSong];
            StartCoroutine(LoadAudioClip());
        }

        private void ShuffleSongs() // Shuffle the songs list for... uhh... randomized fun?
        {
            for (int i = 0; i < AllSongsfilepaths.Length; i++)
            {
                string temp = AllSongsfilepaths[i];
                int randomIndex = UnityEngine.Random.Range(i, AllSongsfilepaths.Length);
                AllSongsfilepaths[i] = AllSongsfilepaths[randomIndex];
                AllSongsfilepaths[randomIndex] = temp;
            }
        }

        private void ChangeCurrentSongIndex(int value) // Increment or decrement the CurrentSong value
        {
            CurrentSong += value;
            if (CurrentSong < 0)
                CurrentSong = AllSongsfilepaths.Length - 1;
            else if (CurrentSong > AllSongsfilepaths.Length - 1)
                CurrentSong = 0;
        }

        private void PlayNextSong() // Plays the next song in the list
        {
            ChangeCurrentSongIndex(1);
            musicPath = AllSongsfilepaths[CurrentSong];
            StartCoroutine(LoadAudioClip());
        }

        private void PlayPreviousSong() // Plays the previous song in the list
        {
            ChangeCurrentSongIndex(-1);
            musicPath = AllSongsfilepaths[CurrentSong];
            StartCoroutine(LoadAudioClip());
        }

        IEnumerator LoadAudioClip()  //Load the song into the preview player
        {
            Logger.Log("Loading file @ " + musicPath);

            UnityWebRequest song = UnityWebRequestMultimedia.GetAudioClip($"{Environment.CurrentDirectory}\\{musicPath}", AudioType.OGGVORBIS);

            yield return song;
            try
            {
                _menuMusic = DownloadHandlerAudioClip.GetContent(song);

                if (_menuMusic != null)
                    _menuMusic.name = Path.GetFileName(musicPath);
                
                else
                    Logger.Log("No audio found!");

            }
            catch (Exception e)
            {
                Logger.Log("Can't load audio! Exception: " + e);
            }
             

            if (_previewPlayer != null && _menuMusic != null)
            {
                Logger.Log("Starting custom menu music...");
                _previewPlayer.SetField("_defaultAudioClip", _menuMusic);
                _previewPlayer.CrossfadeToDefault();
            }
        }
    }
}
