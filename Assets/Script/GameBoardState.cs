using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Script
{
    public class GameBoardState : MonoBehaviour
    {
        //makes class a singleton
        public static GameBoardState Instance { set; get; }
    
        //which team's turn it is
        public static bool isRedTurn;
 
        //the decks that will be used in the game
        public GameBoardDeck gbd = new GameBoardDeck();
    
        public GameHandDeck ghd = new GameHandDeck();

        public int cntBlueCards;
        public int cntRedCards;
        public int cntBlueHandCards;
        public int cntRedHandCards;
        public int cntCivilCards;
        public int cntDeathCards;
        public int cntGoalRedCards;
        public int cntGoalBlueCards;
        public bool isGameover;
        private static Server _server;

        
        private int gridXDim;
        private int gridZDim;
        
        public void Init()
        {
            //needed to make this a singleton
            Instance = this;
   
            //needed to preserve game objects
            DontDestroyOnLoad(gameObject);
            GameManager.Instance.goDontDestroyList.Add(gameObject);
            Debug.Log("Added GameBoardState at position:" + GameManager.Instance.goDontDestroyList.Count + " to donotdestroylist");

            //Get Reference to Server GameObject in Unity
            _server = GetComponentInParent<Server>();
        }

        private void Update()
        {
      
        }

        private void start()
        {
            isGameover = false;
        }
    
        public void BuildHandDeck(string msg)
        {
            var pc = new GameCard();
//        var pc = new GameCard(); 

//		//Demo loading data
//		filePath = Application.persistentDataPath + "/gamecarddeck3.xml";
//		gcd = GameCardDeck.Load (filePath);
//		Debug.Log ("Loaded : " + filePath);

            switch (msg)
            {
                //Build new player decks for a new game
                case CS.CREATE:
                    //Clear the decks prior to rebuilding them
                    ghd.gameCards.Clear();

                    for (var playerCnt = 0; playerCnt < Server.clients.Count; playerCnt++)
                    for (var cardNum = 0; cardNum < CS.CSCARDHANDDIM; cardNum++)
                    {
                        //create new card
                        pc = MakeHandCard(playerCnt);
                
                        //add Card to deck
                        ghd.gameCards.Add(pc);
                    }
                    break;
            
                //update player decks at end of turn
                case CS.END:
                    int cntReplace = CS.TEP_NUM_DRAW;
                    for (var playerCnt = 0; playerCnt < Server.clients.Count; playerCnt++)
                    for (var cardNum = 0; cardNum < CS.CSCARDHANDDIM; cardNum++)
                    {
                        pc = ghd.gameCards[playerCnt * CS.CSCARDHANDDIM + cardNum];
                        if ((pc.cardRevealed == CS.CAR_REVEAL_SHOWN) && (cntReplace > 0));
                        {
                            //replace the correct # cards that have been played at the end of the turn
                            cntReplace--;

                            //create a new card
                            pc = MakeHandCard(playerCnt);

                            //update deck, replacing old card with new card
                            //TODO - This next line needs to be thought through more as messages are not delivered to the client in sequence expected
                            ghd.gameCards[playerCnt * CS.CSCARDHANDDIM + cardNum] = pc;
                        }
                    }
                    break;
            }
            SaveDeck(ghd);
        }

        private void SaveDeck(object obj)
        {
            string filePath;
        
            //Save the decks that have been created
            filePath = Application.persistentDataPath;

            if (obj is GameBoardDeck)
            {
                filePath += "/gameboarddeck1.xml";
                gbd.Save(filePath);
            }
            else if(obj is GameHandDeck)
            {
                filePath += "/gameboardhand1.xml";
                ghd.Save(filePath);
            }

            Debug.Log("Wrote : " + filePath);
        }

        public bool UpdateHandDeckCardStatus(string cardID, string clientID)
        {
            gridXDim = _server.GridXDim;
            gridZDim = _server.GridZDim;
            
            string strReponse = "";
            var s = FindObjectOfType<Server>();
            var pc = new GameCard(); 
            var gc = new GameCard();
            bool isPlayableCard = false;
            for (var playerCnt = 0; playerCnt < Server.clients.Count; playerCnt++)
            for (var cardNum = 0; cardNum < CS.CSCARDHANDDIM; cardNum++)
            {
                pc = ghd.gameCards[playerCnt * CS.CSCARDHANDDIM + cardNum];
            
                //Check to see if the cardID is correctly associated with the ClientID
                //TODO - To make this more secure clients should only know their (and only their own) clientID
                if ((pc.cardID == cardID) && (pc.cardClientID == clientID) && (pc.cardRevealed == CS.CAR_REVEAL_HIDDEN))
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
                                if ((cep.effectName == CS.CEP_EFFECT_RANDOM_REVEAL_CARD)
                                    && (cep.numTimes > 0))
                                {
                                    Debug.Log("Played reveal " + CS.CEP_EFFECT_RANDOM_REVEAL_CARD);
                                    int x = 0;
                                    int z = 0;
                                    string cardGameboardID = "";
                                    do
                                    {
                                        x = Random.Range(0, gridXDim);
                                        z = Random.Range(0, gridZDim);
                                        cardID = gbd.gameCards[x + z * gridXDim].cardID;
                                    } while (gbd.gameCards[x + z * gridXDim].cardRevealed == CS.CAR_REVEAL_SHOWN);
                
                                    string xStr = x.ToString();
                                    string zStr = z.ToString();
                                    string[] zData = {"CMOV", clientID, cardGameboardID, cardID};
                                    Incoming(zData);
                                }

                                if ((cep.effectName == CS.CEP_EFFECT_RANDOM_CHANGE_CARD)
                                    && (cep.numTimes > 0))
                                
                                {
                                    Debug.Log("Played reveal " + CS.CEP_EFFECT_RANDOM_CHANGE_CARD);
                                }

                                if ((cep.effectName == CS.CEP_EFFECT_RANDOM_REMOVE_CARD)
                                    && (cep.numTimes > 0))
                                {
                                    Debug.Log("Played reveal " + CS.CEP_EFFECT_RANDOM_REMOVE_CARD);
                                }
                                if (cep.numTimes > 0) cep.numTimes--;
                                //TODO - This is not quite the right place to make the decision as to if the card is still playable
                                if (cep.numTimes == 0) ghd.gameCards[playerCnt * CS.CSCARDHANDDIM + cardNum].cardRevealed = CS.CAR_REVEAL_SHOWN;
                            }
                        } 
                    }
                }
            }
            SaveDeck(ghd);
            return(isPlayableCard);
        }

        public bool UpdateGameboardDeckCardStatus(string cardID, string clientID)
        {
            gridXDim = _server.GridXDim;
            gridZDim = _server.GridZDim;

            string strReponse = "";
            var s = FindObjectOfType<Server>();
            var bc = new GameCard(); 
            var gc = new GameCard();
            bool isPlayableCard = false;
            string moveResult;

            int z = 0;
            int x = 0;
            
            //Check to see if the cardID is correct and the card has not been played ClientID
            while ((z < gridZDim) && (isPlayableCard == false))
            {
                while ((x < gridXDim) && (isPlayableCard == false))
                {
                    bc = gbd.gameCards[x + z * gridXDim];

                    //TODO - To make this more secure clients should only know their (and only their own) clientID
                    //TODO - cep and cwp are not currently attributes of board cards but there is no reason they should not be
                    if ((bc.cardID == cardID) && (bc.cardRevealed == CS.CAR_REVEAL_HIDDEN))
                    {

                        isPlayableCard = true;

                    }
                    if (isPlayableCard == false) x++;
                }

                if (isPlayableCard == false)
                {
                    x = 0;
                    z++;
                }
            }

            //Now check to see what type of card has been selected
            if (isPlayableCard)
            {
                Debug.Log("Server | cardID is valid:" + cardID);
                bc.cardRevealed = CS.CAR_REVEAL_SHOWN;
                gbd.gameCards[x + z * gridXDim] = bc;
                Debug.Log("Server | Player revealed card:" + cardID + "at x:" +x.ToString() + "z:" +z.ToString());
            
                //Select the card, note that it may not be this client that selected the card

                //START HERE - You have copied the following from Gameboard.TryGameboardMove()
                //START HERE - You are trying to get the gameboard state managed on the server
                //START HERE - The issue you are battling with is that ValidMove is a method on Card.cs not GameCard.cs

                moveResult = bc.ValidMove(isRedTurn);
                switch (moveResult)
                {
                    case CS.GOOD:
                        //Flip the Card and keep on picking
                        var winState = checkVictory();
                        if (winState == CS.BLUEWIN || winState == CS.REDWIN) endGame();
                        break;

                    case CS.BAD:
                        //flip the Card, end the turn
                        EndTurn(moveResult);
                        break;

                    case CS.DEATH_TEAM:
                        //flip the Card, end the game
                        EndTurn(moveResult);
                        break;

                    case CS.CIVIL_TEAM:
                        //flip the Card, end the turn
                        EndTurn(moveResult);
                        break;
                }
            } else
                Debug.Log("Server | cardID is invalid:" + cardID);
            SaveDeck(gbd);
            return(isPlayableCard);
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
            gridXDim = _server.GridXDim;
            gridZDim = _server.GridZDim;

            //iterate through cards looking for a win
            var retVal = "";
            var bc = new GameCard();
            int z = 0;
            int x = 0;
            cntBlueCards = 0;
            cntRedCards = 0;
            for (z = 0; z < gridZDim; z++)
            for (x = 0; x < gridXDim; x++)
                bc = gbd.gameCards[x + z * gridXDim];
            if (bc.cardRevealed == CS.CAR_REVEAL_SHOWN)
            {
                if (bc.cardSuit == CS.BLUE_TEAM) cntBlueCards++;
                if (bc.cardSuit == CS.RED_TEAM) cntRedCards++;
            }
        
            for (z = 0; z < gridZDim; z++)
            for (x = 0; x < gridXDim; x++)
            {
                bc = gbd.gameCards[x + z * gridXDim];
            }
        
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
            Debug.Log("Server | Game Over !!! - Press R to restart");
            isGameover = true;
            return true;
        }
        
        
        private GameCard MakeHandCard(int playerCnt)
        {
            //create a card
            var gc = new GameCard();
            gc.cardID = Guid.NewGuid().ToString();
            gc.cardPlayerNum = playerCnt.ToString();
            gc.cardClientID = Server.clients[playerCnt].clientID;
            if (Server.clients[playerCnt].isRedTeam)
            {
                gc.cardSuit = CS.RED_TEAM;
                gc.cardLocation = CS.CAR_LOCATION_RED_DECK;
            }
            else if (!Server.clients[playerCnt].isRedTeam)
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
        private GameCard MakeBoardCard()
        {
            //create a card
            var gc = new GameCard();
            gc.cardLocation = CS.CAR_LOCATION_GAMEBOARD;
            gc.cardRevealed = CS.CAR_REVEAL_HIDDEN;
            return gc;
        }
        public void Incoming(string[] aData)

            //TODO - Upgrade to XML and message objects when they are available
        {
            gridXDim = _server.GridXDim;
            gridZDim = _server.GridZDim;
            
            int x = 0;
            int z = 0;
        
            switch (aData[0])
            {
                case "CDIC":
                    //Cannot use FindObjectOfType in the constructor, so have to assign in here    
                    var worddictionary = FindObjectOfType<WordDictionary>();
                    
                    //Build the Gameboard data
                    worddictionary.buildGameboardData();
                    isRedTurn = (worddictionary.isRedStart =="1"? true: false);
                    
                    //Clear Gameboard data
                    cntGoalRedCards = 0;
                    cntGoalBlueCards = 0;
                    gbd.gameCards.Clear();
                    
                    //Rebuild Gameboard data
                    for (z = 0; z < gridZDim; z++)
                    for (x = 0; x < gridXDim; x++)
                    {
                        //create new card
                        GameCard gc = MakeBoardCard();
                    
                        //add Card to deck
                        gbd.gameCards.Add(gc);
                    
                        //implement additional attributes
                        gbd.gameCards[x + z * gridXDim].cardXPos = x;
                        gbd.gameCards[x + z * gridXDim].cardZPos = z;
                        gbd.gameCards[x + z * gridXDim].cardWord = worddictionary.gameBoardCardData[x + z * gridXDim].wordList;
                        gbd.gameCards[x + z * gridXDim].cardSuit = worddictionary.gameBoardCardData[x + z * gridXDim].populate;
                        gbd.gameCards[x + z * gridXDim].cardID = worddictionary.gameBoardCardData[x + z * gridXDim].cardid;
                        gbd.gameCards[x + z * gridXDim].cardRevealed = CS.CAR_REVEAL_HIDDEN;
                        
                        //Increase the goals for the teams to win
                        if (gbd.gameCards[x + z * gridXDim].cardSuit == CS.RED_TEAM) cntGoalRedCards += 1;
                        if (gbd.gameCards[x + z * gridXDim].cardSuit == CS.RED_TEAM) cntGoalBlueCards += 1;
                    }
                    break;
                case "CMOV":
                    for (z = 0; z < gridZDim; z++)
                    for (x = 0; x < gridXDim; x++)
                    {
                        //implement additional attributes
                        if (gbd.gameCards[x + z * gridXDim].cardID == aData[3]) gbd.gameCards[x + z * gridXDim].cardRevealed = CS.CAR_REVEAL_SHOWN;
                    }
                    
                    break;
                case "CPCC":
                    BuildHandDeck(CS.CREATE);
                    break;
                case "CPCU":
                    BuildHandDeck(CS.END);
                
                    //Change turn indicator status at end of turn
                    isRedTurn = !isRedTurn;
                    break;
                case "CGFU":
                    break;
                case "CKEY":
                    var tmpVal = aData[2].ToUpper();
                    if (tmpVal == "R") BuildHandDeck(CS.CREATE);
                    break;
            }
        }
    }
}