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

	// Use this for initialization
	void Start () {
		
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
			//TODO - Should be matching on an ID not on name
			if (String.Equals (client.players [cnt].name, lineItemText.text))
			{
				client.players [cnt].name = lineItemText.text;
				client.players [cnt].isPlayer = !isCaller.isOn;
				client.players [cnt].isRedTeam = isRedTeam.isOn;
			}
		}
	}

	public void SelectUIComponent()
	{
		Lobby MyLobbyGroup = FindObjectOfType<Lobby> ();

		int redCallerCnt = 0;
		int blueCallerCnt = 0;
		for (int cnt = 0;cnt < MyLobbyGroup.LobbyLineItems.Count; cnt ++)
		{
			if ((MyLobbyGroup.LobbyLineItems[cnt].isCaller.isOn == true) && (MyLobbyGroup.LobbyLineItems[cnt].isRedTeam.isOn == true))
			{
				redCallerCnt += 1;
			}
			if (redCallerCnt > 1)
			{
				MyLobbyGroup.LobbyLineItems [cnt].isCaller.isOn = false;
				redCallerCnt --;
			}
			if ((MyLobbyGroup.LobbyLineItems[cnt].isCaller.isOn == true) && (MyLobbyGroup.LobbyLineItems[cnt].isRedTeam.isOn == false))
			{
				blueCallerCnt += 1;
			}
			if (blueCallerCnt > 1)
			{
				MyLobbyGroup.LobbyLineItems [cnt].isCaller.isOn = false;
				blueCallerCnt --;
			}
		}
		SetGameClient ();
	}
}
