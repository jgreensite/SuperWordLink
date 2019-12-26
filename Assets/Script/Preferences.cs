using System;
using System.Collections.Generic;
using UnityEngine;

namespace Script
{
    public class PREFS : MonoBehaviour

    {
        public int CSGRIDXDIM = 6;
        public int CSGRIDZDIM = 6;

        //makes class a singleton
        public static PREFS Instance { set; get; }

        // Use this for initialization
        private void Start()
        {
            //needed to make this a singleton
            Instance = this;

            //needed to preserve game objects between scenes
            DontDestroyOnLoad(gameObject);
            GameManager.Instance.goDontDestroyList.Add(gameObject);
            Debug.Log("Added PREFS at position:" + GameManager.Instance.goDontDestroyList.Count +
                      " to donotdestroylist");
        }

        //TODO - update this to an object that is serialized so that it can survive a clear preferences wipe by a user
        public static void SetPref(string key, string val)
        {
            PlayerPrefs.SetString(key, val);
            Debug.Log("Stored preference [" + key + "] as:"+ val);
        }

        public static int getPrefInt(string key)
        {
            return(Int32.Parse(PlayerPrefs.GetString(key, "0")));
        }
        
        public static string getPrefString(string key)
        {
            return(PlayerPrefs.GetString(key, "0"));
        }
    }
}