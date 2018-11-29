﻿using System;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameBoard : MonoBehaviour
{
    public static int gridXDim = CS.CSGRIDXDIM;
    public static int gridZDim = CS.CSGRIDZDIM;
    public static int cardHandDim = CS.CSCARDHANDDIM;
    public GameObject bluePfb;

    private readonly Vector3 boardOffsetLeft = new Vector3(-1.00f, 1.0f, -1.00f);
    public GameObject callerCamera;
    public Card[,] cardsCaller = new Card[gridXDim, gridZDim];
    private string cardSelectedLocation;

    public Card[,] cardsPlayer = new Card[gridXDim, gridZDim];

    //TODO - Remove hardcoding for number of players and cards in each player's hand
    //public List<Card> cardsPlayerHand;
    public Dictionary<string, Card> cardsPlayerHand = new Dictionary<string, Card>();
    public GameObject civilPfb;

    private Client client;
    public int cntBlueCards;
    public int cntBlueHandCards;
    public int cntCivilCards;
    public int cntDeathCards;

    //GameboardCards
    public int cntRedCards;

    //PlayerCards
    public int cntRedHandCards;
    public Text countTextBlue;

    public Text countTextRed;

    public GameObject currentCamera;
    public GameObject deathPfb;

    private GameObject gameBoardCaller;

    //TODO - The vectors are referenced using "x" and "y" properties, better to use arrays with more relevant property names, see how "hand is different to board"
    private Vector2 gameboardCardOver;
    private GameObject gameBoardPlayer;

    //TODO - Remove hardcoding for number of players, this will only work with 3 cards in the hand
    private readonly Vector3 handOffsetLeft = new Vector3(0.0f, 0.0f, -0.5f);
    private readonly Vector3 handOffsetRight = new Vector3((gridXDim + 1) * 0.5f, 0.0f, -0.5f);
    public GameObject handPfb;
    public bool isGameover;

    public bool isRedStart;
    public bool isRedTurn;
    public bool isRestart;

    public GameObject playerCamera;
    private string playerhandCardIDOver;
    public string[] populate = new string[25];

    public GameObject redPfb;

    //used to track selection of board and player cards
    private Card selectedCard;

    private GameObject turnIndicator;
    private TurnIndicator turnIndicatorScript;

    public string[] words = new string[25];

    //makes class a singleton
    public static GameBoard Instance { set; get; }

    private void Start()
    {
        //needed to make this a singleton
        Instance = this;

        client = FindObjectOfType<Client>();

        turnIndicator = GameObject.Find("Turn Indicator");
        turnIndicatorScript = turnIndicator.GetComponent<TurnIndicator>();

        gameBoardCaller = GameObject.Find("Game Board Caller");
        gameBoardPlayer = GameObject.Find("Game Board Player");

        isRestart = false;
        isGameover = false;

        //TODO the dictionary should be the place where the initial cards are dealt
        //Get Dictionary from the server, as opposed to getting it locally
        //Only the host should make this request otherwise we'll generate too many cards as each client will request dictionary
        if (client.isHost)
            client.Send(
                "CDIC" + '|'
                       + client.clientName
            );
        // Get the initial set of Gamecards
    }

    private void Update()
    {
        //TODO - also need a UI button for restarting
        // If the game is over of a restart situation has occured then don't accept anymore input
        var x = 0;
        var z = 0;

        if (Input.GetKeyDown(KeyCode.R))
        {
            client.Send(
                "CKEY" + '|'
                       + client.clientName + '|'
                       + Input.inputString
            );
        }
        else if ((Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.P)) &&
                 (client.isPlayer == false || isGameover))
        {
            SetCamera(Input.inputString.ToUpper());
        }
        else if (isGameover == false)
        {
            UpdateMouseOver();
            //Debug.Log (mouseOver);

            switch (cardSelectedLocation)
            {
                //if chosen a card on the gameboard
                case CS.OBJ_LOCATION_LAYER_GAMEBOARD:
                    x = (int) gameboardCardOver.x;
                    z = (int) gameboardCardOver.y;

//					//TODO - need to add in support for touch screens and cursor keys
//					if (Input.GetMouseButtonDown (0))
//						SelectGameBoardCard (x, z);

                    if (Input.GetMouseButtonUp(0))
                        //Send move to server
                        client.Send(
                            "CMOV" + '|'
                                   + client.clientName + '|'
                                   + x + '|'
                                   + z + '|'
                                   + client.clientID
                        );
                    break;

                //if chosen a card in a player's hand
                case CS.OBJ_LOCATION_LAYER_PLAYERHAND:

                    //TODO - need to add in support for touch screens and cursor keys
//					if (Input.GetMouseButtonDown (0))
//							 (x, z);

                    if (Input.GetMouseButtonUp(0))
                        //Send move to server
                        client.Send(
                            "CHAN" + '|'
                                   + client.clientName + '|'
                                   + playerhandCardIDOver + '|'
                                   + client.clientID
                        );
                    break;
            }
        }
    }

    private void UpdateMouseOver()
    {
        //Check if it is my turn

        //Check if a current camera exists
        if (!currentCamera)
        {
            Debug.Log("Unable to find Current Camera");
            return;
        }

        RaycastHit hitGameBoard;
        RaycastHit hitPlayerCard;

        //Check if clicked on a card in the hand
        if (Physics.Raycast(currentCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition),
            out hitPlayerCard, 25.0f, LayerMask.GetMask(CS.OBJ_LOCATION_LAYER_PLAYERHAND)))
        {
            //TODO colliders on all cards are hardcoded, really should work them out from card geometry
            cardSelectedLocation = CS.OBJ_LOCATION_LAYER_PLAYERHAND;
            playerhandCardIDOver = hitPlayerCard.collider.gameObject.GetComponent<Card>().cardID;
            //Debug.Log("Selected Card in Hand for player:" + playerhandCardOver.x + " at position:" + playerhandCardOver.y);
        }
        //check if clicked on a card on the deck
        else if (Physics.Raycast(currentCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition),
            out hitGameBoard, 25.0f, LayerMask.GetMask(CS.OBJ_LOCATION_LAYER_GAMEBOARD)))
        {
            //TODO - the offset of 1.2 used here are hard coded based on the size of the box collider "real-world" numbers, it should be calculated
            cardSelectedLocation = CS.OBJ_LOCATION_LAYER_GAMEBOARD;
            var gameboardDimx = transform.Find("Game Board Player").localScale.x;
            var gameboardDimz = transform.Find("Game Board Player").localScale.z;
            gameboardCardOver.x = (int) ((hitGameBoard.point.x + gameboardDimx / 2) / (2.45 / gridXDim));
            gameboardCardOver.y = (int) ((hitGameBoard.point.z + gameboardDimz / 2) / (2.35 / gridZDim));
        }
        else
        {
            gameboardCardOver.x = -1;
            gameboardCardOver.y = -1;
            playerhandCardIDOver = "";
            cardSelectedLocation = "";
        }
    }

    private void SelectGameBoardCard(int x, int z)
    {
        //Out of Bounds check
        if (x < 0 || x > gridXDim - 1 || z < 0 || z > gridZDim - 1)
        {
            Debug.Log("Item does not exist in gameboard array at " + x + ", " + z);
            selectedCard = null;
            return;
        }

        var c = cardsPlayer[x, z];
        //cannot flip a card that has been flipped
        if (c != null && !c.isCardUp)
        {
            selectedCard = c;
            Debug.Log("Gameboard Card selected is at position x:" + x + " y:" + z);
        }
        else
        {
            Debug.Log("Error - Gameboard Card is either already selected or out of bounds");
        }
    }

    private void SelectPlayerHandCard(string cardID)
    {
        //Out of Bounds check
        Card c;
        if (!cardsPlayerHand.TryGetValue(cardID, out c))
        {
            Debug.Log("Item does not exist in player hand array " + cardID);
            selectedCard = null;
            return;
        }

        c = cardsPlayerHand[cardID];
        //TODO - There is an implied assumption here that there is no need to check to see if the card in hand has already been selected as used, it's possible to get into this state if the server and client are not in sync or the user hammers that UI clicking like crazy
        if (c != null)
        {
            selectedCard = c;
            selectedCard.isDiscard = true;
            Debug.Log("Player Hand Card selected is player:" + selectedCard.playerNum + " card num:" +
                      selectedCard.cardNum);
        }
        else
        {
            Debug.Log("Error - Player Hand Card is either already selected or out of bounds");
        }
    }

    public void TryGameboardMove(int x, int z)
    {
        string moveResult;

        //Multiplayer support
        //Note that we need to create local variables as if it is not your turn you may not have a selected card defined

        //Select the card, note that it may not be this client that selected the card
        SelectGameBoardCard(x, z);

//		//Check if we are out of bounds
//		if ((x < 0) || (x > (gridXDim-1)) || (z < 0) || (z > (gridZDim-1)))
//		{
//			selectedCard = null;
//		}
//		else
//		{
        if (selectedCard != null)
        {
            moveResult = selectedCard.ValidMove(isRedTurn);
            switch (moveResult)
            {
                case CS.GOOD:
                    //Flip the Card and keep on picking
                    selectedCard = cardsPlayer[x, z];
                    selectedCard.makeFaceUp(true, x, z, cardsCaller[x, z]);
                    //TODO - simplify this switch statement there is a lot of repeated elements in each case
                    //TODO - simplify EndTurn, checkVictory() and endGame(), think these could be one function
                    var winState = checkVictory();
                    if (winState == CS.BLUEWIN || winState == CS.REDWIN) endGame();
                    break;

                case CS.BAD:
                    //flip the Card, end the turn
                    selectedCard = cardsPlayer[x, z];
                    selectedCard.makeFaceUp(true, x, z, cardsCaller[x, z]);
                    EndTurn(moveResult);
                    break;

                case CS.DEATH_TEAM:
                    //flip the Card, end the game
                    selectedCard = cardsPlayer[x, z];
                    selectedCard.makeFaceUp(true, x, z, cardsCaller[x, z]);
                    EndTurn(moveResult);
                    break;

                case CS.CIVIL_TEAM:
                    //flip the Card, end the turn
                    selectedCard = cardsPlayer[x, z];
                    selectedCard.makeFaceUp(true, x, z, cardsCaller[x, z]);
                    EndTurn(moveResult);
                    break;
            }
        }

//		}
    }

    public void TryHandMove(string cardID)
        //Need to build the TryHandMove Function to replicate the TryGameboardMove Function
    {
        string moveResult;

        //Multiplayer support
        //Note that we need to create local variables as if it is not your turn you may not have a selected card defined

        //Select the card, note that it may not be this client that selected the card

        SelectPlayerHandCard(cardID);
        //TODO <START HERE> Build the logic for selecting a card in the hand
        if (selectedCard != null)
        {
            moveResult = selectedCard.ValidMove(isRedTurn);
            switch (moveResult)
            {
                case CS.GOOD:
                    //Flip the Card and keep on picking
                    selectedCard = cardsPlayerHand[cardID];
                    //TODO <START HERE> this is not right selectedCard need not be passed into the function
                    selectedCard.makeUsedUp(true, selectedCard);
                    //TODO - simplify this switch statement there is a lot of repeated elements in each case
                    //TODO - simplify EndTurn, checkVictory() and endGame(), think these could be one function
                    var winState = checkVictory();
                    if (winState == CS.BLUEWIN || winState == CS.REDWIN) endGame();
                    break;
                default:
                {
                    Debug.Log("Error - Invalid result from trying to move card in hand");
                }
                    break;
            }
        }
    }

    private void EndTurn(string moveResult)
    {
        var winState = checkVictory();
        Debug.Log(winState);
        // If Death card is drawn end the game
        if (moveResult == CS.DEATH_TEAM)
        {
            endGame();
            // Otherwise continue to see if a victory has occured
        }
        else
        {
            if (winState == CS.NONEWIN)
            {
                //switch which team is picking
                isRedTurn = !isRedTurn;

                //Get the latest set of Gamecards
                client.GetGameCardDeck();

                if (turnIndicator.GetComponent<Renderer>().material.color == Color.blue)
                    turnIndicatorScript.setColour("red");
                else if (turnIndicator.GetComponent<Renderer>().material.color == Color.red)
                    turnIndicatorScript.setColour("blue");
                else
                    turnIndicator.GetComponent<Renderer>().material.color = Color.white;
                selectedCard = null;
            }
            // Victory may have occured
            else if (winState == CS.BLUEWIN || winState == CS.REDWIN)
            {
                endGame();
            }
            else
            {
                Debug.Log("Error - Winstate is " + winState);
            }
        }
    }

    private string checkVictory()
    {
        //iterate through cards looking for a win
        var retVal = "";
        var cntRed = 9;
        var cntBlue = 8;
        for (var z = 0; z < 5; z++)
        for (var x = 0; x < 5; x++)
            if (cardsPlayer[x, z].isCardUp)
            {
                if (cardsPlayer[x, z].cardType == CS.BLUE_TEAM) cntBlue = cntBlue - 1;
                if (cardsPlayer[x, z].cardType == CS.RED_TEAM) cntRed = cntRed - 1;
            }

        //Update counts
        countTextRed.text = "Red Remaining " + cntRed;
        countTextBlue.text = "Blue Remaining " + cntBlue;

        //See if anyone has won
        if (cntRed == 0)
            retVal = CS.REDWIN;
        else if (cntBlue == 0)
            retVal = CS.BLUEWIN;
        else
            retVal = CS.NONEWIN;
        return retVal;
    }

    private bool endGame()
    {
        Debug.Log("Game Over !!! - Press R to restart");
        isGameover = true;
        return true;
    }

    public void GeneratePlayerGameboard()
    {
        if (client.isPlayer)
            SetCamera("P");
        else
            SetCamera("C");
        //reset counters for each of the card types
        var rndChoose = 0;
        var redCnt = 0;
        var blueCnt = 0;
        var civilCnt = 0;
        var deathCnt = 0;
        var validChoice = false;
        GameObject go = null;
        string cardType = null;

        for (var z = 0; z < 5; z++)
        for (var x = 0; x < 5; x++)
        {
            switch (populate[x + z * 5])
            {
                case CS.RED_TEAM:
                    go = Instantiate(redPfb);
                    cntRedCards += 1;
                    break;
                case CS.BLUE_TEAM:
                    go = Instantiate(bluePfb);
                    cntBlueCards += 1;
                    break;
                case CS.CIVIL_TEAM:
                    go = Instantiate(civilPfb);
                    cntCivilCards += 1;
                    break;
                case CS.DEATH_TEAM:
                    go = Instantiate(deathPfb);
                    cntDeathCards += 1;
                    break;
            }

            GenerateGameBoardCard(x, z, ref go, words[x + z * 5], populate[x + z * 5]);
        }

        //Blue goes first
        if (cntBlueCards > cntRedCards)
        {
            isRedStart = false;
            isRedTurn = false;
            turnIndicatorScript.setColour("blue");
        }
        else
            //Red goes first
        {
            isRedStart = true;
            isRedTurn = true;
            turnIndicatorScript.setColour("red");
        }

        countTextRed.text = "Red Remaining " + cntRedCards;
        countTextBlue.text = "Blue Remaining " + cntBlueCards;
    }

    public void GeneratePlayerHand()
    {
        if (client.isPlayer != true)
        {
            //TODO - Callers may get a differrent view  START HERE
        }

        //reset counters for each of the card types
        GameObject go = null;
        var cardInstructions = "";
        string cardType = null;
        var clientID = "";
        var cardID = "";
        var playerNum = 0;
        var cardNum = 0;
        cntRedHandCards = 0;
        cntBlueHandCards = 0;

        // reset the players hand
        GameObject[] gameObjects;
        gameObjects = GameObject.FindGameObjectsWithTag(CS.OBJ_LOCATION_TAG_PLAYERHAND);
        for (var i = 0; i < gameObjects.Length; i++) Destroy(gameObjects[i]);

        for (var cnt = 0; cnt < client.gcd.gameCards.Count; cnt++)
        {
            //TO DO - This is limited to red and blue only and is based on team on owner of cards, add owner of cards to cards using playerID from server
            playerNum = Convert.ToInt32(client.gcd.gameCards[cnt].cardPlayerNum);
            clientID = client.gcd.gameCards[cnt].cardClientID;
            cardID = client.gcd.gameCards[cnt].cardID;
            go = Instantiate(handPfb);

            if (client.gcd.gameCards[cnt].cardSuit == CS.RED_TEAM)
            {
                cardNum = cntRedHandCards;
                cntRedHandCards += 1;
                cardInstructions =
                    client.gcd.gameCards[cnt].cardSuit +
                    Environment.NewLine + string.Join(" ", new[]
                        {
                            client.gcd.gameCards[cnt].cardWhenPlayable[0].turnStage,
                            "can be played",
                            client.gcd.gameCards[cnt].cardWhenPlayable[0].numTimes.ToString(), "time"
                        }
                    ) +
                    Environment.NewLine + string.Join(" ", new[]
                        {
                            client.gcd.gameCards[cnt].cardEffectPlayable[0].effectName,
                            client.gcd.gameCards[cnt].cardEffectPlayable[0].affectWhat,
                            "can be played",
                            client.gcd.gameCards[cnt].cardEffectPlayable[0].numTimes.ToString(), "time"
                        }
                    );
                cardType = CS.RED_TEAM;
            }
            else if (client.gcd.gameCards[cnt].cardSuit == CS.BLUE_TEAM)
            {
                cardNum = cntBlueHandCards;
                cntBlueHandCards += 1;
                cardInstructions =
                    client.gcd.gameCards[cnt].cardSuit +
                    Environment.NewLine + string.Join(" ", new[]
                        {
                            client.gcd.gameCards[cnt].cardWhenPlayable[0].turnStage,
                            "can be played",
                            client.gcd.gameCards[cnt].cardWhenPlayable[0].numTimes.ToString(), "time"
                        }
                    ) +
                    Environment.NewLine + string.Join(" ", new[]
                        {
                            client.gcd.gameCards[cnt].cardEffectPlayable[0].effectName,
                            client.gcd.gameCards[cnt].cardEffectPlayable[0].affectWhat,
                            "can be played",
                            client.gcd.gameCards[cnt].cardEffectPlayable[0].numTimes.ToString(), "time"
                        }
                    );
                cardType = CS.BLUE_TEAM;
            }

            GeneratePlayerHandCard(playerNum, clientID, cardNum, cardID, ref go, cardInstructions, cardType);
        }
    }

    private void GenerateGameBoardCard(int x, int z, ref GameObject go, string word, string cardType)
    {
        //TODO - the GenerateCard() class should be methods on the Card() class
        go.transform.SetParent(transform);
        var cardGameBoard = go.GetComponent<Card>();

        //Add the word to the card
        go.transform.Find("PlayingCardWordBack").GetComponent<TextMesh>().text = word;
        go.transform.Find("PlayingCardWordFront").GetComponent<TextMesh>().text = word;

        cardsPlayer[x, z] = cardGameBoard;
        Debug.Log(string.Concat(x, ",", z));
        MoveGameBoardCard(go, x, z);

        cardGameBoard.ChangeMaterial(cardType);

        //Requires an empty object of uniform scale to preserve the scale of the card and allow it to rotate without distortion
        var emptyObjectCard = new GameObject();
        emptyObjectCard.transform.parent = gameBoardPlayer.transform;
        cardGameBoard.transform.parent = emptyObjectCard.transform;

        //Rotate the card
        cardGameBoard.makeFaceUp(false);

        //Populate Caller Gameboard
        //copy the card for the Caller gameboard
        var cardCaller = Instantiate(cardGameBoard, 3.35f * Vector3.forward + cardGameBoard.transform.position,
            Quaternion.identity);
        cardsCaller[x, z] = cardCaller;

        //Requires an empty object of uniform scale to preserve the scale of the card and allow it to rotate without distortion
        var emptyObjectCardCaller = new GameObject();
        emptyObjectCardCaller.transform.parent = gameBoardCaller.transform;
        cardCaller.transform.parent = emptyObjectCardCaller.transform;

        //Rotate the card
        cardCaller.makeFaceUp(true);
    }

    private void GeneratePlayerHandCard(int playerNum, string clientID, int cardNum, string cardID, ref GameObject go,
        string word, string cardType)
    {
        //TODO - the GenerateCard() class should be methods on the Card() class
        go.transform.SetParent(transform);
        var cardPlayerHand = go.GetComponent<Card>();

        //Add the word to the card, but just the front not the back
        go.transform.Find("PlayingCardWordBack").GetComponent<TextMesh>().text = "";
        go.transform.Find("PlayingCardWordFront").GetComponent<TextMesh>().text = word;

        //Add the player number, card number and card id to the card, add the reference to it into the array that stores cards
        cardPlayerHand.playerNum = playerNum;
        cardPlayerHand.cardNum = cardNum;
        cardPlayerHand.cardID = cardID;

        cardsPlayerHand.Add(cardID, cardPlayerHand);

        Debug.Log(string.Concat("cardPlayerHand:", playerNum, ",", cardNum));
        MovePlayerHandCard(go, playerNum, cardNum);

        cardPlayerHand.ChangeMaterial(cardType);

        //Requires an empty object of uniform scale to preserve the scale of the card and allow it to rotate without distortion
        var emptyObjectCard = new GameObject();
        emptyObjectCard.transform.parent = gameBoardPlayer.transform;
        cardPlayerHand.transform.parent = emptyObjectCard.transform;
        emptyObjectCard.tag = CS.OBJ_LOCATION_TAG_PLAYERHAND;
        emptyObjectCard.name = CS.OBJ_NAME_ROOT_CARD;

        //Rotate the card
        cardPlayerHand.makeInHand();
    }

    private void MovePlayerHandCard(GameObject go, int playerNum, int cardNum)
    {
        var cardPos = new Vector3(-1.5f, 1.25f, 0.5F * cardNum);
        //TODO - remove hardcoding for number of players only being 2, the else statement will not work with only 2 players
        if (client.players[playerNum].clientID == client.clientID)
            cardPos += handOffsetLeft;
        else
            cardPos += handOffsetRight;
        Debug.Log(cardPos);
        go.transform.position = cardPos;
    }

    private void MoveGameBoardCard(GameObject go, int x, int z)
    {
        var cardPos = new Vector3(0.5F * x, 0, 0.5F * z) + boardOffsetLeft;
        Debug.Log(cardPos);
        go.transform.position = cardPos;
    }

    public void SetCamera(string cam)
    {
        switch (cam)
        {
            case "P":
                callerCamera.GetComponent<Camera>().enabled = false;
                playerCamera.GetComponent<Camera>().enabled = true;
                callerCamera.SetActive(false);
                playerCamera.SetActive(true);
                currentCamera = playerCamera;
                break;

            case "C":
                callerCamera.GetComponent<Camera>().enabled = true;
                playerCamera.GetComponent<Camera>().enabled = false;
                callerCamera.SetActive(true);
                playerCamera.SetActive(false);
                currentCamera = callerCamera;
                break;
        }
    }

    public void ResartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }
}