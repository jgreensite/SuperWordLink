using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using AssemblyCSharp;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;

public class Server : MonoBehaviour
{
    
    public static List<ServerClient> clients;
    private static List<ServerClient> disconnectList;

    //the decks that will be used in the game
    private readonly GameHandDeck gcd = new GameHandDeck();
    private readonly GameHandDeck gcdBlue = new GameHandDeck();
    private readonly GameHandDeck gcdRed = new GameHandDeck();
    public int numParticipants;
    private string[] populate = new string[25];

    public int port = 6321;

    //the actual server
    private TcpListener server;
    private bool serverStarted;

    //the list of words and their assignments
    private string[,] words = new string[2,25];
    
    //which team's turn it is
    private bool isRedTurn;
    
    //makes class a singleton
    public static Server Instance { set; get; }



    public void Init()
    {
        //needed to make this a singleton
        Instance = this;
    
        //needed to preserve game objects
        DontDestroyOnLoad(gameObject);
        GameManager.Instance.goDontDestroyList.Add(gameObject);
        Debug.Log("Added Server at position:" + GameManager.Instance.goDontDestroyList.Count + " to donotdestroylist");

        //create lists of clients that need to be connected / disconnected
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();
        
        try
        {
            //create a new listener and start it listening
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            startListening();
            serverStarted = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket Error Server: " + e.Message);
        }

//		BuildDeck ();
    }

    private void Update()
        // Note if we want this on a standard Linux server that has no context of Unity we'll need to take out the reference to Monobehaviour and build an update function ourselves (running a continuous loop waiting for something to happen
    {
        if (!serverStarted) return;

        foreach (var c in clients)
            //is the client still connected?

            if (!IsConnected(c.tcp))
                //if not disconnect the tcp client
            {
                c.tcp.Close();
                disconnectList.Add(c);
            }
            else
                //assume if not disconnected then are connected
            {
                //check the tcp stream of each client to see if they have placed something on it
                var s = c.tcp.GetStream();
                if (s.DataAvailable)
                {
                    var reader = new StreamReader(s, true);
                    // all data on a single line
                    var data = reader.ReadLine();
                    if (data != null)
                        // if there is data then read it
                        OnIncomingData(c, data);
                }
            }

        for (var i = 0; i < disconnectList.Count - 1; i++)
        {
            //Tell the player running the server that someone has disconnected

            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }
    }

    private void startListening()
    {
        //uses an asynchronous callback to connect a server to a client, this allows us to listen for connections but will not get any data from them
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        //Listen for a client connection
        var listener = (TcpListener) ar.AsyncState;

        //Get a list of all the users that are connected, do this before the client connects
        var Allusers = "";
        foreach (var i in clients)
            Allusers +=
                "|"
                + i.clientName + ","
                + (i.isHost ? 1 : 0) + ","
                + (i.isPlayer ? 1 : 0) + ","
                + (i.isRedTeam ? 1 : 0) + ","
                + i.clientID;

        //If a client connection occurs add it to the list of clients
        var sc = new ServerClient(listener.EndAcceptTcpClient(ar));
        clients.Add(sc);

        //Once a connection occurs the listener will stop, so if you want to listen for more clients you need to restart listening again.
        startListening();

        Debug.Log("Somebody has connected. Starting to listen for any other clients");

        //Ask last person that connected to state who they are
        Broadcast("SWHO" + Allusers, clients[clients.Count - 1]);
    }

    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                return true;
            }

            return false;
        }
        catch
        {
            //TODO - Should not have processing on the exception path, need to handle this differently
            return false;
        }
    }

    //Server send
    private void Broadcast(string data, List<ServerClient> cl)
    {
        foreach (var sc in cl)
            try
            {
                Debug.Log("Server Sending To:" + sc.clientName + " => " + data);
                var writer = new StreamWriter(sc.tcp.GetStream());
                //this has been replace from data to xml
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error : " + e.Message);
            }
    }

    //Used to create a list of clients that only has one item in it, so that can broadcast when only one client connected
    private void Broadcast(string data, ServerClient c)
    {
        var sc = new List<ServerClient> {c};
        Broadcast(data, sc);
    }

    //Server read
    private void OnIncomingData(ServerClient c, string data)
    {
        Debug.Log("Server Receiving: " + data);

        var gbs = FindObjectOfType<GameBoardState>();
        
        //parse the incoming data stream
        var aData = data.Split('|');

        switch (aData[0])
        {
            case "CWHO":
                //See if the number of participants is greater than zero, if it is then it must have been sent

                // if the new client that is added is a host then it will determine the number of players
                if (aData[2] == "1" ? true : false)
                {
                    int.TryParse(aData[6], out numParticipants);

                    //if a client intitiates a host call then clear all other client's connections
                    var i = 0;
                    if (clients.Count > 1)
                        do
                        {
                            var sc = clients[i];

                            //is the client still connected?
                            if (IsConnected(sc.tcp))

                                //TODO - upgrade the server to be able to manage multiple games and present the list of games that are currently running to the players
                            {
                                sc.tcp.Close();
                                clients.Remove(sc);
                            }

                            i++;
                        } while (i < clients.Count - 2);
                }

                // add the new client details, remember that it will always be the last client in the list of clients that we do not have the details for
                clients[clients.Count - 1].clientName = aData[1];
                clients[clients.Count - 1].isHost = aData[2] == "1" ? true : false;
                clients[clients.Count - 1].isPlayer = aData[3] == "1" ? true : false;
                clients[clients.Count - 1].isRedTeam = aData[4] == "1" ? true : false;
                clients[clients.Count - 1].clientID = aData[5];

                //get a list of all the users that are connected, do this after adding the new client's details to this list
                //broadcast this updated list to all the connected clients
                Broadcast("SCNN" + GetStringOfAllClients(), clients);
                break;
            case "CMOV":
                //Send message to GameBoardState
                gbs.Incoming(aData);
   
                Broadcast(
                    "SMOV" + '|'
                           + aData[1] + '|'
                           + aData[2] + '|'
                           + aData[3] + '|'
                           + aData[4],
                    clients
                );
                break;
            case "CHAN":
                //Update the server copy of the deck marking the card as having been played
                //Send message to GameBoardState
                gbs.Incoming(aData);
                
                //TODO - Remove me once have implemented above call
                //UpdateDeckCardStatus(aData[2], aData[3]);
               
                Broadcast(
                    "SHAN" + '|'
                           + aData[1] + '|'
                           + aData[2] + '|'
                           + aData[3],
                    clients
                );
                break;
            case "CDIC":
                //Send message to GameBoardState, this will populate the WordList
                gbs.Incoming(aData);
                
                var distWords = "";
                var distPopulate = "";
    
                //Cannot use FindObjectOfType in the constructor, so have to assign in here    
                var worddictionary = FindObjectOfType<WordDictionary>();
                
                //worddictionary.buildGameboard();
                

                foreach (var tmpStr in worddictionary.wordList) distWords += tmpStr + ",";
                foreach (var tmpStr in worddictionary.populate) distPopulate += tmpStr + ",";
                
                
                Broadcast(
                    "SDIC" + '|'
                           + worddictionary.isRedStart + '|'
                           + distWords + '|'
                           + distPopulate,
                    clients
                );
                break;
            case "CKEY":
                var tmpVal = aData[2].ToUpper();
                Broadcast(
                    "SKEY" + '|'
                           + tmpVal,
                    clients
                );
                break;
            case "CBEG":
                //Note that start at 2 not 1 because the name of the client is transmitted at position 1
                for (var i = 2; i < aData.Length; i++)
                {
                    var bData = aData[i].Split(',');
                    foreach (var sc in clients)
                        //TODO - improve matching, should not be matching on client name it's brittle
                        //Populate the client attributes
                        if (string.Equals(bData[3], sc.clientID))
                        {
                            sc.isPlayer = bData[1] == "0" ? false : true;
                            sc.isRedTeam = bData[2] == "0" ? false : true;
                        }
                }

                Broadcast(
                    "SBEG" + GetStringOfAllClients(),
                    clients
                );
                break;
            case "CPCC":
                //Send message to GameBoardState
                gbs.Incoming(aData);
                //gbs.BuildHandDeck(CS.CREATE);
                Broadcast(gbs.ghd.SaveToText().Replace(Environment.NewLine, ""), clients);
                break;
            case "CPCU":
                //Send message to GameBoardState
                gbs.Incoming(aData);
                //gbs.BuildHandDeck(CS.END);
                Broadcast(gbs.ghd.SaveToText().Replace(Environment.NewLine, ""), clients);
                break;
            
        }
    }

    private string GetStringOfAllClients()
    {
        var concatClients = "";
        for (var i = 0; i < clients.Count; i++)
        {
            concatClients += "|"
                             + clients[i].clientName + ","
                             + (clients[i].isHost ? 1 : 0) + ","
                             + (clients[i].isPlayer ? 1 : 0) + ","
                             + (clients[i].isRedTeam ? 1 : 0) + ","
                             + clients[i].clientID + ",";

            // TODO - Change this it is not the easiest to understand code
            // this is a "magic field" that tells the client it should add the participant and start the lobby
            // it is placed on the last entry of the message sent to the client
            // Really this should be another message embedded in this message
            if (numParticipants != 0 && clients.Count == numParticipants && i == clients.Count - 1)
                concatClients += numParticipants.ToString();
            else
                concatClients += 0;
        }

        return concatClients;
    }

    public void Shutdown()
    {
        foreach (var c in clients)
            //is the client still connected?

            if (IsConnected(c.tcp))
                //if not disconnect the tcp client
            {
                c.tcp.Close();
                disconnectList.Add(c);
            }

        for (var i = 0; i < disconnectList.Count - 1; i++)
        {
            //Tell the player running the server that someone has disconnected

            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }

        server.Stop();
    }

    /*
    private void BuildDeck(string msg)
    {
        var gc = new GameCard();
        var pc = new GameCard(); 

//		//Demo loading data
//		filePath = Application.persistentDataPath + "/gamecarddeck3.xml";
//		gcd = GameCardDeck.Load (filePath);
//		Debug.Log ("Loaded : " + filePath);

        switch (msg)
        {
            //Build new player decks for a new game
            case CS.CREATE:
                //Clear the decks prior to rebuilding them
                gcd.gameCards.Clear();
                gcdBlue.gameCards.Clear();
                gcdRed.gameCards.Clear();

                for (var playerCnt = 0; playerCnt < clients.Count; playerCnt++)
                   for (var cardNum = 0; cardNum < CS.CSCARDHANDDIM; cardNum++)
                   {
                       //create new card
                       gc = makeCard(playerCnt);
                
                       //add Card to deck
                       gcd.gameCards.Add(gc);
                   }
            break;
            
            //update player decks at end of turn
            case CS.END:
                for (var playerCnt = 0; playerCnt < clients.Count; playerCnt++)
                for (var cardNum = 0; cardNum < CS.CSCARDHANDDIM; cardNum++)
                {
                    pc = gcd.gameCards[playerCnt * CS.CSCARDHANDDIM + cardNum];
                    if (pc.cardRevealed == CS.CAR_REVEAL_SHOWN)
                    {
                        //reduce the played count


                        //establish what card has been played

                        //create a new card
                        gc = makeCard(playerCnt);

                        //update deck, replacing old card with new card
                        gcd.gameCards[playerCnt * CS.CSCARDHANDDIM + cardNum] = gc;

                    }
                }
            break;
        }
        SaveDeck();
    }
    */
    /*
    private void SaveDeck()
    {
        string filePath;
        
        //Populate the Red and Blue Decks
        var lastItem = gcd.gameCards.Count;
        var drawnCard = new GameCard();

        for (var cnt = 0; cnt <= lastItem - 1; cnt++)
        {
            drawnCard = gcd.gameCards[cnt];
            if (drawnCard.cardLocation == CS.CAR_LOCATION_RED_DECK)
                gcdRed.gameCards.Add(drawnCard);
            else if (drawnCard.cardLocation == CS.CAR_LOCATION_BLUE_DECK) gcdBlue.gameCards.Add(drawnCard);
        }

        //Save the decks that have been created
        filePath = Application.persistentDataPath + "/gamecarddeck1.xml";
        gcd.Save(filePath);
        Debug.Log("Wrote : " + filePath);

        filePath = Application.persistentDataPath + "/gamecarddeckblue.xml";
        gcdBlue.Save(filePath);
        Debug.Log("Wrote : " + filePath);

        filePath = Application.persistentDataPath + "/gamecarddeckred.xml";
        gcdRed.Save(filePath);
        Debug.Log("Wrote : " + filePath);
    }
    */
    /*
    private void UpdateDeckCardStatus(string cardID, string clientID)
    {
        var pc = new GameCard(); 
        var gc = new GameCard();
        bool isPlayableCard = false;
        for (var playerCnt = 0; playerCnt < clients.Count; playerCnt++)
        for (var cardNum = 0; cardNum < CS.CSCARDHANDDIM; cardNum++)
        {
            pc = gcd.gameCards[playerCnt * CS.CSCARDHANDDIM + cardNum];
            
            //Check to see if the cardID is correctly associated with the ClientID
            //TODO - To make this more secure clients should only know their (and only their own) clientID
            if ((pc.cardID == cardID) && (pc.cardClientID == clientID))
            {
                foreach (var cwp in pc.cardWhenPlayable)
                {
                    if ((cwp.turnStage == CS.CWP_PLAY_ANY_TURN)
                        && (cwp.numTimes > 0))
                    {
                        isPlayableCard = true; cwp.numTimes--;
                    }

                    if ((cwp.turnStage == CS.CWP_PLAY_PLAYER_TURN)
                        && isRedTurn
                        && (pc.cardSuit == CS.RED_TEAM)
                        && (cwp.numTimes > 0))
                    {
                        isPlayableCard = true; cwp.numTimes--;
                    }

                    if ((cwp.turnStage == CS.CWP_PLAY_PLAYER_TURN)
                        && !isRedTurn
                        && (pc.cardSuit == CS.BLUE_TEAM)
                        && (cwp.numTimes > 0))
                    {
                        isPlayableCard = true; cwp.numTimes--;
                    }
                }

                if (isPlayableCard)
                {
                    foreach (var cep in pc.cardEffectPlayable)
                    {
                        if (cep.affectWhat == CS.CEP_AFFECT_GAMEBOARD)
                        {
                            if ((cep.effectName == CS.CEP_EFFECT_REVEAL_CARD)
                                && (cep.numTimes > 0))
                            {
                                Debug.Log("Played: " + CS.CEP_EFFECT_REVEAL_CARD);
                                
                            }

                            if ((cep.effectName == CS.CEP_EFFECT_CHANGE_CARD)
                                && (cep.numTimes > 0))
                                
                            {
                                Debug.Log("Played: " + CS.CEP_EFFECT_CHANGE_CARD);
                            }

                            if ((cep.effectName == CS.CEP_EFFECT_REMOVE_CARD)
                                && (cep.numTimes > 0))
                            {
                                Debug.Log("Played: " + CS.CEP_EFFECT_REMOVE_CARD);
                            }
                            if (cep.numTimes > 0) cep.numTimes--;
                        }
                    } 
                }
                
                //create a new card
                //gc = makeCard(playerCnt);

                //now update card as having been played
                gcd.gameCards[playerCnt * CS.CSCARDHANDDIM + cardNum].cardRevealed = CS.CAR_REVEAL_SHOWN;
            }
        }
        SaveDeck();
    }
    */
    /*
    private GameCard makeCard(int playerCnt)
    {
        //create a card
        var gc = new GameCard();
        gc.cardID = Guid.NewGuid().ToString();
        gc.cardPlayerNum = playerCnt.ToString();
        gc.cardClientID = clients[playerCnt].clientID;
        if (clients[playerCnt].isRedTeam)
        {
            gc.cardSuit = CS.RED_TEAM;
            gc.cardLocation = CS.CAR_LOCATION_RED_DECK;
        }
        else if (!clients[playerCnt].isRedTeam)
        {
            gc.cardSuit = CS.BLUE_TEAM;
            gc.cardLocation = CS.CAR_LOCATION_BLUE_DECK;
        }

        gc.cardRevealed = CS.CAR_REVEAL_HIDDEN;
        //add CardWhenPlayable element to card 
        var cwp = new CardWhenPlayable();
        cwp.turnStage = CS.CWP_PLAY_PLAYER_TURN;
        cwp.numTimes = 1;
        gc.cardWhenPlayable.Add(cwp);
        //add CardEffectPlayable element to card 
        var cep = new CardEffectPlayable();
        //randomly choose an effect
        var rnd = Random.Range(0, CS.CEP_EFFECTS.Length);
        cep.effectName = CS.CEP_EFFECTS[rnd];
        cep.affectWhat = CS.CEP_AFFECT_GAMEBOARD;
        cep.numTimes = 1;
        gc.cardEffectPlayable.Add(cep);
        return gc;
    }
    */
}

public class ServerClient
{
    public string clientID;
    public string clientName;
    public bool isHost;
    public bool isPlayer;
    public bool isRedTeam;
    public TcpClient tcp;

    public ServerClient(TcpClient tcp)
    {
        this.tcp = tcp;
    }
}