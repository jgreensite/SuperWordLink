using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AssemblyCSharp;

public class GameBoard : MonoBehaviour
{
	//makes class a singleton
	public static GameBoard Instance{ set; get; }

	public GameObject playerCamera;
	public GameObject callerCamera;

	public GameObject currentCamera;

	public static int gridXDim = CS.CSGRIDXDIM;
	public static int gridZDim = CS.CSGRIDZDIM;
	public static int cardHandDim = CS.CSCARDHANDDIM;

	public Card[,] cardsPlayer = new Card[gridXDim,gridZDim];
	public Card[,] cardsCaller = new Card[gridXDim,gridZDim];

	//TODO - Remove hardcoding for number of players and cards in each player's hand
	public Card[,] cardsPlayerHand = new Card[2,cardHandDim];

	public GameObject redPfb;
	public GameObject bluePfb;
	public GameObject civilPfb;
	public GameObject deathPfb;
	public GameObject handPfb;

	private GameObject turnIndicator;
	private TurnIndicator turnIndicatorScript;

	private GameObject gameBoardCaller;
	private GameObject gameBoardPlayer;

	private Vector3 boardOffset = new Vector3(-1.00f,1.0f,-1.00f);

	//TODO - Remove hardcoding for number of players, this will only work with 3 cards in the hand
	private Vector3 handOffset = new Vector3(0.0f,0.0f,-0.5f);

	public bool isRedStart;
	public bool isRedTurn;
	public bool isRestart;
	public bool isGameover;

	//GameboardCards
	public int cntRedCards = 0;
	public int cntBlueCards = 0;
	public int cntCivilCards = 0;
	public int cntDeathCards = 0;

	//PlayerCards
	public int cntRedHandCards = 0;
	public int cntBlueHandCards = 0;

	public Text countTextRed;
	public Text countTextBlue;

	public string[] words = new string[25];
	public string[] populate = new string[25];

	private Card selectedCard;
	private Vector2 mouseOver;

	private Client client;

	private void Start()
	{
		//needed to make this a singleton
		Instance = this;

		client = FindObjectOfType<Client> ();

		turnIndicator = GameObject.Find ("Turn Indicator");
		turnIndicatorScript = turnIndicator.GetComponent <TurnIndicator> ();

		gameBoardCaller = GameObject.Find ("Game Board Caller");
		gameBoardPlayer = GameObject.Find ("Game Board Player");

		isRestart = false;
		isGameover = false; 	

		//TODO the dictionary should be the place where the initial cards are dealt
		//Get Dictionary from the server, as opposed to getting it locally
		//Only the host should make this request otherwise we'll generate too many cards as each client will request dictionary
		if (client.isHost==true){
			client.Send(
				"CDIC" + '|'
				+ client.clientName
			);	
		} 
		// Get the initial set of Gamecards
	}

	private void Update()
	{
		//TODO - also need a UI button for restarting
		// If the game is over of a restart situation has occured then don't accept anymore input

		if (Input.GetKeyDown (KeyCode.R))
		{
			client.Send (
				"CKEY" + '|'
				+ client.clientName + '|'
				+ Input.inputString
			);
		}
		else if ((Input.GetKeyDown (KeyCode.C) || Input.GetKeyDown (KeyCode.P)) && (client.isPlayer == false || isGameover == true))
		{
			SetCamera (Input.inputString.ToUpper());
		}
		else if(isGameover == false)
		{
			UpdateMouseOver ();
			//Debug.Log (mouseOver);

			//if it is my turn
			int x = (int)mouseOver.x;
			int z = (int)mouseOver.y;

			//TODO - need to add in support for touch screens and cursor keys
			if (Input.GetMouseButtonDown (0))
				SelectCard (x, z);

			if (Input.GetMouseButtonUp (0))
				//Send move to server
				client.Send(
					"CMOV" + '|'
					+ client.clientName + '|'
					+ (x.ToString()) + '|'
					+ (z.ToString()) + '|'
					+ client.clientID
				);
		}
	}

	private void UpdateMouseOver()
	{
		//Check if it is my turn

		//Check if a current camera exists
		if(!currentCamera)
		{
			Debug.Log ("Unable to find Current Camera");
			return;
		}

		RaycastHit hitGameBoard;
		RaycastHit hitPlayerCard;

		//Check if clicked on a card in the hand
		if (Physics.Raycast (currentCamera.GetComponent<UnityEngine.Camera>().ScreenPointToRay (Input.mousePosition), out hitPlayerCard, 25.0f, LayerMask.GetMask (CS.OBJ_LOCATION_LAYER_PLAYERHAND)))
		{
			//TODO colliders on all cards are hardcoded, really should work them out from card geometry
			Debug.Log (hitPlayerCard.collider.gameObject.GetComponent<Card>().cardNum);
			Debug.Log (hitPlayerCard.collider.gameObject.GetComponent<Card>().playerNum);

//			float gameboardDimx = transform.Find ("Game Board Player").localScale.x;
//			float gameboardDimz = transform.Find ("Game Board Player").localScale.z;
//			mouseOver.x = (int)((hitGameBoard.point.x + gameboardDimx/2)/(2.45/gridXDim));
//			mouseOver.y = (int)((hitGameBoard.point.z + gameboardDimz/2)/(2.35/gridZDim));
		}
		//check if clicked on a card on the deck
		else
			if (Physics.Raycast (currentCamera.GetComponent<UnityEngine.Camera>().ScreenPointToRay (Input.mousePosition), out hitGameBoard, 25.0f, LayerMask.GetMask (CS.OBJ_LOCATION_LAYER_GAMEBOARD)))
		{
			//TODO - the offset of 1.2 used here are hard coded based on the size of the box collider "real-world" numbers, it should be calculated
			float gameboardDimx = transform.Find ("Game Board Player").localScale.x;
			float gameboardDimz = transform.Find ("Game Board Player").localScale.z;
			mouseOver.x = (int)((hitGameBoard.point.x + gameboardDimx/2)/(2.45/gridXDim));
			mouseOver.y = (int)((hitGameBoard.point.z + gameboardDimz/2)/(2.35/gridZDim));
		}
		else
		{
			mouseOver.x = -1;
			mouseOver.y = -1;
		}

			
	}

	private void SelectCard(int x, int z)
	{
		//Out of Bounds check
		if((x < 0) || (x > (gridXDim-1)) || (z<0) || (z>(gridZDim-1)))
		{
			Debug.Log("Item does not exist in array at " + x + ", " + z);
			selectedCard = null;
			return;
		}

		Card c = cardsPlayer[x,z];
		//cannot flip a card that has been flipped
		if ((c != null) && (!c.isCardUp))
		{
			selectedCard = c;
			Debug.Log ("Card selected is " + selectedCard.name);
		} else
		{
			Debug.Log ("Error - Card is either already selected or out of bounds");
		}
	}

	public void TryMove(int x, int z)
	{
		string moveResult;

		//Multiplayer support
		//Note that we need to create local variables as if it is not your turn you may not have a selected card defined

		//Select the card, note that it may not be this client that selected the card
		SelectCard (x, z);

//		//Check if we are out of bounds
//		if ((x < 0) || (x > (gridXDim-1)) || (z < 0) || (z > (gridZDim-1)))
//		{
//			selectedCard = null;
//		}
//		else
//		{
			if (selectedCard != null)
			{				
				moveResult = selectedCard.ValidMove (isRedTurn);
				switch (moveResult)
				{

				case CS.GOOD:
					//Flip the Card and keep on picking
					selectedCard = cardsPlayer [x, z];
					selectedCard.makeFaceUp (true, x,z, cardsCaller[x,z]);
					//TODO - simplify this switch statement there is a lot of repeated elements in each case
					//TODO - simplify EndTurn, checkVictory() and endGame(), think these could be one function
					string winState = checkVictory ();
					if ((winState == CS.BLUEWIN) || (winState == CS.REDWIN))
					{
						endGame ();
					}
					break;

				case CS.BAD:
					//flip the Card, end the turn
					selectedCard = cardsPlayer [x, z];
					selectedCard.makeFaceUp (true, x,z, cardsCaller[x,z]);
					EndTurn (moveResult);
					break;
				
				case CS.DEATH_TEAM:
					//flip the Card, end the game
					selectedCard = cardsPlayer [x, z];
					selectedCard.makeFaceUp (true, x,z, cardsCaller[x,z]);
					EndTurn (moveResult);
					break;
				
				case CS.CIVIL_TEAM:
					//flip the Card, end the turn
					selectedCard = cardsPlayer [x, z];
					selectedCard.makeFaceUp (true, x,z, cardsCaller[x,z]);
					EndTurn (moveResult);
					break;
				}
			}
//		}
	}

	private void EndTurn(string moveResult)
	{
		string winState = checkVictory ();
		Debug.Log (winState);
		// If Death card is drawn end the game
		if (moveResult == CS.DEATH_TEAM)
		{
			endGame ();
		// Otherwise continue to see if a victory has occured
		} else
		{
			if (winState == CS.NONEWIN)
			{
				//switch which team is picking
				isRedTurn = !isRedTurn;

				//Get the latest set of Gamecards
				client.GetGameCardDeck ();

				if (turnIndicator.GetComponent<Renderer> ().material.color == Color.blue)
				{
					turnIndicatorScript.setColour ("red");
				} else if (turnIndicator.GetComponent<Renderer> ().material.color == Color.red)
				{
					turnIndicatorScript.setColour ("blue");
				} else
				{
					turnIndicator.GetComponent<Renderer> ().material.color = Color.white;
				}
				selectedCard = null;
			}
			// Victory may have occured
			else if ((winState == CS.BLUEWIN) || (winState == CS.REDWIN))
			{
				endGame ();
			} else
			{
				Debug.Log("Error - Winstate is " + winState);
			}
		}
	}

	private string checkVictory()
	{
		//iterate through cards looking for a win
		string retVal = "";
		int cntRed = 9;
		int cntBlue = 8;
		for (int z = 0; z < 5; z++)
		{
			for (int x = 0; x < 5; x++)
			{
				if (cardsPlayer [x, z].isCardUp)
				{
					if (cardsPlayer [x, z].cardType == CS.BLUE_TEAM)
					{
						cntBlue = cntBlue -1;
					}
					if (cardsPlayer [x, z].cardType == CS.RED_TEAM)
					{
						cntRed = cntRed - 1;
					}
				}
			}
		}

		//Update counts
		countTextRed.text = "Red Remaining " + cntRed.ToString();
		countTextBlue.text = "Blue Remaining " + cntBlue.ToString();

		//See if anyone has won
		if (cntRed == 0)
		{
			retVal = CS.REDWIN;
		} else if (cntBlue == 0)
		{
			retVal = CS.BLUEWIN;
		} else
		{
			retVal = CS.NONEWIN;
		}
		return(retVal);
	}

	private bool endGame()
	{
		Debug.Log ("Game Over !!! - Press R to restart");
		isGameover = true;
		return(true);
	}
		
	public void GeneratePlayerGameboard()
	{
		if (client.isPlayer == true)
		{
			SetCamera ("P");
		} else
		{
			SetCamera ("C");
		}
		//reset counters for each of the card types
		int rndChoose = 0;
		int redCnt = 0;
		int blueCnt = 0;
		int civilCnt = 0;
		int deathCnt = 0;
		bool validChoice = false;
		GameObject go = null;
		string cardType = null;

		for (int z = 0; z < 5; z++)
		{
			for (int x = 0; x < 5; x++)
			{
				switch (populate [x + z * 5])
				{
				case CS.RED_TEAM:
					go = Instantiate (redPfb) as GameObject;
					cntRedCards += 1;
					break;
				case CS.BLUE_TEAM:
					go = Instantiate (bluePfb) as GameObject;
					cntBlueCards += 1;
					break;
				case CS.CIVIL_TEAM:
					go = Instantiate (civilPfb) as GameObject;
					cntCivilCards += 1;
					break;
				case CS.DEATH_TEAM:
					go = Instantiate (deathPfb) as GameObject;
					cntDeathCards += 1;
					break;
				}
				GenerateGameBoardCard (x, z, ref go, words [x + z * 5], populate [x + z * 5]);
			}
		}
		//Blue goes first
		if (cntBlueCards > cntRedCards)
		{
			isRedStart = false;
			isRedTurn = false;
			turnIndicatorScript.setColour ("blue");

		} else
		//Red goes first
		{
			isRedStart = true;
			isRedTurn = true;
			turnIndicatorScript.setColour ("red");
		}
		countTextRed.text = "Red Remaining " + cntRedCards;
		countTextBlue.text = "Blue Remaining " + cntBlueCards;
	}

	public void GeneratePlayerHand()
	{
		if (client.isPlayer == true)
		{
			//TODO - Callers may get a differrent view  START HERE
		}
		//reset counters for each of the card types
//		int rndChoose = 0;
//		int redCnt = 0;
//		int blueCnt = 0;
//		int civilCnt = 0;
//		int deathCnt = 0;
//		bool validChoice = false;
		GameObject go = null;
		string cardInstructions = "";
		string cardType = null;
		int cardId = 0;

		for (int playerNum = 0; playerNum < 1; playerNum++)
		{
			for (int cardNum = 0; cardNum < 3; cardNum++)
			{
				switch (client.isRedTeam)
				{
				case true:
					go = Instantiate (handPfb) as GameObject;
					cardId = cntRedHandCards;
					cntRedHandCards += 1;
					cardInstructions = "Red team instructions";
					cardType = CS.RED_TEAM;
					break;
				case false:
					go = Instantiate (handPfb) as GameObject;
					cardId = cntBlueHandCards;
					cntBlueHandCards += 1;
					cardInstructions = "Blue team instructions";
					cardType = CS.BLUE_TEAM;
					break;
//				case CS.CIVIL_TEAM:
//					go = Instantiate (civilPfb) as GameObject;
//					cntCivilCards += 1;
//					break;
//				case CS.DEATH_TEAM:
//					go = Instantiate (deathPfb) as GameObject;
//					cntDeathCards += 1;
//					break;
				}
				//TODO - cardId is not used, possibly get rid of it or have it populated by the server
				GeneratePlayerHandCard (playerNum, cardNum, ref go, cardInstructions, cardType, cardId);
			}
		}
//		//Blue goes first
//		if (cntBlueCards > cntRedCards)
//		{
//			isRedStart = false;
//			isRedTurn = false;
//			turnIndicatorScript.setColour ("blue");
//
//		} else
//			//Red goes first
//		{
//			isRedStart = true;
//			isRedTurn = true;
//			turnIndicatorScript.setColour ("red");
//		}
//		countTextRed.text = "Red Remaining " + cntRedCards;
//		countTextBlue.text = "Blue Remaining " + cntBlueCards;
	}

	private void GenerateGameBoardCard(int x, int z, ref GameObject go, string word, string cardType)
	{
		//TODO - the GenerateCard() class should be methods on the Card() class
		go.transform.SetParent(transform);
		Card cardGameBoard = go.GetComponent<Card>();

		//Add the word to the card
		go.transform.Find("PlayingCardWordBack").GetComponent<TextMesh>().text=word;
		go.transform.Find("PlayingCardWordFront").GetComponent<TextMesh>().text=word;

		cardsPlayer[x, z] = cardGameBoard;
		Debug.Log(string.Concat(x, ",", z));
		MoveGameBoardCard (go, x, z);

		cardGameBoard.ChangeMaterial (cardType);

		//Requires an empty object of uniform scale to preserve the scale of the card and allow it to rotate without distortion
		var emptyObjectCard = new GameObject();
		emptyObjectCard.transform.parent = gameBoardPlayer.transform;
		cardGameBoard.transform.parent = emptyObjectCard.transform;

		//Rotate the card
		cardGameBoard.makeFaceUp(false);

		//Populate Caller Gameboard
		//copy the card for the Caller gameboard
		Card cardCaller = Instantiate(cardGameBoard, 3.35f*Vector3.forward + cardGameBoard.transform.position, Quaternion.identity);
		cardsCaller[x, z] = cardCaller;

		//Requires an empty object of uniform scale to preserve the scale of the card and allow it to rotate without distortion
		var emptyObjectCardCaller = new GameObject();
		emptyObjectCardCaller.transform.parent = gameBoardCaller.transform;
		cardCaller.transform.parent = emptyObjectCardCaller.transform;

		//Rotate the card
		cardCaller.makeFaceUp(true);
	}
		
	private void GeneratePlayerHandCard(int playerNum, int cardNum, ref GameObject go, string word, string cardType, int cardId)
	{
		//TODO - the GenerateCard() class should be methods on the Card() class
		go.transform.SetParent(transform);
		Card cardPlayerHand = go.GetComponent<Card>();

		//Add the word to the card, but just the front not the back
		go.transform.Find("PlayingCardWordBack").GetComponent<TextMesh>().text="";
		go.transform.Find("PlayingCardWordFront").GetComponent<TextMesh>().text=word;

		//Add the player and card id to the card, add the reference to it into the array that stores cards
		cardPlayerHand.playerNum = playerNum;
		cardPlayerHand.cardNum = cardNum;

		cardsPlayerHand[playerNum, cardNum] = cardPlayerHand;

		Debug.Log(string.Concat("cardPlayerHand:", playerNum, ",", cardNum));
		MovePlayerHandCard (go, playerNum, cardNum);

		cardPlayerHand.ChangeMaterial (cardType);

		//Requires an empty object of uniform scale to preserve the scale of the card and allow it to rotate without distortion
		var emptyObjectCard = new GameObject();
		emptyObjectCard.transform.parent = gameBoardPlayer.transform;
		cardPlayerHand.transform.parent = emptyObjectCard.transform;

		//Rotate the card
		cardPlayerHand.makeInHand();
	}
		
	private void MovePlayerHandCard(GameObject go, int playerNum, int cardNum)
	{
		Vector3 cardPos = new Vector3(-1.5f,1.25f,0.5F*cardNum) + handOffset;
		Debug.Log(cardPos);
		go.transform.position = cardPos;
	}

	private void MoveGameBoardCard(GameObject go, int x, int z)
	{
		Vector3 cardPos = new Vector3(0.5F*x,0,0.5F*z) + boardOffset;
		Debug.Log(cardPos);
		go.transform.position = cardPos;
	}

	public void SetCamera(string cam)
	{
		switch (cam)
		{
		case "P":
			callerCamera.GetComponent<UnityEngine.Camera> ().enabled = false;
			playerCamera.GetComponent<UnityEngine.Camera> ().enabled = true;
			callerCamera.SetActive (false);
			playerCamera.SetActive (true);
			currentCamera = playerCamera;
			break;

		case "C":
			callerCamera.GetComponent<UnityEngine.Camera>().enabled = true;
			playerCamera.GetComponent<UnityEngine.Camera>().enabled = false;
			callerCamera.SetActive(true);
			playerCamera.SetActive(false);
			currentCamera = callerCamera;
			break;
		}
	}

	public void ResartGame()
	{
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex,LoadSceneMode.Single);
	}
}	