using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby : MonoBehaviour {

	public GameObject lobbyLineItemPrefab;

	// Use this for initialization
	void Start () {
	
		//Create the LineItems
		LobbyLineItem l = Instantiate(lobbyLineItemPrefab).GetComponent<LobbyLineItem>();
		//TODO - Remove PORT hardcoding
		GameObject.Find ("PlayerName").GetComponent<Text> ().text ="bob";
	}

	// Update is called once per frame
	void Update () {
		
	}
}
