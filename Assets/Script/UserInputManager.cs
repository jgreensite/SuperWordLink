using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Script
{
    public class UserInputManager : MonoBehaviour
    {
        private InputField _input;
        void Start()
        {
            _input = GetComponent<InputField>();
            var se = new InputField.SubmitEvent();
            se.AddListener(SubmitName);
            _input.onEndEdit = se;
            //or simply use the line below, 
            //input.onEndEdit.AddListener(SubmitName);  // This also works
        }

        private void SubmitName(string arg0)
        {
            Debug.Log("Captured Value of [" +_input.name + "] as:"+ arg0);
            PREFS.SetPref(_input.name,arg0);
        }
    }
}