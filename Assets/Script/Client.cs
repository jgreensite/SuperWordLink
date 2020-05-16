using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

namespace Script
{
    public class Client : MonoBehaviour
    {
        public string clientID;
        public string clientName;

        //the decks that will be used in the game
        public GameHandDeck gcd = new GameHandDeck();
        public GameHandDeck gcdBlue = new GameHandDeck();
        public GameHandDeck gcdRed = new GameHandDeck();
        private GameHandDeck gcdTemp = new GameHandDeck();
        
        //Client's Current View of
        private Game gClientGame = new Game();
        public GameTeam gClientTeam = new GameTeam();
        public TeamPlayer gClientPlayer = new TeamPlayer();

        //public bool isHost;
        //public bool isPlayer;
        //public bool isRedTeam;
        //private int numParticipants;

        public List<GameClient> players = new List<GameClient>();
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


            //TODO - remove this when all messaging is done via XML
            if (!data[0].Equals('<'))
            {
                //Use these for temporary messages
                Game gEventGame = new Game();
                GameEvent gEventPlayer = new GameEvent();
                
                var aData = data.Split('|');
                switch (aData[0])
                {
                    case "SWHO":
                        players.Clear();
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

                        Send(
                            "CWHO" + '|'
                                   + gClientPlayer.name + '|'
                                   + (gClientPlayer.isHost ? 1 : 0) + '|'
                                   + (gClientPlayer.isPlayer ? 1 : 0) + '|'
                                   + (gClientTeam.id) + '|'
                                   + gClientPlayer.id + '|'
                                   + howManyPlaying + '|'
                                   + sizeOfXDim + '|'
                                   + sizeOfYDim

                        );
                        //START HERE Sending the new message back
                        //This message is probably complete but it highlights the need to get rid of
                        //the class client.gameClient and replace with gamePlayer
                        
                        //The Event
                        gEventPlayer.id = "1";
                        gEventPlayer.name = "CWHO";

                        //Add The Team & Player Information to The Event
                        gEventGame.gameTeam.Add(gClientTeam);

                        //Add The Game Information to the Event
                        gEventGame.howManyPlaying = gClientGame.howManyPlaying;
                        gEventGame.sizeOfXDim = gClientGame.sizeOfXDim;
                        gEventGame.sizeOfYDim = gClientGame.sizeOfYDim;

                        break;

                    case "SCNN":
                        players.Clear();
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
                        players.Clear();
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
            else if(1==1)
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
            else if (1==0)
            {
                Game gTemp = new Game();
                gTemp = Game.LoadFromText(data);
                if (!gClientGame.SaveToText().Equals(gTemp.SaveToText()))
                {
                    // If there are differences render the deck
                    gClientGame = Game.LoadFromText(data);
                    GameBoard.Instance.GeneratePlayerHand();
                }
            }
        }

        //Called when a message is received that a user has connected
        private void UserConnected(string name, bool isHost, bool isPlayer, bool isRedTeam, string clientID)
        {
            var c = new GameClient();
            c.name = name;
            c.isHost = isHost;
            c.isPlayer = isPlayer;
            c.isRedTeam = isRedTeam;
            c.id = clientID;

            players.Add(c);
            //TODO - Update the panel message to say "waiting for host to choose teams"
        }

        public void StartGame()
        {
            var concatPlayers = "";

            for (var cnt = 0; cnt < players.Count; cnt++)
                concatPlayers += "|"
                                 + players[cnt].name + ","
                                 + (players[cnt].isPlayer ? 1 : 0) + ","
                                 + (players[cnt].isRedTeam ? 1 : 0) + ","
                                 + players[cnt].id;

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