﻿using System;
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
        string optionName = "UseCustomMenuSongs";
        string[] filepaths = new string[0];

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

            SceneManager.sceneLoaded += sceneLoaded;

            GetSongsList();
        }


        private void sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name == "Menu")
            {
                _previewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First();
                StartCoroutine(LoadAudioClip());
            }
        }

        private void printToLog(string str)
        {
            Console.WriteLine("[CustomMenuMusic] " + str);
        }

        private void GetSongsList()
        {
            if (CheckOptions())
            {
                filepaths = GetAllCustomSongsPath();
            }
            else
            {
                filepaths = GetAllCustomMenuSongs();
            }

            printToLog("Found " + filepaths.Length + " songs");
            
        }


        private bool CheckOptions()
        {
            return IllusionPlugin.ModPrefs.GetBool("CustomMenuMusic", optionName, false, true); ;
        }

        private string[] GetAllCustomSongsPath()
        {
            if (!Directory.Exists("CustomMenuSongs"))
            {
                Directory.CreateDirectory("CustomMenuSongs");
            }

            string[] filePaths = Directory.GetFiles("CustomMenuSongs", "*.ogg");

            return filePaths;
        }

        
        private string[] GetAllCustomMenuSongs()
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


        private void GetNewSong()
        {           
            var a = UnityEngine.Random.Range(0, filepaths.Length);
            musicPath = filepaths[a];
        }
      

        IEnumerator LoadAudioClip()
        {

            GetNewSong();

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
