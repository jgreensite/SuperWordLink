using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour {

	public GameObject lobbyLineItemPrefab;
	public List<LobbyLineItem> LobbyLineItems = new List<LobbyLineItem>();

	// Use this for initialization
	void Start () {

		Client client = FindObjectOfType<Client> ();
		//Create each LineItem and them to the List of LineItems
		foreach (GameClient gc in client.players)
		{
			LobbyLineItem line = Instantiate(lobbyLineItemPrefab).GetComponent<LobbyLineItem>();
			LobbyLineItems.Add(line);
			line.transform.SetParent(transform,false);
			line.SetLobbyLineItem(gc);
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
