using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyLineItem : MonoBehaviour {

	public Text lineItemText;
	public Toggle isCaller;
	public Toggle isRedTeam;
	public Toggle isBlueTeam;

	Client client;

	// Use this for initialization
	void Start () {
		client = FindObjectOfType<Client> ();
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
		

	public void SelectCaller()
	{
		int redCallerCnt = 0;
		int blueCallerCnt = 0;

		if ((isRedTeam.isOn == true) && (isBlueTeam.isOn == false))
		{
			foreach (GameClient gc in client.players)
			{
				if (gc.isPlayer == false)
				{
					redCallerCnt += 1;
				}
			}
		}
		else if ((isRedTeam.isOn == false) && (isBlueTeam.isOn == true))
		{
			foreach (GameClient gc in client.players)
			{
				if (gc.isPlayer == false)
				{
					blueCallerCnt += 1;
				}
			}
		}
		if ((blueCallerCnt > 1) || (redCallerCnt > 1))
		{
			isCaller.isOn = false;
		}
		else
		{
			isCaller.isOn = true;
		}
	}
}
