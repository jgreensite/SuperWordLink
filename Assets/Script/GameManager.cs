using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using AssemblyCSharp;

public class GameManager : MonoBehaviour {

	//makes class a singleton
	public static GameManager Instance { set; get; }

	public GameObject mainMenu;
	public GameObject serverMenu;
	public GameObject connectMenu;
	public GameObject lobbyMenu;
	public GameObject gameMenu;
	public List<GameObject> goDontDestroyList = new List<GameObject>();

	public GameObject serverPrefab;
	public GameObject clientPrefab;

	public InputField nameInput;

	// Keep this on the GameManager so the server can be run on a seperate machine
	public InputField minPlayers;

	//Server UI
	public Toggle toggleHostLocal;
	public InputField HostAddress;

	private bool isHostLocal;

	private void Start()
	{
		//needed to make this a singleton
		Instance = this;

		mainMenu.SetActive(true);
		serverMenu.SetActive(false);
		connectMenu.SetActive(false);
		lobbyMenu.SetActive(false);


		DontDestroyOnLoad (gameObject);
		GameManager.Instance.goDontDestroyList.Add (gameObject);
		Debug.Log ("Added GameManager at position:" + GameManager.Instance.goDontDestroyList.Count + " to donotdestroylist");
	}

	public void ConnectButton()
	{
		mainMenu.SetActive(false);
		serverMenu.SetActive(false);
		connectMenu.SetActive (true);
		//sets the default server that is going to be connected to
		toggleHostLocal.isOn = false;
		toggleHostLocal.isOn = true;
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
				c.ConnectToServer(CS.GAMESERVERLOCALADDRESS, CS.GAMESERVERPORT);
			}
			else
			{
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

	public void HostLocalConnectToggle()
	{
		if (toggleHostLocal.isOn == true)
		{
			HostAddress.text = CS.GAMESERVERLOCALADDRESS;
			HostAddress.readOnly = true;
		} else
		{
			HostAddress.text = CS.GAMESERVERREMOTEADDRESS;
			HostAddress.readOnly = false;
		}
	}

	public void ConnectToServerButton()
	{
		//string HostAddress = GameObject.Find ("HostInput").GetComponent<InputField> ().text;
		if (HostAddress.text == "")
			HostAddress.text = CS.GAMESERVERREMOTEADDRESS;
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

			c.ConnectToServer(HostAddress.text, CS.GAMESERVERPORT);
			Debug.Log("Connecting to " + HostAddress.text + ":" + CS.GAMESERVERPORT);
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
		Debug.Log ("Game Options UI Back button pressed");
		mainMenu.SetActive(true);
		serverMenu.SetActive (false);
		connectMenu.SetActive(false);
		lobbyMenu.SetActive(false);

		RestartAll ();

//		Server s = FindObjectOfType<Server> ();
//		if (s != null)
//			DestroyObject (s.gameObject);
//
//		Client c = FindObjectOfType<Client> ();
//		if (c != null)
//			DestroyObject (c.gameObject);
	}

	public void PanZoomButton(Button buttonPanZoom)
	{
		bool isClicked;
		GameObject cam;
		cam = GameBoard.Instance.currentCamera;
		//Toggle the camera's Pan & Zoom capability
		isClicked = cam.GetComponent<CameraHandler> ().isPanZoom;
		cam.GetComponent<CameraHandler> ().isPanZoom = !isClicked;
		if (isClicked)
		{
			buttonPanZoom.GetComponentInChildren<Text>().text = CS.UILABELPANZOOM;
		}
		else
		{
			buttonPanZoom.GetComponentInChildren<Text>().text = CS.UILABELSELECT;
		}
	}

	public void StartGame()
	{
		SceneManager.LoadScene ("Main");
	}

	public void RestartAll()
	{
		// Call the shutdown script on the server
		Server s = FindObjectOfType<Server> ();
		if (s != null)
			Server.Instance.Shutdown ();

		//destroy the objects that were labelled as donotdestroy when we are restarting
		for (var i = GameManager.Instance.goDontDestroyList.Count - 1; i > -1; i--)
		{
			if (GameManager.Instance.goDontDestroyList [i] != null)
			{
				Destroy (GameManager.Instance.goDontDestroyList [i]);
				GameManager.Instance.goDontDestroyList.RemoveAt (i);
			}
		}
		SceneManager.LoadScene ("Menu");
	}
}
