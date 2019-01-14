using System;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public GameObject clientPrefab;
    public GameObject connectMenu;
    public GameObject gameMenu;
    public List<GameObject> goDontDestroyList = new List<GameObject>();
    public InputField HostAddress;

    private bool isHostLocal;
    public GameObject lobbyMenu;

    public GameObject mainMenu;

    // Keep this on the GameManager so the server can be run on a seperate machine
    public InputField minPlayers;

    public InputField nameInput;
    public GameObject serverMenu;

    public GameObject serverPrefab;

    //Server UI
    public Toggle toggleHostLocal;

    //makes class a singleton
    public static GameManager Instance { set; get; }

    private void Start()
    {
        //needed to make this a singleton
        Instance = this;

        mainMenu.SetActive(true);
        serverMenu.SetActive(false);
        connectMenu.SetActive(false);
        lobbyMenu.SetActive(false);


        DontDestroyOnLoad(gameObject);
        Instance.goDontDestroyList.Add(gameObject);
        Debug.Log("Added GameManager at position:" + Instance.goDontDestroyList.Count + " to donotdestroylist");
    }

    public void ConnectButton()
    {
        mainMenu.SetActive(false);
        serverMenu.SetActive(false);
        connectMenu.SetActive(true);
        //sets the default server that is going to be connected to
        toggleHostLocal.isOn = false;
        toggleHostLocal.isOn = true;
        lobbyMenu.SetActive(false);
    }

    public void setHostingToLocal()
    {
        isHostLocal = true;
    }

    public void setHostingToRemote()
    {
        isHostLocal = false;
    }

    public void HostButton()
    {
        try
        {
            if (isHostLocal)
            {
                //Create the Host's server first
                var s = Instantiate(serverPrefab);
                s.GetComponent<Server>().Init();
                s.GetComponent<GameBoardState>().Init();
            }

            var rnd = Random.Range(0, 99999);

            //The Host has a client as well as a server
            var c = Instantiate(clientPrefab).GetComponent<Client>();
            c.clientName = nameInput.text;
            if (c.clientName == "") c.clientName = "Host_" + rnd;

            c.isHost = true;
            c.isPlayer = false;
            c.isRedTeam = true;
            c.clientID = rnd.ToString();


            if (isHostLocal)
                c.ConnectToServer(CS.GAMESERVERLOCALADDRESS, CS.GAMESERVERPORT);
            else
                c.ConnectToServer(CS.GAMESERVERREMOTEADDRESS, CS.GAMESERVERPORT);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        mainMenu.SetActive(false);
        serverMenu.SetActive(true);
        connectMenu.SetActive(false);
        lobbyMenu.SetActive(false);
    }

    public void HostLocalConnectToggle()
    {
        if (toggleHostLocal.isOn)
        {
            HostAddress.text = CS.GAMESERVERLOCALADDRESS;
            HostAddress.readOnly = true;
        }
        else
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
            var c = Instantiate(clientPrefab).GetComponent<Client>();
            c.clientName = nameInput.text;
            var rnd = Random.Range(0, 99999);
            if (c.clientName == "") c.clientName = "Client_" + rnd;
            c.isHost = false;
            c.isPlayer = true;
            c.isRedTeam = false;
            c.clientID = rnd.ToString();

            c.ConnectToServer(HostAddress.text, CS.GAMESERVERPORT);
            Debug.Log("Connecting to " + HostAddress.text + ":" + CS.GAMESERVERPORT);
            connectMenu.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void OpenLobby()
    {
        mainMenu.SetActive(false);
        serverMenu.SetActive(false);
        connectMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }

    public void StartGameButton()
    {
        var client = FindObjectOfType<Client>();
        client.StartGame();
    }

    public void BackButton()
    {
        Debug.Log("Game Options UI Back button pressed");
        mainMenu.SetActive(true);
        serverMenu.SetActive(false);
        connectMenu.SetActive(false);
        lobbyMenu.SetActive(false);

        RestartAll();

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
        isClicked = cam.GetComponent<CameraHandler>().isPanZoom;
        cam.GetComponent<CameraHandler>().isPanZoom = !isClicked;
        if (isClicked)
            buttonPanZoom.GetComponentInChildren<Text>().text = CS.UILABELPANZOOM;
        else
            buttonPanZoom.GetComponentInChildren<Text>().text = CS.UILABELSELECT;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Main");
    }

    public void RestartAll()
    {
        // Call the shutdown script on the server
        var s = FindObjectOfType<Server>();
        if (s != null)
            Server.Instance.Shutdown();

        //destroy the objects that were labelled as donotdestroy when we are restarting
        for (var i = Instance.goDontDestroyList.Count - 1; i > -1; i--)
            if (Instance.goDontDestroyList[i] != null)
            {
                Destroy(Instance.goDontDestroyList[i]);
                Instance.goDontDestroyList.RemoveAt(i);
            }

        SceneManager.LoadScene("Menu");
    }
}