using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;

namespace Script
{
    public class Server : MonoBehaviour
    {
        public static List<ServerClient> clients;
        private static List<ServerClient> disconnectList;

        //the decks that will be used in the game
        private readonly GameHandDeck gcd = new GameHandDeck();
        private readonly GameHandDeck gcdBlue = new GameHandDeck();
        private readonly GameHandDeck gcdRed = new GameHandDeck();
        public int numParticipants;
        //public int GridXDim;
        //public int GridZDim;
        
        private string[] populate;
 
        //tcp port
        public int port = 6321;

        //the actual server
        private TcpListener server;
        private bool serverStarted;

        //the list of words and their assignments
        private string[,] words;

        private GameState gbs;
        
        //which team's turn it is
        //private bool isRedTurn;
        
        //makes class a singleton
        public static Server Instance { set; get; }
        
        public void Init()
        {
            gbs = FindObjectOfType<GameState>();
            
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
                            OnIncomingData(data);
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
           // var gbs = FindObjectOfType<GameState>();

            //Listen for a client connection
            var listener = (TcpListener) ar.AsyncState;

            //Get a list of all the users that are connected, do this before the client connects
            /*
            var Allusers = "";
            foreach (var i in clients)
                Allusers +=
                    "|"
                    + i.clientName + ","
                    + (i.isHost ? 1 : 0) + ","
                    + (i.isPlayer ? 1 : 0) + ","
                    + i.teamID + ","
                    + i.clientID;
                    */

            //If a client connection occurs add it to the list of clients
            var sc = new ServerClient(listener.EndAcceptTcpClient(ar));
            clients.Add(sc);
            
            //And now add it to the game
            //First create a new player
            /*
            var c = new TeamPlayer();
            c.id = sc.clientID;
            */
            
            //Now add the player to team, at the moment it has no team so place it in a dummy team used for players yet to be assigned a team
            gbs.g.MovePlayerTeam(sc.clientID, CS.NO_TEAM);
            
            /*
            var fTeam =  gbs.g.gameTeams.Where(team => team.id == CS.NO_TEAM);
            if (fTeam.FirstOrDefault() == null)
            {
                var t = new GameTeam();
                t.name = CS.NO_TEAM;
                t.id = CS.NO_TEAM;
                t.teamPlayers.Add(c);
                gbs.g.gameTeams.Add(t);
            }
            else
            {
                //gClientGame.gameTeam.Where(team => team.name == tempTeamID).FirstOrDefault().teamPlayers.Add(c);
                fTeam.FirstOrDefault().teamPlayers.Add(c);
            }
            */
            
            //Once a connection occurs the listener will stop, so if you want to listen for more clients you need to restart listening again.
            startListening();

            Debug.Log("Somebody has connected. Starting to listen for any other clients");

            //Ask last person that connected to state who they are
            //Build the message
            GameMessage gOutgoingMessage = new GameMessage();
            //Message Header
            gOutgoingMessage.id = CS.MESSAGE_SWHO;
            gOutgoingMessage.name = gOutgoingMessage.id;
            gOutgoingMessage.type = CS.MESSAGE_COMMAND;
            
            //Message Sender
            gOutgoingMessage.sender.id = CS.SERVER_ID;
            gOutgoingMessage.sender.name = CS.SERVER_ID;
            
            //Message Receiver
            gOutgoingMessage.receiver.id = sc.clientID;
            
            //Broadcast the message
            //Broadcast("SWHO" + gbs.g.SaveToText(), clients[clients.Count - 1]);
            Broadcast(gOutgoingMessage.SaveToText().Replace(Environment.NewLine, ""), clients[clients.Count - 1]);
            
            //Finally Add Message to the list of events in the game
            gbs.g.GameMessages.Add(gOutgoingMessage);
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
                    Debug.Log("Server Sending To:" + sc.clientID + " => " + data);
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

        private void Broadcast(GameMessage data, List<ServerClient> cl)
        {
            var strdata = data.SaveToText().Replace(Environment.NewLine, "");
            foreach (var sc in cl)
                try
                {
                    Debug.Log("Server Sending To:" + sc.clientID + " => " + strdata);
                    var writer = new StreamWriter(sc.tcp.GetStream());
                    //this has been replace from data to xml
                    writer.WriteLine(strdata);
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

        public void Broadcast(GameMessage data, ServerClient c)
        {
            var sc = new List<ServerClient> {c};
            Broadcast(data,sc);
        }
        
        //Server read
        public void OnIncomingData(string data)
        {
            Debug.Log("Server Receiving: " + data);

            //var gbs = FindObjectOfType<GameState>();
        
            //parse the incoming data stream
            var aData = data.Split('|');

            int x = 0;
            int z = 0;
        
            var distPosX = "";
            var distPosZ = "";
            var distWords = "";
            var distPopulate = "";
            var distReveal = "";
            var distCardID = "";
        
            //used for messaging
            GameMessage gIncomingMessage = new GameMessage();
            gIncomingMessage = GameMessage.LoadFromText(data);
            GameMessage gOutgoingMessage = new GameMessage();
            
            //used as temporary stores for found players and teams in the server
            TeamPlayer fPlayer = new TeamPlayer();
            GameTeam fTeam = new GameTeam();
            
            //used to refer to players and teams in incoming messages
            TeamPlayer gIncomingPlayer = new TeamPlayer();
            GameTeam gIncomingTeam = new GameTeam();
            
            //First Add Message to the list of events in the game
            gbs.g.GameMessages.Add(gIncomingMessage);
            
            // find the player who sent the message in the server's list of players
            gbs.g.FindPlayer(gIncomingMessage.sender.id, ref fPlayer);
                    
            // find the player's details in the message received
            gIncomingMessage.FindPlayer(gIncomingMessage.sender.id, ref gIncomingPlayer);

            //Now decide what to do with the stored message
            switch (gIncomingMessage.name)
            {
                case CS.MESSAGE_CWHO:

                    //If a host request is made reset the game and the list of connections
                    if (fPlayer != null && gIncomingPlayer.isHost)
                    {
                        //update player parameters
                        fPlayer.isHost = gIncomingPlayer.isHost;

                        //update game parameters                                
                        gbs.g.gameParameters.howManyPlaying = gIncomingMessage.gameParameters.howManyPlaying;
                        gbs.g.gameParameters.howManyTeams = gIncomingMessage.gameParameters.howManyTeams;
                        gbs.g.gameParameters.howManyCallers = gIncomingMessage.gameParameters.howManyCallers;

                        gbs.g.gameParameters.sizeOfXDim = gIncomingMessage.gameParameters.sizeOfXDim;
                        gbs.g.gameParameters.sizeOfYDim = gIncomingMessage.gameParameters.sizeOfYDim;

                        populate = new string[gbs.g.gameParameters.sizeOfXDim *
                                              gbs.g.gameParameters.sizeOfYDim];
                        words = new string[2,
                            gbs.g.gameParameters.sizeOfXDim * gbs.g.gameParameters.sizeOfYDim];

                        //clear all other client's connections
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
                        
                        //clear teams
                        gbs.g.gameTeams.Clear();
                    
                        //add in team for players not yet assigned
                        var noTeam = new GameTeam
                        {
                            id = CS.NO_TEAM,
                            name = CS.NO_TEAM
                        };
                        gbs.g.gameTeams.Add(noTeam);
                    }

                    // add the new client details to the list of clients, remember that it will always be the last client in the list of clients that we do not have the details for
                    clients[clients.Count - 1].clientID = gIncomingPlayer.id;
                   
                    //Add the new client details to the team of players that are unassigned team
                    foreach (var client in clients)
                    {
                        var tmpTeamPlayer= new TeamPlayer
                        {
                            id = gIncomingPlayer.id,
                            name = gIncomingPlayer.name,
                            isHost = gIncomingPlayer.isHost,
                            isPlayer = gIncomingPlayer.isPlayer
                        };
                        gbs.g.gameTeams.FirstOrDefault(gt => gt.id == CS.NO_TEAM).teamPlayers.Add(tmpTeamPlayer);
                    }
                    
                    //add in remaining teams, note they will have no players
                    for (var cntTeams = 0; cntTeams < gbs.g.gameParameters.howManyTeams; cntTeams++)
                    {
                        var tmpTeam = new GameTeam
                        {
                            id = UnityEngine.Random.Range(0, 99999).ToString()
                        };
                        //todo - remove hardcoding of only 2 teams
                        if (cntTeams == 0)
                        {
                            tmpTeam.name = CS.RED_TEAM;
                        }
                        else
                        {
                            tmpTeam.name = CS.BLUE_TEAM;
                        }
                        gbs.g.gameTeams.Add(tmpTeam);
                    }

                    //Respond by telling all clients there is a new list of clients
                    //Build the message
                    //Message Header
                    gOutgoingMessage.id = CS.MESSAGE_SCNN;
                    gOutgoingMessage.name = gOutgoingMessage.id;
                    gOutgoingMessage.type = CS.MESSAGE_EVENT;

                    //Message Sender
                    gOutgoingMessage.sender.id = CS.SERVER_ID;
                    gOutgoingMessage.sender.name = CS.SERVER_NAME;

                    //Message Details
                    gOutgoingMessage.gameParameters = gbs.g.gameParameters;
                    gOutgoingMessage.gameTeams = gbs.g.gameTeams;

                    Broadcast(gOutgoingMessage, clients);
                    break;
                case "CMOV":
                    //Currently all validation for the gameboard move is done client side
                    gbs.Incoming(gIncomingMessage.name);
                
                    string strValidGameboardMove = gbs.UpdateGameboardDeckCardStatus(aData[2], aData[3]) ? "1" : "0";
                
                    Broadcast(
                        "SMOV" + '|'
                               + aData[1] + '|'
                               // + aData[2] + '|'
                               // + aData[3] + '|'
                               + aData[2] + '|'
                               + aData[3] + "|"
                               + strValidGameboardMove,
                        clients
                    );
                    break;
                case "CHAN":
                    //Update the server copy of the deck marking the card as having been played
                    //Send message to GameBoardState

                    string strValidHandMove = gbs.UpdateHandDeckCardStatus(aData[2], aData[3]) ? "1" : "0";
                    Broadcast(
                        "SHAN" + '|'
                               + aData[1] + '|'
                               + aData[2] + '|'
                               + aData[3] + '|'
                               + strValidHandMove,
                        clients
                    );
                    break;
                case CS.MESSAGE_CDIC:
                    //Send message to GameBoardState, this will populate the WordList
                    /*
                    gbs.Incoming(aData);
                                        
                    distWords = "";
                    distPopulate = "";
                    distCardID = "";
    
                    //Cannot use FindObjectOfType in the constructor, so have to assign in here    
                    var worddictionary = FindObjectOfType<WordDictionary>();
                
                    //worddictionary.buildGameboard();


                    foreach (var pair in worddictionary.gameBoardCardData)
                    {
                        distWords += pair.Value.wordList + ",";
                        distPopulate += pair.Value.populate + ",";
                        distCardID += pair.Value.cardid + ",";
                    }
                    
                    Broadcast(
                        "SDIC" + '|'
                               + worddictionary.isRedStart + '|'
                               + distWords + '|'
                               + distPopulate + '|'
                               + distCardID,
                        clients
                    );
                    */
                    
                    //Send message to GameBoardState, this will populate the WordList
                    gbs.Incoming(gIncomingMessage.name);

                    //Respond by telling all clients the dictionary to use
                    //build the dictionary
                    
                    /*
                    //TODO - Improve the efficiency of how you popolate the card details dictionary should really be doing this not the server

                    //Cannot use FindObjectOfType in the constructor, so have to assign in here    
                    var worddictionary = FindObjectOfType<WordDictionary>();
                  
                    //TODO - Rewmove hardcodng for only one gameboard
                    var cnt = 0;
                    var gbd = new GameBoardDeck();
                    gbs.g.GameBoardDecks.Add(gbd);
                    GameBoardDeck gbdFirstOrDefault = gbs.g.GameBoardDecks.FirstOrDefault();
                    
                    //Populate the deck
                    foreach (var word in worddictionary.gameBoardCardData)
                    {
                        var card = new GameCard();
                        card.id = word.Value.cardid;
                        card.cardSuit = word.Value.populate;
                        card.cardWord = word.Value.wordList;
                        gbdFirstOrDefault.gameCards.Add(card);
                    }
                    
                    */
                    
                    //Build the message
                    //Message Header
                    gOutgoingMessage.id = CS.MESSAGE_SDIC;
                    gOutgoingMessage.name = gOutgoingMessage.id;
                    gOutgoingMessage.type = CS.MESSAGE_REPLY;

                    //Message Sender
                    gOutgoingMessage.sender.id = CS.SERVER_ID;
                    gOutgoingMessage.sender.name = CS.SERVER_NAME;

                    //Message Details
                    gOutgoingMessage.gameParameters = gbs.g.gameParameters;
                    gOutgoingMessage.gameTeams = gbs.g.gameTeams;
                    gOutgoingMessage.GameBoardDecks = gbs.g.GameBoardDecks;
                    
                    Broadcast(gOutgoingMessage.SaveToText().Replace(Environment.NewLine, ""), clients);
                      
                    break;
                case "CKEY":
                    gbs.Incoming(gIncomingMessage.name);
                    var tmpVal = aData[2].ToUpper();
                    Broadcast(
                        "SKEY" + '|'
                               + tmpVal,
                        clients
                    );
                    break;
                case "CBEG":
                    //replace all teams
                    gbs.g.gameTeams = gIncomingMessage.gameTeams;
                    
                    //Respond by telling all clients the updated details for all players of all teams
                    //Build the message
                    //Message Header
                    gOutgoingMessage.id = CS.MESSAGE_SBEG;
                    gOutgoingMessage.name = gOutgoingMessage.id;
                    gOutgoingMessage.type = CS.MESSAGE_EVENT;

                    //Message Sender
                    gOutgoingMessage.sender.id = CS.SERVER_ID;
                    gOutgoingMessage.sender.name = CS.SERVER_NAME;

                    //Message Details
                    gOutgoingMessage.gameParameters = gbs.g.gameParameters;
                    gOutgoingMessage.gameTeams = gbs.g.gameTeams;
                    
                    Broadcast(gOutgoingMessage.SaveToText().Replace(Environment.NewLine, ""), clients);
                    break;
                case "CPCC":
                    //Send message to GameBoardState
                    gbs.Incoming(gIncomingMessage.name);
                    Broadcast(gbs.ghd.SaveToText().Replace(Environment.NewLine, ""), clients);
                    break;
                case "CPCU":
                    //Send message to GameBoardState
                    gbs.Incoming(gIncomingMessage.name);
                    Broadcast(gbs.ghd.SaveToText().Replace(Environment.NewLine, ""), clients);
                    break;
            
                //Hand Card Affects on the board
                case "CGFU":

                    distPosX = "";
                    distPosZ = "";
                    distWords = "";
                    distPopulate = "";
                    distReveal = "";
               
                    for (z = 0; z < gbs.g.gameParameters.sizeOfYDim; z++)
                    for (x = 0; x < gbs.g.gameParameters.sizeOfXDim; x++)
                    {

                        //implement additional attributes
                        distPosX += gbs.gbd.gameCards[x + z * gbs.g.gameParameters.sizeOfXDim].cardXPos + ",";
                        distPosZ += gbs.gbd.gameCards[x + z * gbs.g.gameParameters.sizeOfXDim].cardZPos + ",";
                        distWords += gbs.gbd.gameCards[x + z * gbs.g.gameParameters.sizeOfXDim].cardWord + ",";
                        distPopulate += gbs.gbd.gameCards[x + z * gbs.g.gameParameters.sizeOfXDim].cardSuit +",";
                        distReveal += gbs.gbd.gameCards[x + z * gbs.g.gameParameters.sizeOfXDim].cardRevealed +",";
                        distCardID += gbs.gbd.gameCards[x + z * gbs.g.gameParameters.sizeOfXDim].cardID + ",";
                    }

                    Broadcast(
                        "SGFU" + '|'
                               + GameState.isRedTurn + '|'
                               + distWords.Remove(distWords.LastIndexOf(',')) + '|'
                               + distPopulate.Remove(distPopulate.LastIndexOf(',')) + '|'
                               + distPosX.Remove(distPosX.LastIndexOf(',')) + '|'
                               + distPosZ.Remove(distPosZ.LastIndexOf(',')) + '|'
                               + distReveal.Remove(distReveal.LastIndexOf(',')) +'|'
                               + distCardID.Remove(distCardID.LastIndexOf(',')),
                        clients
                    );
                    break;
            }
        }

        private string GetStringOfAllClients()
        {
            var concatClients = "";
            for (var i = 0; i < clients.Count; i++)
            {
                concatClients += "|"
                                 // + clients[i].clientName + ","
                                 // + (clients[i].isHost ? 1 : 0) + ","
                                 // + (clients[i].isPlayer ? 1 : 0) + ","
                                 // + clients[i].teamID + ","
                                 + clients[i].clientID + ",";

                // TODO - Change this it is not the easiest to understand code
                // this is a "magic field" that tells the client it should add the participant and start the lobby
                // it is placed on the last entry of the message sent to the client
                // Really this should be another message embedded in this message
                
                if (numParticipants != 0 && clients.Count == numParticipants && i == clients.Count - 1)
                    concatClients += numParticipants.ToString() + ",";
                else
                    concatClients += "0" + ",";
                
                if (gbs.g.gameParameters.sizeOfXDim != 0 && clients.Count == numParticipants && i == clients.Count - 1)
                    concatClients += gbs.g.gameParameters.sizeOfXDim.ToString() + ",";
                else
                    concatClients += "0" + ",";
                
                if (gbs.g.gameParameters.sizeOfYDim != 0 && clients.Count == numParticipants && i == clients.Count - 1)
                    concatClients += gbs.g.gameParameters.sizeOfYDim.ToString();
                else
                    concatClients += "0";
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
    }

    public class ServerClient
    {
        public string clientID;
        //public string clientName;
        //public bool isHost;
        //public bool isPlayer;
        //public string teamID;
        public TcpClient tcp;

        public ServerClient(TcpClient tcp)
        {
            this.tcp = tcp;
            clientID = tcp.GetHashCode().ToString();
        }
    }
}