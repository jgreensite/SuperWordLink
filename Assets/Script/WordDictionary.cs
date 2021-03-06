﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Script
{
    public class WordDictionary : MonoBehaviour
    {
        //Establish whose turn it is
        
        private int gridXDim;
        private int gridZDim;
        private int gridSize;
        public Card[,] cardsPlayerGameBoard;
    
        private Dictionary<int, List<string>> words = new Dictionary<int, List<string>>();
        public Dictionary<int, GameWords> gameBoardCardData = new Dictionary<int, GameWords>();
        //public string[] wordList = new string[gridSize];
        //public string[] populate = new string[gridSize];
        //public string[] cardid = new string[gridSize];
    
        public int largeWordLength = 9;
        public int smallWordLength = 3;

        public string isRedStart = "";
    
        //Set in the editor
        public TextAsset txt;

        public GameObject redPfb;
        public GameObject bluePfb;
        public GameObject deathPfb;
        public GameObject civilPfb;
        private static Server _server;
        private static GameState _gameState;

        //makes class a singleton
        public static WordDictionary Instance { set; get; }
      
        // Use this for initialization
        private void Start()
        {
            //needed to make this a singleton
            Instance = this;

            //needed to preserve game objects between scenes
            DontDestroyOnLoad(gameObject);
            GameManager.Instance.goDontDestroyList.Add(gameObject);
            Debug.Log("Added WordDictionary at position:" + GameManager.Instance.goDontDestroyList.Count +
                      " to donotdestroylist");
            
            //Get Reference to Server GameObject in Unity
            _server = GetComponentInParent<Server>();
            _gameState = GetComponentInParent<GameState>();
        }

        public void buildGameboardData()
        {
            //dimension the arrays            
            gridXDim = _gameState.g.gameParameters.sizeOfXDim;
            gridZDim = _gameState.g.gameParameters.sizeOfYDim;

            gridSize = gridXDim * gridZDim;
            cardsPlayerGameBoard = new Card[gridXDim, gridZDim];
            
            //clear the data
            gameBoardCardData.Clear();
        
            //build the data
            GetWords();
            GenerateWords();
            AssignWords();
            
        }

        // Use this to generate the words for the cards
        private void GetWords()
        {
            //TextAsset txt = (TextAsset)Resources.Load("Words");
            var dict = txt.text.Split("\n"[0]); // Should probably check for null in txt, but it's just an example

            for (var len = 3; len < 11; len++) words[len] = new List<string>();

            foreach (var word in dict)
            {
                var n = word.Length;
                if (n < 11 && n > 2)
                {
                    var l = words[n];
                    l.Add(word);
                }
            }
        }

        private void GenerateWords()
        {
            var rndWordLength = -1;
            var rndWordPosition = -1;
            var wordString = "";
            var cntDuplicateWord = 1;
            var newWord = "";
            for (var x = 0; x < (gridSize); x++)
            {
                while (cntDuplicateWord >0)
                {
                    cntDuplicateWord = 0;
                    rndWordLength = Random.Range(smallWordLength, largeWordLength);
                    rndWordPosition = Random.Range(1, words[rndWordLength].Count);
                    newWord = words[rndWordLength][rndWordPosition];
                    foreach(var pair in gameBoardCardData)
                    {
                        if (pair.Value.wordList == newWord)
                        {
                            cntDuplicateWord += 1;
                        }
                    }
                }

                gameBoardCardData.Add(x, new GameWords
                {
                    wordList = newWord,
                    populate = "",
                    cardid =Guid.NewGuid().ToString()
                });
            
                wordString = wordString + ", " + gameBoardCardData[x].wordList;
                cntDuplicateWord = 1;
            }

            Debug.Log("Word list " + wordString);
        }
    
   
        //public string[] AssignWords(out string isRedStart)
        private void AssignWords()
        {
            //TODO - Don't hardcode or repeat the grid dimensions
     
            string cardType = null;
            var cardTypes = new string[gridSize];
            
            int cntRedCards;
            int cntBlueCards;
            int cntCivilCards;
            int cntDeathCards;
     
            //decide at random who goes first and how many of each card is needed
            var rndExtraRed = Random.Range(0, 2);
            cntRedCards = cntBlueCards = cntCivilCards = Mathf.Abs(gridSize / 3);

            //Blue goes first
            if (rndExtraRed == 0)
            {
                cntBlueCards = cntRedCards + 1;
                cntCivilCards = cntRedCards - 1;
                isRedStart = "0";
            }
            else
                //Red goes first
            {
                cntRedCards = cntBlueCards + 1;
                cntCivilCards = cntBlueCards - 1;
                isRedStart = "1";
            }

            cntDeathCards = gridSize - (cntRedCards + cntBlueCards + cntCivilCards);

            var redCnt = 0;
            var blueCnt = 0;
            var civilCnt = 0;
            var deathCnt = 0;

            //populate cards at random
            //TODO - Don't repeat the grid dimensions
            for (var z = 0; z < gridZDim; z++)
            for (var x = 0; x < gridXDim; x++)
            {
                var validChoice = false;
                while (validChoice == false)
                {
                    //TODO - Don't hardcode or repeat the grid dimensions
                    var rndChoose = Random.Range(0, gridSize);
                    var rndSwitch = 0;

                    //randomly pick a Card according to the distribution of each card types total number of cards
                    if (rndChoose >= gridSize - cntDeathCards) // deathcard possibilities
                        rndSwitch = 3;
                    else if (rndChoose > cntRedCards + cntCivilCards - 1 &&
                             rndChoose <= cntRedCards + cntCivilCards + cntBlueCards - 1) // bluecard possibilities
                        rndSwitch = 2;
                    else if (rndChoose > cntRedCards - 1 && rndChoose <= cntRedCards + cntCivilCards - 1
                    ) // civilcard possibilities
                        rndSwitch = 1;
                    else // redcard possibilities
                        rndSwitch = 0;
                    switch (rndSwitch)
                    {
                        //Having picked a card see if it can be added
                        //red cards
                        case 0:
                            if (redCnt < cntRedCards)
                            {
                                redCnt++;
                                validChoice = true;
                                Debug.Log("red chosen");
                                cardType = CS.RED_TEAM;
                            }
                            else
                            {
                                validChoice = false;
                            }

                            break;

                        //blue cards
                        case 1:
                            if (blueCnt < cntBlueCards)
                            {
                                blueCnt++;
                                validChoice = true;
                                Debug.Log("blue chosen");
                                cardType = CS.BLUE_TEAM;
                            }
                            else
                            {
                                validChoice = false;
                            }

                            break;

                        //civilian cards
                        case 2:
                            if (civilCnt < cntCivilCards)
                            {
                                civilCnt++;
                                validChoice = true;
                                Debug.Log("civil chosen");
                                cardType = CS.CIVIL_TEAM;
                            }
                            else
                            {
                                validChoice = false;
                            }

                            break;

                        //death cards
                        case 3:
                            if (deathCnt < cntDeathCards)
                            {
                                deathCnt++;
                                validChoice = true;
                                Debug.Log("death chosen");
                                cardType = CS.DEATH_TEAM;
                            }
                            else
                            {
                                validChoice = false;
                            }

                            break;
                    }

                    //if the card is able to added then add it
                    if (validChoice)
                    {
                        gameBoardCardData[x + z * gridXDim].populate = cardType;
                    }
 
                }
            }

            //return (cardTypes);
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}