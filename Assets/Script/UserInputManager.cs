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
            
            //read stored value
            _input.text = GetVal();
            
            //add listener
            var se = new InputField.SubmitEvent();
            se.AddListener(SetVal);
            _input.onEndEdit = se;
            //or simply use the line below, 
            //input.onEndEdit.AddListener(SubmitName);  // This also works
        }

        private void SetVal(string arg0)
        {
            Debug.Log("Captured Value of [" +_input.name + "] as:"+ arg0);
            PREFS.SetPref(_input.name,arg0);
        }

        private string GetVal()
        {
            string val = PREFS.getPrefString(_input.name);
            Debug.Log("Read Value of [" +_input.name + "] as:"+ val);
            return (val);
        }
    }
}