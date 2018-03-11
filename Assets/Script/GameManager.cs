using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using AssemblyCSharp;

public class GameManager : MonoBehaviour {

	public static GameManager Instance { set; get; }

	public GameObject mainMenu;
	public GameObject serverMenu;
	public GameObject connectMenu;
	public GameObject lobbyMenu;

	public GameObject serverPrefab;
	public GameObject clientPrefab;

	public InputField nameInput;

	// Keep this on the GameManager so the server can be run on a seperate machine
	public InputField minPlayers;

	private bool isHostLocal;

	private void Start()
	{
		Instance = this;
		mainMenu.SetActive(true);
		serverMenu.SetActive(false);
		connectMenu.SetActive(false);
		lobbyMenu.SetActive(false);


		DontDestroyOnLoad (gameObject);

	}

	public void ConnectButton()
	{
		mainMenu.SetActive(false);
		serverMenu.SetActive(false);
		connectMenu.SetActive (true);
		lobbyMenu.SetActive(false);
	}

	public void setHostingToLocal()
	{
		isHostLocal=true;
	}

	public void setHostingToRemote()
	{
		isHostLocal=false;
	}

	public void HostButton()
	{
		try
		{
			if (isHostLocal == true)
			{
				//Create the Host's server first
				Server s = Instantiate(serverPrefab).GetComponent<Server>();
				//TODO - Remove PORT hardcoding
				s.Init();
			}

			int rnd = UnityEngine.Random.Range(0,99999);

			//The Host has a client as well as a server
			Client c = Instantiate(clientPrefab).GetComponent<Client>();
			c.clientName = nameInput.text;
			if (c.clientName == "")
			{
				c.clientName = "Host_" + rnd.ToString();
			}
	
			c.isHost = true;
			c.isPlayer = false;
			c.isRedTeam = true;


			if (isHostLocal == true)
			{
				//TODO - Remove PORT hardcoding
				c.ConnectToServer(CS.GAMESERVERLOCALADDRESS, CS.GAMESERVERPORT);
			}
			else
			{
				//TODO - Remove PORT hardcoding
				c.ConnectToServer(CS.GAMESERVERREMOTEADDRESS, CS.GAMESERVERPORT);				
			}
		}
		catch (Exception e)
		{
			Debug.Log (e.Message);
		}

		mainMenu.SetActive(false);
		serverMenu.SetActive (true);
		connectMenu.SetActive(false);
		lobbyMenu.SetActive(false);
	}

	public void ConnectToServerButton()
	{
		string HostAddress = GameObject.Find ("HostInput").GetComponent<InputField> ().text;
		if (HostAddress == "")
			HostAddress = "127.0.0.1";
		try
		{
			Client c = Instantiate(clientPrefab).GetComponent<Client>();
			//TODO - Remove PORT hardcoding
			c.clientName = nameInput.text;
			int rnd = UnityEngine.Random.Range(0,99999);
			if (c.clientName == "")
			{
				c.clientName = "Client_" + rnd.ToString();
			}
			c.isHost = false;
			c.isPlayer = true;
			c.isRedTeam = false;

			c.ConnectToServer(HostAddress, 6321);
			connectMenu.SetActive(false);
		}
		catch (Exception e)
		{
			Debug.Log (e.Message);
		}
	}

	public void OpenLobby()
	{
		mainMenu.SetActive(false);
		serverMenu.SetActive (false);
		connectMenu.SetActive(false);
		lobbyMenu.SetActive(true);
	}

	public void StartGameButton()
	{
		Client client = FindObjectOfType<Client> ();
		client.StartGame ();
	}

	public void BackButton()
	{
		mainMenu.SetActive(true);
		serverMenu.SetActive (false);
		connectMenu.SetActive(false);
		lobbyMenu.SetActive(false);

		Server s = FindObjectOfType<Server> ();
		if (s != null)
			DestroyObject (s.gameObject);

		Client c = FindObjectOfType<Client> ();
		if (c != null)
			DestroyObject (c.gameObject);
	}

	public void StartGame()
	{
		SceneManager.LoadScene ("Main");
	}
}
