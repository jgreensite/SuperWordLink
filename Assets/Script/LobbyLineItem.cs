using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LobbyLineItem : MonoBehaviour {

	public Text lineItemText;
	public Toggle isCaller;
	public Toggle isRedTeam;
	public Toggle isBlueTeam;

//	Client client;

	// Use this for initialization
	void Start () {
//		client = FindObjectOfType<Client> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetLobbyLineItem(GameClient gc)
	{
		lineItemText.text = gc.name;
		isCaller.isOn = !(gc.isPlayer);
		isRedTeam.isOn = gc.isRedTeam;
		isBlueTeam.isOn = !(gc.isRedTeam);
	}

	private void SetGameClient()
	{
		Client client = FindObjectOfType<Client> ();

		for (int cnt = 0; cnt < client.players.Count; cnt++)
		{
			if (String.Equals (client.players [cnt].name, lineItemText.text))
			{
				client.players [cnt].name = lineItemText.text;
				client.players [cnt].isPlayer = !isCaller.isOn;
				client.players [cnt].isRedTeam = isRedTeam.isOn;
			}
		}
	}

	public void SelectCaller()
	{
		Client client = FindObjectOfType<Client> ();
		Lobby MyLobbyGroup = FindObjectOfType<Lobby> ();

		int redCallerCnt = 0;
		int blueCallerCnt = 0;

		if ((isRedTeam.isOn == true) && (isBlueTeam.isOn == false))
		{
			for (int cnt = 0;cnt < MyLobbyGroup.LobbyLineItems.Count; cnt ++)
			{
				if ((MyLobbyGroup.LobbyLineItems[cnt].isCaller == true) && (MyLobbyGroup.LobbyLineItems[cnt].isRedTeam == true))
				{
					redCallerCnt += 1;
				}
				if (redCallerCnt > 1)
				{
					MyLobbyGroup.LobbyLineItems [cnt].isCaller.isOn = false;
					redCallerCnt --;
				}
			}
		}
		else if ((isRedTeam.isOn == false) && (isBlueTeam.isOn == true))
		{
			for (int cnt = 0;cnt < MyLobbyGroup.LobbyLineItems.Count; cnt ++)
			{
				if ((MyLobbyGroup.LobbyLineItems[cnt].isCaller == true) && (MyLobbyGroup.LobbyLineItems[cnt].isRedTeam == false))
				{
					blueCallerCnt += 1;
				}
				if (blueCallerCnt > 1)
				{
					MyLobbyGroup.LobbyLineItems [cnt].isCaller.isOn = false;
					blueCallerCnt --;
				}
			}
		}
		SetGameClient ();
	}
}
