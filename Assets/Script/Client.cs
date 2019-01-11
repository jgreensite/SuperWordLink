using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using AssemblyCSharp;
using UnityEngine;

public class Client : MonoBehaviour
{
    public string clientID;
    public string clientName;

    //the decks that will be used in the game
    public GameHandDeck gcd = new GameHandDeck();
    public GameHandDeck gcdBlue = new GameHandDeck();
    public GameHandDeck gcdRed = new GameHandDeck();
    private GameHandDeck gcdTemp = new GameHandDeck();
    public bool isHost;
    public bool isPlayer;
    public bool isRedTeam;
    private int numParticipants;

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
        int howManyPlaying;
        var cardID = "";


        //TODO - remove this when all messaging is done via XML
        if (!data[0].Equals('<'))
        {
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

                    //Get the number of participants from the GameManager and convet to an integer
                    //Note that cannot write the number of participants that is used to decide to start the game
                    //this must be initiated from the server sending a message to the client
                    if (int.TryParse(GameManager.Instance.minPlayers.text, out howManyPlaying))
                    {
                    }
                    else if (isHost && GameManager.Instance.minPlayers.text == "")
                    {
                        howManyPlaying = 1;
                    }
                    else
                    {
                        howManyPlaying = 0;
                    }

                    Send(
                        "CWHO" + '|'
                               + clientName + '|'
                               + (isHost ? 1 : 0) + '|'
                               + (isPlayer ? 1 : 0) + '|'
                               + (isRedTeam ? 1 : 0) + '|'
                               + clientID + '|'
                               + howManyPlaying
                    );
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

                        //if we get the signal to start the lobby, start it 
                        if (bData[5] != "0") GameManager.Instance.OpenLobby();
                    }

                    break;
                case "SMOV":
                    x = int.Parse(aData[2]);
                    z = int.Parse(aData[3]);
                    GameBoard.Instance.TryGameboardMove(x, z);
                    break;

                case "SHAN":
                    cardID = aData[2];
                    GameBoard.Instance.TryHandMove(cardID);
                    break;

                case "SDIC":
                    GameBoard.Instance.isRedTurn = aData[1] == "0" ? false: true;
                    GameBoard.Instance.isRedStart = aData[1] == "0" ? false: true;

                    var wData = aData[2].Split(',');
                    GameBoard.Instance.words = wData;

                    var pData = aData[3].Split(',');
                    GameBoard.Instance.populate = pData;

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
                        if (string.Equals(bData[4], clientID))
                        {
                            isHost = bData[1] == "0" ? false : true;
                            isPlayer = bData[2] == "0" ? false : true;
                            isRedTeam = bData[3] == "0" ? false : true;
                        }
                    }

                    GameManager.Instance.StartGame();
                    break;
            }
        }
        //Must be XML
        else
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
    }

    //Called when a message is received that a user has connected
    private void UserConnected(string name, bool isHost, bool isPlayer, bool isRedTeam, string clientID)
    {
        var c = new GameClient();
        c.name = name;
        c.isHost = isHost;
        c.isPlayer = isPlayer;
        c.isRedTeam = isRedTeam;
        c.clientID = clientID;

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
                             + players[cnt].clientID;

        Send(
            "CBEG" + "|"
                   + clientName
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
                   + clientName + '|'
                   + (isHost ? 1 : 0) + '|'
                   + (isPlayer ? 1 : 0) + '|'
                   + (isRedTeam ? 1 : 0) + '|'
                   + clientID + '|'
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
    public string clientID;
    public bool isHost;
    public bool isPlayer;
    public bool isRedTeam;
    public string name;
}