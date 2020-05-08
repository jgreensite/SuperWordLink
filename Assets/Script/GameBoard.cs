using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Script
{
    public class GameBoard : MonoBehaviour
    {
        public static int gridXDim = Convert.ToInt32(GameManager.Instance.gridXDim.text);
        public static int gridZDim = Convert.ToInt32(GameManager.Instance.gridZDim.text);
        public static int cardHandDim = CS.CSCARDHANDDIM;
    
    
        //used to position the gameboards in space
        private readonly Vector3 boardOffsetLeft = new Vector3(-1.00f, 1.0f, -1.00f);
        public readonly Vector3 callerCardOffset = CS.CALLERCARDOFFSET * Vector3.forward;
    
        public GameObject callerCamera;
        //public Card[,] cardsCaller = new Card[gridXDim, gridZDim];
        public Dictionary<string, Card> cardsCaller = new Dictionary<string, Card>();
        private string cardSelectedLocation;
        public bool cardsPlayerHandForceUpdate = false;
        public bool cardsGameboardForceUpdate = false;

        //public Card[,] cardsPlayerGameBoard = new Card[gridXDim, gridZDim];
        public Dictionary<string, Card> cardsPlayerGameBoard = new Dictionary<string, Card>();

        private Client client;
        //TODO - Remove hardcoding for number of players and cards in each player's hand
        //GameboardCards
        public Dictionary<string, Card> cardsPlayerHand = new Dictionary<string, Card>();
    
        public GameObject civilPfb;
        public GameObject deathPfb;
        public GameObject bluePfb;
        public GameObject redPfb;

    
        public int cntBlueCards;
        public int cntRedCards;
        public int cntBlueHandCards;
        public int cntRedHandCards;
        public int cntCivilCards;
        public int cntDeathCards;
        public Text countTextBlue;
        public Text countTextRed;
        public int cntGoalBlueCards;
        public int cntGoalRedCards;

  
    
        public GameObject currentCamera; 
    
        public GameObject gameBoardCaller;
        public GameObject gameBoardPlayer;

        //TODO - The vectors are referenced using "x" and "y" properties, better to use arrays with more relevant property names, see how "hand is different to board"
        private Vector2 gameboardCardOver;
   

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
        private string gameboardCardIDOver;
    
        //used to track selection of board and player cards
        private Card selectedCard;

        private GameObject turnIndicator;
        private TurnIndicator turnIndicatorScript;

        //used to pass data into the Gameboard
        public string[] words = new string[gridXDim * gridZDim];
        public string[] populate = new string[gridXDim * gridZDim];
        public string[] cardids = new string[gridXDim * gridZDim];
        public int[] xPos = new int[gridXDim * gridZDim];
        public int[] zPos = new int[gridXDim * gridZDim];
        public string[] reveal = new string[gridXDim * gridZDim];

    
        //makes class a singleton
        public static GameBoard Instance { set; get; }

        private void Start()
        {
            //needed to make this a singleton
            Instance = this;

            //TODO This is how to use FindObjectOfType
            client = FindObjectOfType<Client>();

            turnIndicator = GameObject.Find("Turn Indicator");
            turnIndicatorScript = turnIndicator.GetComponent<TurnIndicator>();

            gameBoardCaller = GameObject.Find("Game Board Caller");
            gameBoardPlayer = GameObject.Find("Game Board Player");
        
            //Move the Caller Gameboard to the correct place relative to the Player Gameboard, then move the camera to focus on it
            gameBoardCaller.transform.position = callerCardOffset + gameBoardPlayer.transform.position;

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
//                      x = (int) gameboardCardOver.x;
//                      z = (int) gameboardCardOver.y;

//					//TODO - need to add in support for touch screens and cursor keys
//					if (Input.GetMouseButtonDown (0))
//						SelectGameBoardCard (x, z);

                        if (Input.GetMouseButtonUp(0))
                            //Send move to server
                            client.Send(
                                "CMOV" + '|'
                                       + client.clientName + '|'
//                                       + x + '|'
//                                       + z + '|'
                                       + gameboardCardIDOver + '|'
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

                if (cardsGameboardForceUpdate)
                {
                    client.Send(
                        "CGFU" + '|'
                               + client.clientName + '|'
                               + client.clientID
                    );
                    cardsGameboardForceUpdate = !cardsGameboardForceUpdate;
                }

                if (cardsPlayerHandForceUpdate)
                {
                    client.Send(
                        "CPFU" + '|'
                               + client.clientName + '|'
                               + client.clientID
                    );
                    cardsPlayerHandForceUpdate = !cardsPlayerHandForceUpdate;
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

            RaycastHit hitGameBoardCard;
            RaycastHit hitPlayerCard;

            //Check if clicked on a card in the hand
            var ray = currentCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hitPlayerCard, Mathf.Infinity, LayerMask.GetMask(CS.OBJ_LOCATION_LAYER_PLAYERHAND)))
            {
                //TODO colliders on all cards are hardcoded, really should work them out from card geometry
                cardSelectedLocation = CS.OBJ_LOCATION_LAYER_PLAYERHAND;
                playerhandCardIDOver = hitPlayerCard.collider.gameObject.GetComponent<Card>().cardID;
                //Debug.Log("Selected Card in Hand for player:" + playerhandCardOver.x + " at position:" + playerhandCardOver.y);
            }
            //check if clicked on a card on the deck
            else if (Physics.Raycast(ray, out hitGameBoardCard, Mathf.Infinity, LayerMask.GetMask(CS.OBJ_LOCATION_LAYER_GAMEBOARD)))
            {
                //TODO - the offset of 1.2 used here are hard coded based on the size of the box collider "real-world" numbers, it should be calculated
                cardSelectedLocation = CS.OBJ_LOCATION_LAYER_GAMEBOARD;
                var gameboardDimx = transform.Find("Game Board Player").localScale.x;
                var gameboardDimz = transform.Find("Game Board Player").localScale.z;
//                gameboardCardOver.x = (int) ((hitGameBoardCard.point.x + gameboardDimx / 2) / (2.45 / gridXDim));
//                gameboardCardOver.y = (int) ((hitGameBoardCard.point.z + gameboardDimz / 2) / (2.35 / gridZDim));
                gameboardCardIDOver = hitGameBoardCard.collider.gameObject.GetComponent<Card>().cardID;
                Debug.DrawLine(ray.origin, currentCamera.GetComponent<Camera>().transform.forward * 10, Color.red);
                //Debug.Log("User just hovered over cardID:" +gameboardCardIDOver);
                //gameboardCardIDOver = cardsPlayerGameBoard[(int)gameboardCardOver.x, (int)gameboardCardOver.y].cardID;
            }
            else
            {
                gameboardCardOver.x = -1;
                gameboardCardOver.y = -1;
                playerhandCardIDOver = "";
                gameboardCardIDOver = "";
                cardSelectedLocation = "";
            }
        }

        private void SelectGameBoardCard(string CardID)
        {
            //Out of Bounds check
            Card c;
            if (!cardsPlayerGameBoard.TryGetValue(CardID, out c))
            // if (x < 0 || x > gridXDim - 1 || z < 0 || z > gridZDim - 1)
            {
                Debug.Log("Item does not exist in gameboard dictionary for CardID:" + CardID);
                selectedCard = null;
                return;
            }

            c = cardsPlayerGameBoard[CardID];
            //cannot flip a card that has been flipped
            if (c != null && !c.isCardUp)
            {
                selectedCard = c;
                Debug.Log("Gameboard Card selected is CardID:" + CardID);
            }
            else
            {
                Debug.Log("Error - Gameboard CardId:" + CardID+" is either already selected or out of bounds");
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

        public void TryGameboardMove(string cardID)
        {
            string moveResult;

            //Multiplayer support
            //Note that we need to create local variables as if it is not your turn you may not have a selected card defined

            //Select the card, note that it may not be this client that selected the card
            SelectGameBoardCard(cardID);

            if (selectedCard != null)
            {
                moveResult = selectedCard.ValidCardMove(isRedTurn);
                switch (moveResult)
                {
                    case CS.GOOD:
                        //Flip the Card and keep on picking
                        selectedCard = cardsPlayerGameBoard[cardID];
                        selectedCard.makeReveal(true);
                        //TODO - simplify this switch statement there is a lot of repeated elements in each case
                        //TODO - simplify EndTurn, checkVictory() and endGame(), think these could be one function
                        var winState = checkVictory();
                        if (winState == CS.BLUEWIN || winState == CS.REDWIN) endGame();
                        break;

                    case CS.BAD:
                        //flip the Card, end the turn
                        selectedCard = cardsPlayerGameBoard[cardID];
                        selectedCard.makeReveal(true);
                        EndTurn(moveResult);
                        break;

                    case CS.DEATH_TEAM:
                        //flip the Card, end the game
                        selectedCard = cardsPlayerGameBoard[cardID];
                        selectedCard.makeReveal(true);
                        EndTurn(moveResult);
                        break;

                    case CS.CIVIL_TEAM:
                        //flip the Card, end the turn
                        selectedCard = cardsPlayerGameBoard[cardID];
                        selectedCard.makeReveal(true);
                        EndTurn(moveResult);
                        break;
                }
            }

//		}
        }

        public void  TryHandMove(string cardID)
            //TOD0 - Need to build the TryHandMove Function to replicate the TryGameboardMove Function
        {
            string moveResult;

            //Multiplayer support
            //Note that we need to create local variables as if it is not your turn you may not have a selected card defined
            //Select the card, note that it may not be this client that selected the card

            SelectPlayerHandCard(cardID);
        
            if (selectedCard != null)
            {
                //TODO - Would like to put in a SERVER check to see if it is the correct player's turn but these is no guarantee that card on the board has not already flipped over ending the turn for the curent player
                // FOR NOW FORCE POSITIVE SO CHECK ALWAYS PASSES
                moveResult = CS.GOOD;
                switch (moveResult)
                {
                    case CS.GOOD:
                        //Flip the Card and keep on picking
                        selectedCard = cardsPlayerHand[cardID];
                        //TODO is this right? Does selectedCard need not be passed into the function?
                        selectedCard.makeUsedUp(true);
                        cardsGameboardForceUpdate = true;
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
                    client.GetGameCardDeck(CS.END);

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
            cntBlueCards = 0;
            cntRedCards = 0;
            foreach (var c in cardsPlayerGameBoard)
            {
                if (c.Value.isCardUp)
                {
                    if (c.Value.cardType == CS.BLUE_TEAM) cntBlueCards++;
                    if (c.Value.cardType == CS.RED_TEAM) cntRedCards++;
                }
            }

            //Update counts
            countTextRed.text = "Red Remaining " + (cntGoalRedCards - cntRedCards);
            countTextBlue.text = "Blue Remaining " + (cntGoalBlueCards - cntBlueCards);

            //See if anyone has won
            if (cntRedCards == cntGoalRedCards)
                retVal = CS.REDWIN;
            else if (cntBlueCards == cntGoalBlueCards)
                retVal = CS.BLUEWIN;
            else
                retVal = CS.NONEWIN;
            return retVal;
        }

        private bool endGame()
        {
            Debug.Log("Client | Game Over !!! - Press R to restart");
            isGameover = true;
            return true;
        }

        public void GeneratePlayerGameboard()
        {
            cntGoalRedCards = 0;
            cntGoalBlueCards = 0;
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

            for (var z = 0; z < gridZDim; z++)
            for (var x = 0; x < gridXDim; x++)
            {
                switch (populate[x + z * gridXDim])
                {
                    case CS.RED_TEAM:
                        go = Instantiate(redPfb);
                        cntGoalRedCards += 1;
                        break;
                    case CS.BLUE_TEAM:
                        go = Instantiate(bluePfb);
                        cntGoalBlueCards += 1;
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

                GenerateGameBoardCard(x, z, ref go, words[x + z * gridXDim], populate[x + z * gridXDim], cardids[x + z * gridXDim]);
            }

            //Blue goes first
            if (isRedStart == false)
            {
                turnIndicatorScript.setColour("blue");
            } else {
                //Red goes first
                turnIndicatorScript.setColour("red");
            }

            countTextRed.text = "Red Remaining " + cntGoalRedCards;
            countTextBlue.text = "Blue Remaining " + cntGoalBlueCards;
        }
        public void UpdatePlayerGameboard()
        {
            var cnt = -1;
            foreach (var c in cardsPlayerGameBoard)
            {
                cnt++;
                if (((c.Value.isCardUp) == true && (reveal[cnt] == CS.CAR_REVEAL_HIDDEN))
                    || ((c.Value.isCardUp) == false && (reveal[cnt] == CS.CAR_REVEAL_SHOWN)))
                {
                
                    TryGameboardMove(c.Value.cardID);
                }

            }
        }

        public void GeneratePlayerHand()
        {
            if (client.isPlayer != true)
            {
                //TODO - Callers may get a differrent view
            }

            //reset counters for each of the card types
            GameObject go = null;
            var cardInstructions = "";
            string cardType = null;
            var clientID = "";
            var cardID = "";
            var playerNum = 0;
            var cardNum = 0;
            var usedUp = "";
            cntRedHandCards = 0;
            cntBlueHandCards = 0;

            // reset the players hand, first destroy the gameobjects
            GameObject[] gameObjects;
            gameObjects = GameObject.FindGameObjectsWithTag(CS.OBJ_LOCATION_TAG_PLAYERHAND);
            for (var i = 0; i < gameObjects.Length; i++) Destroy(gameObjects[i]);

            // now destroy the data structures
            cardsPlayerHand.Clear();
        
            for (var cnt = 0; cnt < client.gcd.gameCards.Count; cnt++)
            {
                playerNum = Convert.ToInt32(client.gcd.gameCards[cnt].cardPlayerNum);
                clientID = client.gcd.gameCards[cnt].cardClientID;
                cardID = client.gcd.gameCards[cnt].cardID;
                usedUp = client.gcd.gameCards[cnt].cardRevealed;
                if (clientID == client.clientID)
                {
                    //TO DO - This is limited to red and blue only and is based on team on owner of cards, add owner of cards to cards using playerID from server
                    go = Instantiate(handPfb);
    
                    if (client.gcd.gameCards[cnt].cardSuit == CS.RED_TEAM)
                    {
                        cardNum = cntRedHandCards;
                        cntRedHandCards += 1;
                        cardType = CS.RED_TEAM;
                    }
                    else if (client.gcd.gameCards[cnt].cardSuit == CS.BLUE_TEAM)
                    {
                        cardNum = cntBlueHandCards;
                        cntBlueHandCards += 1;
                        cardType = CS.BLUE_TEAM;
                    }
                
                    //Instructions are regardless of suit
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
                    //Generate the card in the UX 
                
                    GeneratePlayerHandCard(playerNum, clientID, cardNum, cardID, ref go, cardInstructions, cardType, usedUp);
                }
            }
        }

        private void GenerateGameBoardCard(int x, int z, ref GameObject go, string word, string cardType, string cardID)
        {
            //TODO - the GenerateCard() class should be methods on the Card() class
            //go.transform.SetParent(gameBoardPlayer.transform);
            var cardGameBoard = go.GetComponent<Card>();

            //Add the word to the card
            go.transform.Find("PlayingCardWordBack").GetComponent<TextMesh>().text = word;
            go.transform.Find("PlayingCardWordFront").GetComponent<TextMesh>().text = word;
 
            cardsPlayerGameBoard.Add(cardID,cardGameBoard);
            Debug.Log(string.Concat(x, ",", z));
            MoveGameBoardCard(go, x, z);

            cardGameBoard.ChangeMaterial(cardType);

            //Requires an empty object of uniform scale to preserve the scale of the card and allow it to rotate without distortion
            var emptyObjectCard = new GameObject();
            emptyObjectCard.name = String.Concat(CS.OBJ_NAME_ROOT_CARD,"_",CS.OBJ_OWNER_PLAYER,"_",x.ToString(),"_",z.ToString()," [",word,"]");
            //emptyObjectCard.tag = CS.OBJ_LOCATION_TAG_GAMEBOARD;
            emptyObjectCard.transform.parent = gameBoardPlayer.transform;
            cardGameBoard.transform.parent = emptyObjectCard.transform;

            //Populate the CardID
            cardGameBoard.cardID = cardID;
            cardGameBoard.name = cardID;
        
            //Rotate the card
            cardGameBoard.makeFaceUp(false);
            
            //Place in the right layer
            cardGameBoard.makeInGameboard();
        
            //Now Populate Caller Gameboard
        
            //Requires an empty object of uniform scale to preserve the scale of the card and allow it to rotate without distortion
            var emptyObjectCardCaller = Instantiate(new GameObject(),callerCardOffset,
                Quaternion.identity, gameBoardCaller.transform);
        
            //TODO - Copy requires manually copying the local scale, don't know why
            emptyObjectCardCaller.transform.localScale = emptyObjectCard.transform.localScale;
     
            emptyObjectCardCaller.name = String.Concat(CS.OBJ_NAME_ROOT_CARD,"_",CS.OBJ_OWNER_CALLER,"_",x.ToString(),"_",z.ToString()," [",word,"]");
            emptyObjectCardCaller.tag = CS.OBJ_LOCATION_TAG_GAMEBOARD;
        
            //copy the card used for the Player gameboard to the Caller gameboard 
            var cardCaller = Instantiate(cardGameBoard, callerCardOffset + cardGameBoard.transform.position,
                Quaternion.identity, emptyObjectCardCaller.transform);
        
            //TODO - copy requires manually copying the local scale, don't know why
            emptyObjectCardCaller.transform.localPosition = emptyObjectCard.transform.localPosition;
        
            cardCaller.name = cardID;
        
            cardsCaller.Add(cardID, cardCaller);

            //Rotate the card
            cardCaller.makeFaceUp(true);
        }

        private void GeneratePlayerHandCard(int playerNum, string clientID, int cardNum, string cardID, ref GameObject go,
            string word, string cardType, string usedUp)
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

            //Place the card against the correct place in the gameboard
            MovePlayerHandCard(go, playerNum, cardNum);

            //Make the card the same suit as the player
            cardPlayerHand.ChangeMaterial(cardType);

            //Requires an empty object of uniform scale to preserve the scale of the card and allow it to rotate without distortion
            var emptyObjectCard = new GameObject();
            emptyObjectCard.name = String.Concat(CS.OBJ_NAME_ROOT_CARD,"_",CS.OBJ_LOCATION_TAG_PLAYERHAND,"_",cardNum.ToString()," [",cardType,"]");
            emptyObjectCard.tag = CS.OBJ_LOCATION_TAG_PLAYERHAND;

            //Establish if the card is one for a caller or a player
            for (var cnt = 0; cnt < client.players.Count; cnt++)
            {
                if (string.Equals(client.players[cnt].clientID, clientID))
                {
                    if (client.players[cnt].isPlayer)
                    {
                        emptyObjectCard.transform.parent = gameBoardPlayer.transform;
                    }
                    else
                    {
                        emptyObjectCard.transform.parent = gameBoardCaller.transform;
                    }
                }
            }

            cardPlayerHand.transform.parent = emptyObjectCard.transform;
            cardPlayerHand.name = cardID;

            //Rotate the card
            cardPlayerHand.makeInHand();
        
            //if the card has been used up then make it such
            if(usedUp == CS.CAR_REVEAL_SHOWN) cardPlayerHand.makeUsedUp(true);
        }

        private void MovePlayerHandCard(GameObject go, int playerNum, int cardNum)
        {
            var cardPos = new Vector3(-1.5f, 1.25f, 0.5F * cardNum);
            //TODO - need to find a way to visual the cards of other players 
            if (client.players[playerNum].clientID == client.clientID)
                cardPos += handOffsetLeft;
            else
                cardPos += handOffsetRight;
            if (client.isPlayer != true)
            {
                cardPos += callerCardOffset;
            }
      
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
}