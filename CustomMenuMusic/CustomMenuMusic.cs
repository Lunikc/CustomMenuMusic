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

        public void Awake()
        {
            DontDestroyOnLoad(this);

            //SceneManager.sceneLoaded += sceneLoaded;
            SceneManager.activeSceneChanged += new UnityEngine.Events.UnityAction<Scene, Scene>(this.SceneManager_activeSceneChanged);

            if (!Directory.Exists("CustomMenuSongs"))
            {
                Directory.CreateDirectory("CustomMenuSongs");
            }

            UnityEngine.Random.InitState(System.Environment.TickCount);
            GetSongsList();
            
        }

        public void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            printToLog(arg1.name);
            if (arg1.name == "MenuCore")
            {
                if (!_previewPlayer == Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First())
                {
                    _previewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First();
                    GetNewSong();
                }
            }
        }

        private void printToLog(string str)  //Prints a string to the log. Quite explicit
        {
            Console.WriteLine("[CustomMenuMusic] :" + str);
        }  

        private void GetSongsList() // Initializes the song list
        {
            if (CheckOptions())
            {
                AllSongsfilepaths = GetAllCustomMenuSongs();
            }
            else
            {
                AllSongsfilepaths = GetAllCustomSongs();
            }

            printToLog("Found " + AllSongsfilepaths.Length + " custom menu songs");

            ShuffleSongs();
          
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

            printToLog("CustomMenuSongs files found " + filePaths.Length);

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
                Console.WriteLine("[CustomMenuMusic] " + excpt);
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

        IEnumerator LoadAudioClip()  //Load the song into the preview player
        {

            printToLog("Loading file @ " + musicPath);
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
                    printToLog("No audio found!");
                }
            }
            catch (Exception e)
            {
                printToLog("Can't load audio! Exception: " + e);
            }
             

            if (_previewPlayer != null && _menuMusic != null)
            {
                printToLog("Applying custom menu music...");
                _previewPlayer.SetPrivateField("_defaultAudioClip", _menuMusic);
                _previewPlayer.CrossfadeToDefault();
                
            }
        }

    }
}
