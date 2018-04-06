using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AssemblyCSharp;

public class WordDictionary : MonoBehaviour {

	//makes class a singleton
	public static WordDictionary Instance { set; get; }

	//Set in the editor
	public TextAsset txt;
	public int smallWordLength = 3;
	public int largeWordLength = 9;

	// Use this for initialization
	void Start () {
		//needed to make this a singleton
		Instance = this;

		//needed to preserve game objects between scenes
		DontDestroyOnLoad(gameObject);
		GameManager.Instance.goDontDestroyList.Add (gameObject);
		Debug.Log ("Added WordDictionary at position:" + GameManager.Instance.goDontDestroyList.Count + " to donotdestroylist");
	}


	// Use this to generate the words for the cards
	public string[] GetWords () {

		//TextAsset txt = (TextAsset)Resources.Load("Words");
		string[] dict = txt.text.Split("\n"[0]); // Should probably check for null in txt, but it's just an example

		Dictionary<int,List<string>> words = new Dictionary<int, List<string>>();
		for(int len = 3; len < 11; len++)
		{
			words[len] = new List<string>();
		}

		foreach(string word in dict)
		{
			int n = word.Length;
			if(n < 11 && n > 2)
			{
				List<string> l = words[n];
				l.Add(word);
			}
		}
		return(GenerateWords (words));
	}

	private string[] GenerateWords(Dictionary<int,List<string>> words)
	{
		int rndWordLength = -1;
		int rndWordPosition = -1;
		string wordString = "";
		bool keepSearching = true;
		string newWord = "";
		string[] wordList = new string[25];
		for(int x = 0; x < 25; x++)
		{
			while (keepSearching)
			{
				rndWordLength = Random.Range(smallWordLength,largeWordLength);
				rndWordPosition = Random.Range(1,words[rndWordLength].Count);
				newWord = words[rndWordLength][rndWordPosition];
				if (!string.IsNullOrEmpty (wordList [0]))
				{
					keepSearching = wordList.Contains(newWord);
				} else
				{
					keepSearching = false;
				}
			}
			wordList[x] = newWord;
			wordString = wordString+ ", " + wordList[x];
			keepSearching = true;
		}
		Debug.Log ("Word list " + wordString);
		return(wordList);
	}

	public string[] AssignWords()
	{
		//TODO - Add in the code that enables us at random to decide if Red or Blue goes first
		//it is currently in the Start() of GameBoard class

		//TODO - Don't hardcode or repeat the grid dimensions
		int gridXDim = 5;
		int gridZDim = 5;
		int gridSize = gridXDim * gridZDim;

		string cardType = null;
		string[] cardTypes = new string[gridSize];

		int cntRedCards;
		int cntBlueCards;
		int cntCivilCards;
		int cntDeathCards;

		//decide at random who goes first and how many of each card is needed
		int rndExtraRed = Random.Range(0,2);
		cntRedCards = cntBlueCards = cntCivilCards = Mathf.Abs (gridSize/3);

		//Blue goes first
		if (rndExtraRed == 0)
		{
			cntBlueCards = cntRedCards + 1;
			cntCivilCards = cntRedCards - 1;
		} else
			//Red goes first
		{
			cntRedCards = cntBlueCards + 1;
			cntCivilCards = cntBlueCards - 1;
		}
		cntDeathCards = gridSize - (cntRedCards + cntBlueCards + cntCivilCards);

		int redCnt = 0;
		int blueCnt = 0;
		int civilCnt = 0;
		int deathCnt = 0;

		//populate cards at random
		//TODO - Don't repeat the grid dimensions
		for (int z = 0; z < gridZDim; z++)
		{
			for (int x = 0; x < gridXDim; x++)
			{
				bool validChoice = false;
				while (validChoice == false)
				{
					//TODO - Don't hardcode or repeat the grid dimensions
					int rndChoose = Random.Range(0,gridSize);
					int rndSwitch = 0;

					//randomly pick a Card according to the disrubtion of each card types total number of cards
					if (rndChoose >= gridSize - cntDeathCards) // deathcard possibilities
					{
						rndSwitch = 3;
					} else if ( rndChoose > (cntRedCards + cntCivilCards -1) && rndChoose <= (cntRedCards + cntCivilCards + cntBlueCards -1)) // bluecard possibilities
					{
						rndSwitch = 2;
					} else if (rndChoose > (cntRedCards -1) && rndChoose <= (cntRedCards + cntCivilCards -1)) // civilcard possibilities
					{
						rndSwitch = 1;
					} else // redcard possibilities
					{
						rndSwitch = 0;
					}
					switch (rndSwitch)
					{

					//Having picked a card see if it can be added
					//red cards
					case 0:
						if (redCnt < cntRedCards)
						{
							redCnt++;
							validChoice = true;
							Debug.Log ("red chosen");
							cardType = CS.RED_TEAM;
						} else
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
							Debug.Log ("blue chosen");
							cardType = CS.BLUE_TEAM;
						} else
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
							Debug.Log ("civil chosen");
							cardType = CS.CIVIL_TEAM;

						} else
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
							Debug.Log ("death chosen");
							cardType = CS.DEATH_TEAM;
						} else
						{
							validChoice = false;
						}
						break;
					}
					//if the card is able to added then add it
					if (validChoice == true)
					{
						cardTypes[x+z*5] = cardType;
					}
				}
			}
		}
		return (cardTypes);
	}

	// Update is called once per frame
	void Update ()
	{
		
	}
}
