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
    }
}