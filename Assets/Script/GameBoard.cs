using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AssemblyCSharp;

public class GameBoard : MonoBehaviour
{
	public static GameBoard Instance{ set; get; }

	public GameObject playerCamera;
	public GameObject callerCamera;

	private GameObject currentCamera;

	public static int gridXDim = 5;
	public static int gridYDim = 5;

	public Card[,] cardsPlayer = new Card[gridXDim,gridYDim];
	public Card[,] cardsCaller = new Card[gridXDim,gridYDim];

	public GameObject redPfb;
	public GameObject bluePfb;
	public GameObject civilPfb;
	public GameObject deathPfb;

	private GameObject turnIndicator;
	private TurnIndicator turnIndicatorScript;

	private GameObject gameBoardCaller;
	private GameObject gameBoardPlayer;

	private Vector3 boardOffset = new Vector3(-1.00f,1.0f,-1.00f);

	public bool isRedStart;
	public bool isRedTurn;
	public bool isRestart;
	public bool isGameover;

	public int cntRedCards = 0;
	public int cntBlueCards = 0;
	public int cntCivilCards = 0;
	public int cntDeathCards = 0;

	public Text countTextRed;
	public Text countTextBlue;

	public string[] words = new string[25];
	public string[] populate = new string[25];

	private Card selectedCard;
	private Vector2 mouseOver;

	private Client client;

	//TODO - Add in a constants file for things like "bad" and "good"

	private void Start()
	{
		Instance = this;
		client = FindObjectOfType<Client> ();

//		//decide at random who goes first and how many of each card is needed
//		int rndExtraRed = Random.Range(0,2);
//		cntRedCards = cntBlueCards = cntCivilCards = Mathf.Abs (gridXDim * gridYDim/3);
//
		turnIndicator = GameObject.Find ("Turn Indicator");
		turnIndicatorScript = turnIndicator.GetComponent <TurnIndicator> ();
//
		gameBoardCaller = GameObject.Find ("Game Board Caller");
		gameBoardPlayer = GameObject.Find ("Game Board Player");
//
//		//otherScript = turnIndicator.GetComponent(TurnIndicator);
//		//Blue goes first
//		if (rndExtraRed == 0)
//		{
//			cntBlueCards = cntRedCards + 1;
//			cntCivilCards = cntRedCards - 1;
//			isRedStart = false;
//			isRedTurn = false;
//			turnIndicatorScript.setColour ("blue");
//
//		} else
//		//Red goes first
//		{
//			cntRedCards = cntBlueCards + 1;
//			cntCivilCards = cntBlueCards - 1;
//			isRedStart = true;
//			isRedTurn = true;
//			turnIndicatorScript.setColour ("red");
//		}
//		cntDeathCards = gridXDim * gridYDim - (cntRedCards + cntBlueCards + cntCivilCards);
//
//		//countTextRed = 0;
//		countTextRed.text = "Red Remaining " + cntRedCards;
//		//countTextBlue = 0;
//		countTextBlue.text = "Blue Remaining " + cntBlueCards;

		isRestart = false;
		isGameover = false; 	
//		//Send message so can start the process of getting cards;

		//TODO - Get Dictionary from the server, as opposed to getting it locally
		//Should we delegate all communication to the clients / servers?
		//Note that only one of these requests should be made otherwise we'll generate too many cards as each client will request
		if (client.isHost==true){
			client.Send(
				"CDIC" + '|'
				+ client.clientName
			);	
		} 
	}

	private void Update()
	{
		//TODO - need a UI button too for this
		// If the game is over of a restart situation has occured then don't accept anymore input


		if (Input.GetKeyDown (KeyCode.R))
		{
			client.Send (
				"CKEY" + '|'
				+ client.clientName + '|'
				+ Input.inputString
			);
		}
		else if ((Input.GetKeyDown (KeyCode.C) || Input.GetKeyDown (KeyCode.P)) && (client.isHost == true || isGameover == true))
		{
			setCamera (Input.inputString.ToUpper());
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
					+ (z.ToString())
				);
				//TryMove (x, z);
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

		RaycastHit hit;
		if (Physics.Raycast (currentCamera.GetComponent<UnityEngine.Camera>().ScreenPointToRay (Input.mousePosition), out hit, 25.0f, LayerMask.GetMask ("Board")))
		{
			//TODO - the offset of 1.2 used here is hard coded based on the size of the box collider "real-world" numbers, it should be calculated
			float gameboardDimx = transform.Find ("Game Board Player").localScale.x;
			float gameboardDimz = transform.Find ("Game Board Player").localScale.z;
			mouseOver.x = (int)((hit.point.x + gameboardDimx/2)/(2.45/gridXDim));
			mouseOver.y = (int)((hit.point.z + gameboardDimz/2)/(2.35/gridYDim));
		}
		else
		{
			mouseOver.x = -1;
			mouseOver.y = -1;
		}
			
	}

	private void SelectCard(int x, int y)
	{
		//TODO - remove hardcoding of array dimension
		//Out of Bounds check
		if((x < 0) || (x > 4) || (y<0) || (y>4))
		{
			Debug.Log("Item does not exist in array at " + x + ", " + y);
			return;
		}

		Card c = cardsPlayer[x,y];
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

		//TODO - The logic here is a little bit hacky, really we should not have to call SelectCard before and within this method
		SelectCard (x, z);

		//Check if we are out of bounds
		if ((x < 0) || (x > 4) || (z < 0) || (z > 4))
		{
			selectedCard = null;
		}
		else
		{
			if (selectedCard != null)
			{				
				moveResult = selectedCard.ValidMove (isRedTurn);
				switch (moveResult)
				{

				case CS.GOOD:
					//Flip the Card and keep on picking
					selectedCard = cardsPlayer [x, z];
					//TODO - isCardUp should occur as a result of startrotating
					selectedCard.isCardUp = true;
					selectedCard.makeFaceUp (x,z, cardsCaller[x,z]);
					//TODO - simplify this switch statement there is a lot of repeated elements in each case
					//TODO - simplify EndTurn, checkVictory() and endGame(), think these could be one function
					string winState = checkVictory ();
					if ((winState == "bluewin") || (winState == "redwin"))
					{
							endGame ();
					}
					break;

				case CS.BAD:
					//flip the Card, end the turn
					selectedCard = cardsPlayer [x, z];
					selectedCard.isCardUp = true;
					selectedCard.makeFaceUp (x,z, cardsCaller[x,z]);
					EndTurn (moveResult);
					break;
				
				case CS.DEATH_TEAM:
					//flip the Card, end the game
					selectedCard = cardsPlayer [x, z];
					selectedCard.isCardUp = true;
					selectedCard.makeFaceUp (x,z, cardsCaller[x,z]);
					EndTurn (moveResult);
					break;
				
				case CS.CIVIL_TEAM:
					//flip the Card, end the turn
					selectedCard = cardsPlayer [x, z];
					selectedCard.isCardUp = true;
					selectedCard.makeFaceUp (x,z, cardsCaller[x,z]);
					EndTurn (moveResult);
					break;
				}
			}
		}
	}

	private void EndTurn(string moveResult)
	{
		string winState = checkVictory ();
		Debug.Log (winState);
		// If Death card is drawn end the game
		if (moveResult == "death")
		{
			endGame ();
		// Otherwise continue to see if a victory has occured
		} else
		{
			if (winState == "nonewin")
			{
				//switch which team is picking
				isRedTurn = !isRedTurn;

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
			else if ((winState == "bluewin") || (winState == "redwin"))
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
		for (int y = 0; y < 5; y++)
		{
			for (int x = 0; x < 5; x++)
			{
				if (cardsPlayer [x, y].isCardUp)
				{
					if (cardsPlayer [x, y].cardType == CS.BLUE_TEAM)
					{
						cntBlue = cntBlue -1;
					}
					if (cardsPlayer [x, y].cardType == CS.RED_TEAM)
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
			retVal = "redwin";
		} else if (cntBlue == 0)
		{
			retVal = "bluewin";
		} else
		{
			retVal = "nonewin";
		}
		return(retVal);
	}

	private bool endGame()
	{
		Debug.Log ("Game Over !!! - Press R to restart");
		isGameover = true;
		return(true);
	}
		
	public void GenerateGameboard()
	{
		if (client.isHost == true)
		{
			setCamera ("C");
		} else
		{
			setCamera ("P");
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
				GenerateCard (x, z, ref go, words [x + z * 5], populate [x + z * 5]);
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

	private void GenerateCard(int x, int z, ref GameObject go, string word, string cardType)
	{
		//TODO - the GenerateCard() class should be methods on the Card() class
		go.transform.SetParent(transform);
		Card cardPlayer = go.GetComponent<Card>();
		cardPlayer.isCardUp = false;

		//Add the word to the card
		go.transform.Find("PlayingCardWordBack").GetComponent<TextMesh>().text=word;
		go.transform.Find("PlayingCardWordFront").GetComponent<TextMesh>().text=word;

		cardsPlayer[x, z] = cardPlayer;
		Debug.Log(string.Concat(x, ",", z));
		MoveCard (go, x, z);

		cardPlayer.ChangeMaterial (cardType);

		//Requires an empty object of uniform scale to preserve the scale of the card and allow it to rotate without distortion
		var emptyObjectCard = new GameObject();
		emptyObjectCard.transform.parent = gameBoardPlayer.transform;
		cardPlayer.transform.parent = emptyObjectCard.transform;

		//copy the card for the Caller gameboard
		Card cardCaller = Instantiate(cardPlayer, 3.35f*Vector3.forward + cardPlayer.transform.position, Quaternion.identity);
		cardsCaller[x, z] = cardCaller;

		//Requires an empty object of uniform scale to preserve the scale of the card and allow it to rotate without distortion
		var emptyObjectCardCaller = new GameObject();
		emptyObjectCardCaller.transform.parent = gameBoardCaller.transform;
		cardCaller.transform.parent = emptyObjectCardCaller.transform;

		//Rotate the card
		cardCaller.rotateFaceUp = 0;
	}
		
	private void MoveCard(GameObject go, int x, int z)
	{
		Vector3 cardPos = new Vector3(0.5F*x,0,0.5F*z) + boardOffset;
		Debug.Log(cardPos);
		go.transform.position = cardPos;
	}

	public void setCamera(string cam)
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

	public void resartGame()
	{
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex,LoadSceneMode.Single);
	}
}	