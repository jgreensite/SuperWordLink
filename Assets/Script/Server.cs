using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class Server : MonoBehaviour
{
	//makes class a singleton
	public static Server Instance { set; get; }

	public int port = 6321;
	private List<ServerClient> clients;
	private List<ServerClient> disconnectList;

	//the actual server
	private TcpListener server;
	private bool serverStarted;

	//the list of words and their assignments

	string[] words = new string[25];
	string[] populate = new string[25];
	public int numParticipants = 0;

	public void Init(){

		//needed to make this a singleton
		Instance = this;

		//needed to preserve game objects
		DontDestroyOnLoad(gameObject);
		GameManager.Instance.goDontDestroyList.Add (gameObject);
		Debug.Log ("Added Server at position:" + GameManager.Instance.goDontDestroyList.Count + " to donotdestroylist");

		//create lists of clients that need to be connected / disconnected
		clients = new List<ServerClient>();
		disconnectList = new List<ServerClient>();

		try
		{
			//create a new listener and start it listening
			server = new TcpListener (IPAddress.Any, port);
			server.Start();
			startListening();
			serverStarted = true;
		}
		catch (Exception e)
		{
			Debug.Log("Socket Error Server: " + e.Message);
		}
	}

	private void Update()
	// Note if we want this on a standard Linux server that has no context of Unity we'll need to take out the reference to Monobehaviour and build an update function ourselves (running a continuous loop waiting for something to happen
	{
		if (!serverStarted)
		{
			return;
		}
			
		foreach (ServerClient c in clients)
		{
			//is the client still connected?

			if(!IsConnected(c.tcp))
			//if not disconnect the tcp client
			{
				c.tcp.Close();
				disconnectList.Add (c);
				continue;
			}
			else
			//assume if not disconnected then are connected
			{
				//check the tcp stream of each client to see if they have placed something on it
				NetworkStream s = c.tcp.GetStream();
				if (s.DataAvailable)
				{
					StreamReader reader = new StreamReader (s, true);
					// all data on a single line
					string data = reader.ReadLine ();
					if (data != null)
					// if there is data then read it
					{
						OnIncomingData (c, data);
					}
				}	
			}
		}

		for (int i = 0; i < disconnectList.Count - 1; i++)
		{
			//Tell the player running the server that someone has disconnected

			clients.Remove (disconnectList [i]);
			disconnectList.RemoveAt (i);
		}
	}

	private void startListening()
	{
		//uses an asynchronous callback to connect a server to a client, this allows us to listen for connections but will not get any data from them
		server.BeginAcceptTcpClient (AcceptTcpClient, server);
	}

	private void AcceptTcpClient(IAsyncResult ar)
	{
		//Listen for a client connection
		TcpListener listener = (TcpListener)ar.AsyncState;

		//Get a list of all the users that are connected, do this before the client connects
		string Allusers = "";
		foreach (ServerClient i in clients)
		{
			Allusers +=
			"|"
			+ i.clientName + ","
			+ ((i.isHost)?1:0).ToString() + ","
			+ ((i.isPlayer)?1:0).ToString() + ","
			+ ((i.isRedTeam)?1:0).ToString();
		}

		//If a client connection occurs add it to the list of clients
		ServerClient sc = new ServerClient (listener.EndAcceptTcpClient (ar));
		clients.Add (sc);

		//Once a connection occurs the listener will stop, so if you want to listen for more clients you need to restart listening again.
		startListening();

		Debug.Log ("Somebody has connected. Starting to listen for any other clients");

		//Ask last person that connected to state who they are
		Broadcast ("SWHO" + Allusers, clients[clients.Count-1]);

	}
		
	private bool IsConnected(TcpClient c)
	{
		try
		{
			if (c !=null && c.Client !=null && c.Client.Connected)
			{
				if(c.Client.Poll(0,SelectMode.SelectRead))
					return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
				return true;
			}
			else
			{
				return false;	
			}
		}
		catch{
			//TODO - Should not have processing on the exception path, need to handle this differently
			return false;
		}
	}

	//Server send
	private void Broadcast(string data, List<ServerClient> cl)
	{
		foreach (ServerClient sc in cl)
		{
			try
			{
				Debug.Log ("Server Sending To:" + sc.clientName + " => " + data);
				StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
				writer.WriteLine(data);
				writer.Flush();
			}
			catch (Exception e)
			{
				Debug.Log ("Write error : " + e.Message);
			}
		}
	}

	//Used to create a list of clients that only has one item in it, so that can broadcast when only one client connected
	private void Broadcast(string data, ServerClient c)
	{
		List<ServerClient> sc = new List<ServerClient>{ c };
		Broadcast (data, sc);
	}

	//Server read
	private void OnIncomingData(ServerClient c, string data)
	{
		Debug.Log ("Server Receiving: " + data);

		//parse the incoming data stream
		string[] aData = data.Split('|');

		switch (aData [0])
		{
		case "CWHO":
			//See if the number of participants is greater than zero, if it is then it must have been sent

			// if the new client that is added is a host then it will determine the number of players
			if ((aData[2] == "1")? true : false) 
			{
				Int32.TryParse (aData[5], out numParticipants);
			
				//if a client intitiates a host call then clear all other client's connections
				int i = 0;
				if (clients.Count > 1)
				{
					do
					{
						ServerClient sc = clients [i];

						//is the client still connected?
						if (IsConnected (sc.tcp))

						//TODO - upgrade the server to be able to manage multiple games and present the list of games that are currently running to the players
						{
							sc.tcp.Close ();
							clients.Remove (sc);
						}
						i++;
					} while (i < clients.Count - 2);
				}
			}

			// add the new client details, remember that it will always be the last client in the list of clients that we do not have the details for
			clients[clients.Count-1].clientName = aData[1];
			clients[clients.Count-1].isHost = ((aData[2] == "1")? true : false);
			clients[clients.Count-1].isPlayer = ((aData[3] == "1")? true : false);
			clients[clients.Count-1].isRedTeam = ((aData[4] == "1")? true : false);

			//get a list of all the users that are connected, do this after adding the new client's details to this list
			//broadcast this updated list to all the connected clients
			Broadcast ("SCNN" + GetStringOfAllClients(), clients);
			break;
		case "CMOV":
			Broadcast (
				"SMOV" + '|'
				+ aData [1] + '|'
				+ aData [2] + '|'
				+ aData [3],
				clients
			);
			break;
		case "CDIC":
			//Generate the Wordlist and assignment
			WordDictionary worddictionary = FindObjectOfType<WordDictionary> ();
			words = worddictionary.GetWords();
			populate = worddictionary.AssignWords ();

			string distWords = "";
			string distPopulate = "";
			foreach (string tmpStr in words)
			{
				distWords += tmpStr + ",";	
			}

			foreach (string tmpStr in populate)
			{
				distPopulate += tmpStr + ",";	
			}
			Broadcast (
				"SDIC" + '|'
				+ distWords + '|'
				+ distPopulate,
				clients
			);
			break;
		case "CKEY":
			string tmpVal = aData [2].ToUpper();
			Broadcast (
				"SKEY" + '|'
				+ tmpVal,
				clients
			);
			break;
		case "CBEG":
			//Note that start at 2 not 1 becuase the name of the client is transmitted at position 1
			for (int i = 2; i < aData.Length; i++)
			{
				string[] bData = aData [i].Split (',');
				foreach (ServerClient sc in clients)
				{
					//TODO - improve matching, should not be matching on client name it's brittle
					//Populate the client attributes
					if (String.Equals (bData [0], sc.clientName))
					{
						sc.isPlayer = (bData [1] == "0") ? false : true;
						sc.isRedTeam = (bData [2] == "0") ? false : true;
					}
				}
	
			}

			Broadcast (
				"SBEG" + GetStringOfAllClients(),
				clients
			);
			break;
		}
	}

	private string GetStringOfAllClients()
	{
		string concatClients = "";
		for (int i = 0; i < clients.Count;i ++)
		{
			concatClients += "|"
				+ clients[i].clientName + ","
				+ ((clients[i].isHost) ? 1 : 0).ToString () + ","
				+ ((clients[i].isPlayer) ? 1 : 0).ToString () + ","
				+ ((clients[i].isRedTeam) ? 1 : 0).ToString () + ",";

			// TODO - Change this it is not the easiest to understand code
			// this is a "magic field" that tells the client it should add the participant and start the lobby
			// it is placed on the last entry of the message sent to the client
			// Really this should be another message embedded in this message
			if ((numParticipants != 0) && (clients.Count == numParticipants) && (i == clients.Count-1))
			{
				concatClients += numParticipants.ToString ();
			}
			else
			{
				concatClients += 0;
			}
		}
		return concatClients;
	}

	public void Shutdown()
	{
		foreach (ServerClient c in clients)
		{
			//is the client still connected?

			if(IsConnected(c.tcp))
				//if not disconnect the tcp client
			{
				c.tcp.Close();
				disconnectList.Add (c);
				continue;
			}
		}

		for (int i = 0; i < disconnectList.Count - 1; i++)
		{
			//Tell the player running the server that someone has disconnected

			clients.Remove (disconnectList [i]);
			disconnectList.RemoveAt (i);
		}
		server.Stop();
	}
}

public class ServerClient
{
	public string clientName;
	public TcpClient tcp;
	public bool isHost;
	public bool isPlayer;
	public bool isRedTeam;

	public ServerClient(TcpClient tcp)
	{
		this.tcp = tcp;
	}
}