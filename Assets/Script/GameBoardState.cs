using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;
using AssemblyCSharp;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameBoardState : MonoBehaviour
{
    //makes class a singleton
    public static GameBoardState Instance { set; get; }
    
    //which team's turn it is
    public static bool isRedTurn;
 
    //the decks that will be used in the game
    public GameBoardDeck gbd = new GameBoardDeck();
    
    public GameHandDeck ghd = new GameHandDeck();

    public void Init()
    {
        //needed to make this a singleton
        Instance = this;
   
        //needed to preserve game objects
        DontDestroyOnLoad(gameObject);
        GameManager.Instance.goDontDestroyList.Add(gameObject);
        Debug.Log("Added GameBoardState at position:" + GameManager.Instance.goDontDestroyList.Count + " to donotdestroylist");
        
        //Establish whose turn it is
        var worddictionary = FindObjectOfType<WordDictionary>();
    }

    private void Update()
    {
      
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
                for (var playerCnt = 0; playerCnt < Server.clients.Count; playerCnt++)
                for (var cardNum = 0; cardNum < CS.CSCARDHANDDIM; cardNum++)
                {
                    pc = ghd.gameCards[playerCnt * CS.CSCARDHANDDIM + cardNum];
                    if (pc.cardRevealed == CS.CAR_REVEAL_SHOWN)
                    {
                        //reduce the played count


                        //establish what card has been played

                        //create a new card
                        pc = MakeHandCard(playerCnt);

                        //update deck, replacing old card with new card
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

    private void UpdateHandDeckCardStatus(string cardID, string clientID)
    {
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
                            if ((cep.effectName == CS.CEP_EFFECT_REVEAL_CARD)
                                && (cep.numTimes > 0))
                            {
                                string tmpStr = "Played reveal " + CS.CEP_EFFECT_REVEAL_CARD;
                                Debug.Log("Played reveal " + CS.CEP_EFFECT_REVEAL_CARD);
                                
                            }

                            if ((cep.effectName == CS.CEP_EFFECT_CHANGE_CARD)
                                && (cep.numTimes > 0))
                                
                            {
                                string tmpStr = "Played reveal " + CS.CEP_EFFECT_CHANGE_CARD;
                                Debug.Log("Played reveal " + CS.CEP_EFFECT_CHANGE_CARD);
                            }

                            if ((cep.effectName == CS.CEP_EFFECT_REMOVE_CARD)
                                && (cep.numTimes > 0))
                            {
                                string tmpStr = "Played reveal " + CS.CEP_EFFECT_REMOVE_CARD;
                                Debug.Log("Played reveal " + CS.CEP_EFFECT_REMOVE_CARD);
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
        gc.cardID = Guid.NewGuid().ToString();
        gc.cardLocation = CS.CAR_LOCATION_GAMEBOARD;
        gc.cardRevealed = CS.CAR_REVEAL_HIDDEN;
        return gc;
    }
    public void Incoming(string[] aData)

    //TODO - Upgrade to XML and message objects when they are available
    {
        int x = 0;
        int z = 0;
        switch (aData[0])
        {
            case "CDIC":
                //Cannot use FindObjectOfType in the constructor, so have to assign in here    
                var worddictionary = FindObjectOfType<WordDictionary>();
                
                worddictionary.buildGameboard();
                isRedTurn = (worddictionary.isRedStart =="1"? true: false);

                for (z = 0; z < CS.CSGRIDZDIM; z++)
                for (x = 0; x < CS.CSGRIDXDIM; x++)
                {
                    GameCard gc = MakeBoardCard();
                    
                    //create new card
                    gc = MakeBoardCard();
                
                    //add Card to deck
                    gbd.gameCards.Add(gc);
                    
                    //implement additional attributes
                    gbd.gameCards[x + z * CS.CSGRIDXDIM].cardXPos = x;
                    gbd.gameCards[x + z * CS.CSGRIDXDIM].cardZPos = z;
                    gbd.gameCards[x + z * CS.CSGRIDXDIM].cardWord = worddictionary.wordList[x + z * CS.CSGRIDXDIM];
                    gbd.gameCards[x + z * CS.CSGRIDXDIM].cardSuit = worddictionary.populate[x + z * CS.CSGRIDXDIM];
                }
                break;
            case "CMOV":
                x = int.Parse(aData[2]);
                z = int.Parse(aData[3]);
                gbd.gameCards[x + z * CS.CSGRIDXDIM].cardRevealed = CS.CAR_REVEAL_SHOWN;
                break;
            case "CHAN":
                UpdateHandDeckCardStatus(aData[2], aData[3]);
                break;
            case "CPCC":
                BuildHandDeck(CS.CREATE);
                break;
            case "CPCU":
                BuildHandDeck(CS.END);
                
                //Change turn indicator status
                isRedTurn = !isRedTurn;
                break;
        }
    }
}