using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
//using MoreLinq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Script
{
    public class Client : MonoBehaviour
    {
 //       public string clientID;
 //       public string clientName;

        //the decks that will be used in the game
        public GameHandDeck gcd = new GameHandDeck();
        public GameHandDeck gcdBlue = new GameHandDeck();
        public GameHandDeck gcdRed = new GameHandDeck();
        private GameHandDeck gcdTemp = new GameHandDeck();
        
        //Client's Current View of
        public Game gClientGame = new Game();
        public GameTeam gClientTeam = new GameTeam();
        public TeamPlayer gClientPlayer = new TeamPlayer();

        //public bool isHost;
        //public bool isPlayer;
        //public bool isRedTeam;
        //private int numParticipants;

        //public List<GameClient> players = new List<GameClient>();
        private StreamReader reader;
        private TcpClient socket;

        private bool socketReady;
        private NetworkStream stream;
        private StreamWriter writer;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            GameManager.Instance.goDontDestroyList.Add(gameObject);
            Debug.Log("Added Client at position:" + GameManager.Instance.goDontDestroyList.Count + " to donotdestroylist");
            gClientTeam.teamPlayers.Add(gClientPlayer);
        }

        public bool ConnectToServer(string host, int port)
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
                Debug.Log("Socket Error Client: " + e.Message);
            }

            return socketReady;
        }

        private void Update()
        {
            if (socketReady)
                if (stream.DataAvailable)
                {
                    var data = reader.ReadLine();
                    if (data != null)
                        OnIncomingData(data);
                }
        }

        //Sending messages to the Server
        public void Send(string data)
        {
            if (!socketReady) return;
            Debug.Log("Client Sending:" + data);
            writer.WriteLine(data);
            writer.Flush();
        }

        public void Send(GameMessage data)
        {
            Send(data.SaveToText().Replace(Environment.NewLine, ""));
        }

        //Reading messages from the Server
        private void OnIncomingData(string data)
        {
            var x = 0;
            var z = 0;
            Debug.Log("Client Receiving: " + data);
            int howManyPlaying =0;
            int sizeOfXDim =0;
            int sizeOfYDim =0;
            var cardID = "";

            //Use these for temporary messages
            Game gEventGame = new Game();
            GameMessage gMessagePlayer = new GameMessage();
            
            //TODO - remove this when all messaging is done via XML
            if (!data[0].Equals('<'))
            {

                
                var aData = data.Split('|');
                switch (aData[0])
                {

                    case "SCNN":
                        gClientGame.gameTeam.Clear();
                        for (var i = 1; i < aData.Length; i++)
                        {
                            var bData = aData[i].Split(',');
                            UserConnected(
                                bData[0],
                                bData[1] == "0" ? false : true,
                                bData[2] == "0" ? false : true,
                                bData[3] == "0" ? false : true,
                                bData[4]
                            );

                            //if we get the signal to start the lobby, start it addind the constants set by whichever player isHost
                            if (bData[5] != "0")
                            {
                                GameManager.Instance.gridXDim.text =  bData[6];
                                GameManager.Instance.gridZDim.text =  bData[7];
                                GameManager.Instance.OpenLobby();
                            }
                        }

                        break;
                    case "SMOV":
//                        x = int.Parse(aData[2]);
//                        z = int.Parse(aData[3]);
                        cardID = aData[2];
                        if (aData[4] == "1")
                        {
//                          GameBoard.Instance.TryGameboardMove(x, z, cardID);
                            GameBoard.Instance.TryGameboardMove(cardID);
                        } else{
                            Debug.Log("Invalid Move attempted by " + aData[1]);
                        }
                        break;

                    case "SHAN":
                        cardID = aData[2];
                        if (aData[4] == "1")
                        {
                            GameBoard.Instance.TryHandMove(cardID);
                        } else {
                            Debug.Log("Invalid Move attempted by " + aData[1]);    
                        }
                        break;

                    case "SDIC":
                        GameBoard.Instance.isRedTurn = aData[1] == "0" ? false: true;
                        GameBoard.Instance.isRedStart = aData[1] == "0" ? false: true;

                        var wData = aData[2].Split(',');
                        GameBoard.Instance.words = wData;

                        var pData = aData[3].Split(',');
                        GameBoard.Instance.populate = pData;

                        var iData = aData[4].Split(',');
                        GameBoard.Instance.cardids = iData;
                    
                        GameBoard.Instance.GeneratePlayerGameboard();

                        GetGameCardDeck(CS.CREATE);
                        break;
                    case "SKEY":
                    {
                        switch (aData[1])
                        {
                            case "R":
                                //need to get rid of the local gameCard copy if we receive a restart. When the scene loads we need to make sure we load a new set of cards
                                gcd.gameCards.RemoveAll(gameCards => gameCards.cardID != "");
                                GameBoard.Instance.ResartGame();
                                break;

                            case "P":
                                GameBoard.Instance.SetCamera(aData[1]);
                                break;
                                ;
                            case "C":
                                GameBoard.Instance.SetCamera(aData[1]);
                                break;
                        }
                    }
                        break;
                    case "SBEG":
                        gClientGame.gameTeam.Clear();
                        for (var i = 1; i < aData.Length; i++)
                        {
                            var bData = aData[i].Split(',');
                            UserConnected(
                                bData[0],
                                bData[1] == "0" ? false : true,
                                bData[2] == "0" ? false : true,
                                bData[3] == "0" ? false : true,
                                bData[4]
                            );

                            //Populate the client attributes
                            if (string.Equals(bData[4], gClientPlayer.id))
                            {
                                gClientPlayer.isHost = bData[1] == "0" ? false : true;
                                gClientPlayer.isPlayer = bData[2] == "0" ? false : true;
                                gClientTeam.id = bData[3];
                                gClientTeam.name = gClientTeam.id;
                            }
                        }

                        GameManager.Instance.StartGame();
                        break;
                    case"SGFU":
                        //Forced client update of Gameboard from server
                        GameBoard.Instance.isRedTurn = aData[1] == "False" ? false: true;

                        wData = aData[2].Split(',');
                        GameBoard.Instance.words = wData;

                        pData = aData[3].Split(',');
                        GameBoard.Instance.populate = pData;

                        var xData = aData[4].Split(',').Select(s => Int32.Parse(s)).ToArray();
                        GameBoard.Instance.xPos = xData;
                    
                        var zData = aData[5].Split(',').Select(s => Int32.Parse(s)).ToArray();;
                        GameBoard.Instance.zPos = zData;
                    
                        var rData = aData[6].Split(',');                   
                        GameBoard.Instance.reveal = rData;

                        GameBoard.Instance.UpdatePlayerGameboard();
                        break;
                }
            }
            //Must be XML
            //This section is harccoded for only dealing with decks, it is the first use of XML messaging and eventually must be removed
            else if(1==0)
            {
                //TODO - Assumes that the xml message is one to populate a game card deck
                gcdTemp = GameHandDeck.LoadFromText(data);
                if (!gcdTemp.SaveToText().Equals(gcd.SaveToText()))
                {
                    gcd = GameHandDeck.LoadFromText(data);
                    //TODO - Assumes only a red and blue deck
                    gcdRed = new GameHandDeck();
                    gcdBlue = new GameHandDeck();
                    for (var cnt = 0; cnt < gcd.gameCards.Count; cnt++)
                    {
                        var gc = gcd.gameCards[cnt];
                        if (gc.cardSuit == CS.RED_TEAM)
                            gcdRed.gameCards.Add(gc);
                        else if (gc.cardSuit == CS.BLUE_TEAM) gcdBlue.gameCards.Add(gc);
                    }

                    // If there are differences render the deck
                    GameBoard.Instance.GeneratePlayerHand();
                }
            }
            else if (1==1)
            {
                GameMessage gIncomingMessage = new GameMessage();
                GameMessage gOutgoingMessage = new GameMessage();
                gIncomingMessage = GameMessage.LoadFromText(data);
                switch (gIncomingMessage.name)
                {
                    case "SWHO":
                        //gClientGame.gameTeam.Clear();
                        
                        //This message sets the ClientID from the server
                        gClientPlayer.id = gIncomingMessage.receiver.id;

                        foreach (var cntT in gIncomingMessage.gameTeam)
                        {
                            var fPlayer =  cntT.teamPlayers.Where(player => player.id == gClientPlayer.id);
                            if (fPlayer.FirstOrDefault() == null)
                                gClientTeam.id = cntT.id;
                                gClientTeam.name = cntT.name;
                        }

                        //Hosts will use their preferences in order to determine the game parameters e.g. number of players, board dimentions
                        //Note that cannot write the number of participants that is used to decide to start the game
                        //this must be initiated from the server sending a message to the client

                        if (gClientPlayer.isHost)    
                        {
                            howManyPlaying = PREFS.getPrefInt("MinPlayers");
                            sizeOfXDim = PREFS.getPrefInt("GridXDim");
                            sizeOfYDim = PREFS.getPrefInt("GridZDim");
                        }
                        else
                        {
                            howManyPlaying = 0;
                            sizeOfXDim = 0;
                            sizeOfYDim = 0;
                        }
                        //Respond the ask for more details about the player
                        //Build the message
                        //Message Header
                        gOutgoingMessage.id = CS.MESSAGE_CWHO;
                        gOutgoingMessage.name = gOutgoingMessage.id;
                        gOutgoingMessage.type = CS.MESSAGE_REPLY;
                        
                        //Message Sender
                        gOutgoingMessage.sender.id = gClientPlayer.id;
                        gOutgoingMessage.sender.name = gClientPlayer.name;
                        
                        //Message Details
                        gOutgoingMessage.gameParameters.howManyPlaying = howManyPlaying;
                        gOutgoingMessage.gameParameters.sizeOfXDim = sizeOfXDim;
                        gOutgoingMessage.gameParameters.sizeOfYDim = sizeOfYDim;
                        
                        gOutgoingMessage.gameTeam.Clear();
                        gOutgoingMessage.gameTeam.Add(gClientTeam);
                        
                        Send(gOutgoingMessage);
                        break;
                }
            }
        }

        //Called when a message is received that a user has connected
        private void UserConnected(string name, bool isHost, bool isPlayer, bool isRedTeam, string clientID)
        {
            //var c = new GameClient();
            //c.name = name;
            //c.isHost = isHost;
            //c.isPlayer = isPlayer;
            //c.isRedTeam = isRedTeam;
            //c.id = clientID;
            
            var c = new TeamPlayer();
            c.name = name;
            c.isHost = isHost;
            c.isPlayer = isPlayer;
            c.id = clientID;
            var tempTeamName = isRedTeam ? CS.RED_TEAM : CS.BLUE_TEAM;
            var tempTeamID = tempTeamName;
            var fTeam = gClientGame.gameTeam.Where(team => team.id == tempTeamID);
            if (fTeam.FirstOrDefault() == null)
            {
                var t = new GameTeam();
                t.name = tempTeamName;
                t.id = tempTeamID;
                t.teamPlayers.Add(c);
                gClientGame.gameTeam.Add(t);
            }
            else
            {
               //gClientGame.gameTeam.Where(team => team.name == tempTeamID).FirstOrDefault().teamPlayers.Add(c);
               fTeam.FirstOrDefault().teamPlayers.Add(c);
            }

            //players.Add(c);
            //TODO - Update the panel message to say "waiting for host to choose teams"
        }

        public void StartGame()
        {
            var concatPlayers = "";
            //todo - Ideally use and extension linq such as more enumerable to write a simpler version of the following nested 'foreach' call
            foreach (var cntT in gClientGame.gameTeam) foreach (var cntP in cntT.teamPlayers)
                concatPlayers += "|"
                                 + cntP.name + ","
                                 + (cntP.isPlayer ? 1 : 0) + ","
                                 + cntT.id + ","
                                 + cntP.id;

            Send(
                "CBEG" + "|"
                       + gClientPlayer.name
                       + concatPlayers
            );
        }

        public void GetGameCardDeck(string action)
        {
            string msg= "";
            switch (action)
            {
                case CS.CREATE:
                    msg = "CPCC";
                    break;
                case CS.END:
                    msg = "CPCU";
                    break;
            }
            Send(
                msg + "|"
                    + gClientPlayer.name  + '|'
                    + (gClientPlayer.isHost ? 1 : 0) + '|'
                    + (gClientPlayer.isPlayer ? 1 : 0) + '|'
                    + gClientTeam.id + '|'
                    + gClientPlayer.id + '|'
            );
        }

        //Called when the application quits, from Monobehaviour
        private void OnApplicationQuit()
        {
            closeSocket();
        }

        //Called when the object is disabled, from Monobehaviour
        private void OnDisable()
        {
            closeSocket();
        }

        //Close socket, especially if there is a crash don't leave the socket open
        private void closeSocket()
        {
            if (!socketReady)
                return;
            writer.Close();
            reader.Close();
            socket.Close();
            socketReady = false;
        }
    }

    public class GameClient
    {
        public string id;
        public bool isHost;
        public bool isPlayer;
        public bool isRedTeam;
        public string name;
    }
}