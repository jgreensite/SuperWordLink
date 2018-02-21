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


	public void SetGameClient()
	{
		Client client = FindObjectOfType<Client> ();

		for (int cnt = 0; cnt < client.players.Count; cnt++)
		{
			if (String.Equals (client.players [cnt].name, lineItemText.text))
			{
				client.players [cnt].name = lineItemText.text;
				client.players [cnt].isPlayer = !isCaller.isOn;
				client.players [cnt].isRedTeam = isRedTeam.isOn;
				client.players [cnt].isRedTeam = !isBlueTeam.isOn;
			}
		}
	}

	public void SelectCaller()
	{
		Client client = FindObjectOfType<Client> ();

		int redCallerCnt = 0;
		int blueCallerCnt = 0;

		if ((isRedTeam.isOn == true) && (isBlueTeam.isOn == false))
		{
			for (int cnt = 0;cnt < client.players.Count; cnt ++)
			{
				if ((client.players[cnt].isPlayer == false) && (client.players[cnt].isRedTeam == true))
				{
					redCallerCnt += 1;
				}
			}
		}
		else if ((isRedTeam.isOn == false) && (isBlueTeam.isOn == true))
		{
			for (int cnt = 0;cnt < client.players.Count; cnt ++)
			{
				if ((client.players[cnt].isPlayer == false) && (client.players[cnt].isRedTeam == false))
				{
					blueCallerCnt += 1;
				}
			}
		}
		for (int cnt = 0; cnt < client.players.Count; cnt++) {
			if (String.Equals(client.players[cnt].name,lineItemText.text)){
				if ((blueCallerCnt > 1) || (redCallerCnt > 1)) {
					isCaller.isOn = false;
					client.players [cnt].isPlayer = true;	
				} else if ((blueCallerCnt < 1) && (redCallerCnt < 1)) {
					isCaller.isOn = true;
					client.players [cnt].isPlayer = false;
				}
			}
		}
	}
}
