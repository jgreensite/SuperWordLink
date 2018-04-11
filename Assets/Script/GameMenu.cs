using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour {

	public Button buttonPanZoom;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void BackButton() {
		Debug.Log ("Game HUD Back button pressed");
		GameManager.Instance.RestartAll ();			
	}

	public void PanZoomButton(){
		Debug.Log ("Game HUD Pan/Zoom button pressed");
		GameManager.Instance.PanZoomButton(buttonPanZoom);		
	}
}
