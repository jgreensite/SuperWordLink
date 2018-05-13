using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.IO;
using AssemblyCSharp;

public class Client : MonoBehaviour
{
	public string clientName;
	public bool isHost;
	public bool isRedTeam;
	public bool isPlayer;
	public string clientID;

	private bool socketReady;
	private TcpClient socket;
	private NetworkStream stream;
	private StreamWriter writer;
	private StreamReader reader;
	private int numParticipants;

	public List<GameClient> players = new List<GameClient>();

	//the decks that will be used in the game
	private GameCardDeck gcd = new GameCardDeck ();
	private GameCardDeck gcdRed = new GameCardDeck ();
	private GameCardDeck gcdBlue = new GameCardDeck ();

	private void Start()
	{
		DontDestroyOnLoad(gameObject);
		GameManager.Instance.goDontDestroyList.Add (gameObject);
		Debug.Log ("Added Client at position:" + GameManager.Instance.goDontDestroyList.Count + " to donotdestroylist");
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
			Debug.Log ("Socket Error Client: " + e.Message);
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
		Debug.Log ("Client Sending:" + data);
		writer.WriteLine(data);
		writer.Flush();

	}

	//Reading messages from the Server
	private void OnIncomingData(string data)
	{
		Debug.Log ("Client Receiving: " + data);
		int howManyPlaying;


		//TODO - remove this when all messaging is done via XML
		if (!(data[0].Equals('<')))
		{
			string[] aData = data.Split('|');
			switch (aData [0])
			{
			case "SWHO":
				players.Clear ();
				for (int i = 1; i < aData.Length; i++)
				{
					string[] bData = aData [i].Split (',');
					UserConnected (
						bData [0],
						(bData [1] == "0") ? false : true,
						(bData [2] == "0") ? false : true,
						(bData [3] == "0") ? false : true,
						bData [4]
					);		
				}
			//Get the number of participants from the GameManager and convet to an integer
			//Note that cannot write the number of participants that is used to decide to start the game
			//this must be initiated from the server sending a message to the client
				if (Int32.TryParse (GameManager.Instance.minPlayers.text, out howManyPlaying))
				{

				} else if ((isHost) && (GameManager.Instance.minPlayers.text == ""))
				{
					howManyPlaying = 1;
				} else
				{
					howManyPlaying = 0;
				}
				Send (
					"CWHO" + '|'
					+ clientName + '|'
					+ ((isHost) ? 1 : 0).ToString () + '|'
					+ ((isPlayer) ? 1 : 0).ToString () + '|'
					+ ((isRedTeam) ? 1 : 0).ToString () + '|'
					+ clientID + '|'
					+ howManyPlaying.ToString ()

				);
				break;

			case "SCNN":
				players.Clear ();
				for (int i = 1; i < aData.Length; i++)
				{
					string[] bData = aData [i].Split (',');
					UserConnected (
						bData [0],
						(bData [1] == "0") ? false : true,
						(bData [2] == "0") ? false : true,
						(bData [3] == "0") ? false : true,
						bData [4]
					);

					//if we get the signal to start the lobby, start it 
					if (bData [5] != "0")
					{
					GameManager.Instance.OpenLobby ();
					}
				}
				
				break;
			case "SMOV":
				int x = int.Parse (aData [2]);
				int z = int.Parse (aData [3]);
				GameBoard.Instance.TryMove (x, z);
				break;

			case "SDIC":
				string[] wData = aData [1].Split (',');
				GameBoard.Instance.words = wData;

				string[] pData = aData [2].Split (',');
				GameBoard.Instance.populate = pData;

				GameBoard.Instance.GeneratePlayerGameboard ();

				GetGameCardDeck ();
				break;
			case "SKEY":
				{
					switch (aData [1])
					{
					case "R":
						GameBoard.Instance.ResartGame ();
						break;
				
					case "P":
						GameBoard.Instance.SetCamera (aData [1]);
						break;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    
				
					case "C":
						GameBoard.Instance.SetCamera (aData [1]);
						break;
					}
				}			
				break;
			case "SBEG":

				for (int i = 1; i < aData.Length; i++)
				{
					string[] bData = aData [i].Split (',');
					UserConnected (
						bData [0],
						(bData [1] == "0") ? false : true,
						(bData [2] == "0") ? false : true,
						(bData [3] == "0") ? false : true,
						bData [4]
					);

					//Populate the client attributes
					if (String.Equals (bData [4], clientID))
					{
						isHost = (bData [1] == "0") ? false : true;
						isPlayer = (bData [2] == "0") ? false : true;
						isRedTeam = (bData [3] == "0") ? false : true;
					}
				}
				GameManager.Instance.StartGame ();
				break;
			}
		}
		//Must be XML
		else 
		{
			//TODO - Assumes that the xml message is one to populate a game card deck
			gcd = GameCardDeck.LoadFromText(data);
			for(int cnt = gcd.gameCards.Count-1; cnt > -1 ;cnt--)
			{
				GameCard gc = gcd.gameCards[cnt]; 
				if (gc.cardSuit == CS.RED_TEAM)
				{
					gcd.gameCards.Remove (gc);
					gcdRed.gameCards.Add (gc);
				}
				else if (gc.cardSuit == CS.BLUE_TEAM)
				{
					gcd.gameCards.Remove (gc);
					gcdBlue.gameCards.Add (gc);
				}
			}
			// Having populated the client side data strucutres for the deck now render the deck
			GameBoard.Instance.GeneratePlayerHand();
		}
	}
		
	//Called when a message is received that a user has connected
	private void UserConnected(string name, bool isHost, bool isPlayer, bool isRedTeam, string clientID)
	{
		GameClient c = new GameClient ();
		c.name = name;
		c.isHost = isHost;
		c.isPlayer = isPlayer;
		c.isRedTeam = isRedTeam;
		c.clientID = clientID;

		players.Add (c);
		//TODO - Update the panel message to say "waiting for host to choose teams"
	}

	public void StartGame()
	{
		string concatPlayers = "";

		for (int cnt = 0; cnt < players.Count; cnt++)
		{
			concatPlayers += "|"
			+ players [cnt].name + ","
			+ ((players [cnt].isPlayer) ? 1 : 0).ToString () + ","
			+ ((players [cnt].isRedTeam) ? 1 : 0).ToString () + ","
			+ players[cnt].clientID;
		}
			
		Send(
			"CBEG" + "|"
			+ clientName
			+ concatPlayers
		);
	}

	public void GetGameCardDeck()
	{
		Send (
			"CPCI" + "|"
			+ clientName + '|'
			+ ((isHost) ? 1 : 0).ToString () + '|'
			+ ((isPlayer) ? 1 : 0).ToString () + '|'
			+ ((isRedTeam) ? 1 : 0).ToString () + '|'
			+ clientID + '|'
		);
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
	public string clientID;
}

