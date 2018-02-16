using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.IO;

public class Client : MonoBehaviour
{
	public string clientName;
	public bool isHost;
	public bool isRedTeam;
	public bool isPlayer;

	private bool socketReady;
	private TcpClient socket;
	private NetworkStream stream;
	private StreamWriter writer;
	private StreamReader reader;

	private List<GameClient> players = new List<GameClient>();

	private void Start()
	{

		DontDestroyOnLoad(gameObject);
	}

	public bool ConnectToServer (string host, int port)
	{
		if (socketReady)
			return false;
		try
		{
			socket = new TcpClient(host, port);	
			stream = socket.GetStream();
			writer = new StreamWriter(stream);
			reader = new StreamReader(stream);

			socketReady = true;
		}
		catch (Exception e)
		{
			Debug.Log ("Socket Error : " + e.Message);
		}
		return socketReady;
	}

	private void Update()
	{
		if (socketReady)
		{
			if (stream.DataAvailable)
			{
				string data = reader.ReadLine ();
				if (data != null)
					OnIncomingData(data);
			}
		}
	}

	//Sending messages to the Server
	public void Send(string data)
	{
		if (!socketReady)
		{
			return;
		}
		writer.WriteLine(data);
		writer.Flush();

	}

	//Reading messages from the Server
	private void OnIncomingData(string data)
	{
		Debug.Log ("Client: " + data);

		string[] aData = data.Split('|');

		switch (aData [0])
		{
		case "SWHO":
			for (int i = 1; i < aData.Length - 1; i++)
			{
				//TODO - this was originally UserConnected (aData [i], false);, it's been hacked to get it to compile
				UserConnected (aData [i], false, true, true);		
			}
			Send (
				"CWHO" + '|'
				+ clientName + '|'
				+ ((isHost)?1:0).ToString() + '|'
				+ ((isPlayer)?1:0).ToString() +'|'
				+ ((isRedTeam)?1:0).ToString()
			);
			break;

		case "SCNN":
			UserConnected (
				aData [1],
				(aData [2] == "0") ? false : true,
				(aData [3] == "0") ? false : true,
				(aData [4] == "0") ? false : true
			);
			break;
		case "SMOV":
			int x = int.Parse(aData [2]);
			int z = int.Parse(aData [3]);
			GameBoard.Instance.TryMove(x,z);
			break;

		case "SDIC":
			string[] wData = aData[1].Split(',');
			GameBoard.Instance.words = wData;

			string[] pData = aData[2].Split(',');
			GameBoard.Instance.populate = pData;

			GameBoard.Instance.GenerateGameboard();
			break;
		case "SKEY":
			{
				switch (aData [1])
				{
				case "R":
					GameBoard.Instance.resartGame ();
					break;
				
				case "P":
					GameBoard.Instance.setCamera (aData [1]);
					break;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    
				
				case "C":
					GameBoard.Instance.setCamera (aData [1]);
					break;
				}
			}			
			break;
		case "SBEG":
			GameManager.Instance.StartGame ();
			break;
		}
	}
		
	//Called when a message is received that a user has connected
	private void UserConnected(string name, bool isHost, bool isPlayer, bool isRedTeam)
	{
		GameClient c = new GameClient ();
		c.name = name;
		c.isHost = isHost;
		c.isPlayer = isPlayer;
		c.isRedTeam = isRedTeam;

		players.Add (c);

		//Once at least two players are connected allow the host to assign players
		//note that all client's need to start the game but they need to share data regarding moves and cards

		//TODO - Create teams for the players
		if (players.Count >= 2)
		{
			//TODO - Update the panel message to say "waiting for host to choose teams"
			{
				//checks if this client is the host
//				if (isHost == true)
					GameManager.Instance.OpenLobby();
			}
		}
	}

	//Called when the application quits, from Monobehaviour
	private void OnApplicationQuit()
	{
		closeSocket ();
	}

	//Called when the object is disabled, from Monobehaviour
	private void OnDisable()
	{
		closeSocket ();
	}

	//Close socket, especially if there is a crash don't leave the socket open
	private void closeSocket()
	{
		if (!socketReady)
			return;
		writer.Close ();
		reader.Close ();
		socket.Close ();
		socketReady = false;
	}
} 

public class GameClient
{
	public string name;
	public bool isHost;
	public bool isPlayer;
	public bool isRedTeam;
}
