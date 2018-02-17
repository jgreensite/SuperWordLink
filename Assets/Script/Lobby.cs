using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour {

	public GameObject lobbyLineItemPrefab;

	// Use this for initialization
	void Start () {

		Client client = FindObjectOfType<Client> ();
		//Create the LineItems
		foreach (GameClient gc in client.players)
		{
			LobbyLineItem line = Instantiate(lobbyLineItemPrefab).GetComponent<LobbyLineItem>();
			//GameObject.Find ("PlayerName").GetComponent<Text>().text ="bob";
			line.transform.SetParent(transform,false);
			line.SetLobbyLineItem(gc);
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
