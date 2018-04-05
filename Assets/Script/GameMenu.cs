using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void BackButton() {
		Debug.Log ("Back button pressed");

		//Call the shutdown script on the server
		Server.Instance.Shutdown ();

		//destroy the objects that were labelled as donotdestroy when we are restarting
		for (var i = GameManager.Instance.goDontDestroyList.Count - 1; i > -1; i--)
		{
			if (GameManager.Instance.goDontDestroyList [i] != null)
			{
				Destroy (GameManager.Instance.goDontDestroyList [i]);
				GameManager.Instance.goDontDestroyList.RemoveAt (i);
			}
		}
			
		SceneManager.LoadScene ("Menu");
	}
}
