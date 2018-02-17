using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour {

	public GameObject lobbyLineItemPrefab;

	// Use this for initialization
	void Start () {
	
		//Create the LineItems
		LobbyLineItem line = Instantiate(lobbyLineItemPrefab).GetComponent<LobbyLineItem>();
		//TODO - Remove PORT hardcoding
		//GameObject.Find ("PlayerName").GetComponent<Text>().text ="bob";
		line.transform.SetParent(transform,false);
		line.SetLobbyLineItemText();
	}

	// Update is called once per frame
	void Update () {
		
	}
}
