using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssemblyCSharp;


public class Card : MonoBehaviour {

	//Card rotations and text presentations
	public bool rotateFaceUp = false;
	public bool rotateVertical = false;
	public bool rotateFlattenText = false;

//	private float m_maximumTilt = 45f;
	private float m_tiltSpeed = 1.0f;

	private float m_initialXRot;
	private float m_initialYRot;
	private float m_initialZRot;

	private float m_initialTextXRot;
	private float m_initialTextYRot;
	private float m_initialTextZRot;

	private float m_currentXRot;
	private float m_currentYRot;
	private float m_currentZRot;

	private float m_currentTextXRot;
	private float m_currentTextYRot;
	private float m_currentTextZRot;

	private float m_finalXValue = 0f;
	private float m_finalYValue = 0f;
	private float m_finalZValue = 0f;

//	private float m_finalTextXValue = 0f;
//	private float m_finalTextYValue = 0f;
//	private float m_finalTextZValue = 0f;

	public string cardType;
	public bool isCardUp;
	int cnt = 0;

	public Material[] mats = new Material[CS.NUMCARDMATERIALS];
	public Material redMaterial;
	public Material blueMaterial;
	public Material civilMaterial;
	public Material deathMaterial;

	public string tmpStr = CS.EMPTY;

	public int playerNum;
	public int cardNum;

	// Check move is valid
	public string ValidMove(bool isRedTurn){
		string retVal = CS.ERROR;
		switch (cardType)
		{
		case CS.RED_TEAM:
			if (isRedTurn)
			{
				retVal = CS.GOOD;
			} else
			{
				retVal = CS.BAD;
			}
			break;

		case CS.BLUE_TEAM:
			if (!isRedTurn)
			{
				retVal = CS.GOOD;
			} else
			{
				retVal = CS.BAD;
			}
			break;

		case CS.DEATH_TEAM:
			retVal = CS.DEATH_TEAM;
			break;

		case CS.CIVIL_TEAM:
			retVal = CS.CIVIL_TEAM;
			break;
		
		default:
			retVal = CS.ERROR;
			break;
		}
		return(retVal);
	}

	// Call this to start the rotation and highlight the card
	public void makeFaceUp(bool flagUp){

		if (flagUp == true)
		{
			// set the faceUp status now even though rotation has not finished
			isCardUp = true;

			// This sets the desired rotation of the card, the update function will then perform the rotation 
			m_finalXValue = 180f;
			m_finalYValue = 0f;
			m_finalZValue = 0f;

//			//This sets the desired rotation of the text on the card, the update function will then perform the rotation 
//			m_finalTextXValue = 0f;
//			m_finalTextYValue = 0f;
//			m_finalTextZValue = 0f;

		}
		else if (flagUp == false)
		{
			// set the faceUp status now even though rotation has not finished
			isCardUp = false;

			// This sets the desired rotation of the card, the update function will theXMLn perform the rotation 
			m_finalXValue = 0f;
			m_finalYValue = 0f;
			m_finalZValue = 0f;

//			//This sets the desired rotation of the text on the card, the update function will then perform the rotation 
//			m_finalTextXValue = 0f;
//			m_finalTextYValue = 0f;
//			m_finalTextZValue = 0f;
		}
	}

	// Call this to start the rotation and highlight the card
	public void makeFaceUp(bool flagUp, int x, int z, Card siblingCaller){
		makeFaceUp (flagUp);

		// This sets the highlight
		ChangeHighlight ();
		//siblingCaller.ChangeHighlight();
	}

	// Call this to make a card in the players hand
	public void makeInHand (){
		// set the faceUp status now even though rotation has not finished
		isCardUp = true;

		// This sets the desired rotation of the card, the update function will then perform the rotation 
		m_finalXValue = 0f;
		m_finalYValue = 0f;
		m_finalZValue = 0f;

		//This sets the Layer
		//TODO - Consider using something like the following to set recursievely the layer and do this on the. Array.ForEach(gameObject.GetComponentsInChildren<Transform>(), (entry) => entry.gameObject.layer = LayerMask.NameToLayer("MyLayer"));
		gameObject.transform.Find("Card").gameObject.layer = LayerMask.NameToLayer(CS.OBJ_LOCATION_LAYER_PLAYERHAND);

//		//This sets the desired rotation of the text on the card, the update function will then perform the rotation 
//		m_finalTextXValue = 0f;
//		m_finalTextYValue = 0f;
//		m_finalTextZValue = 0f;
	}

	// Call this to highlight the card has been used and remove it from play
	public void makeUsedUp(bool flagUp, int x, int z, Card siblingCaller){

		//TODO - Change what happends to a card when it gets used
		makeFaceUp (flagUp);

		// This sets the highlight
		ChangeHighlight ();
		siblingCaller.ChangeHighlight();
	}



	// Use this for initialization
	void Start () {
		cnt = cnt + 1;
		//get the Initial rotation of the card and store it for reference and for setting up the current rotations
		m_currentXRot = m_initialXRot = transform.rotation.eulerAngles.x;
		m_currentYRot = m_initialYRot = transform.rotation.eulerAngles.y;
		m_currentZRot = m_initialZRot = transform.rotation.eulerAngles.z;

		Debug.Log ("Card X, Y and Z rotations are " + m_currentXRot + " " + m_currentYRot + " " + m_currentZRot);

		//get the Initial rotation of the text on the front of the card and store it for reference and for setting up the current rotations
		m_currentTextXRot = m_initialTextXRot = transform.Find("PlayingCardWordFront").transform.rotation.eulerAngles.x;
		m_currentTextYRot = m_initialTextYRot = transform.Find("PlayingCardWordFront").transform.rotation.eulerAngles.y;
		m_currentTextZRot = m_initialTextZRot = transform.Find("PlayingCardWordFront").transform.rotation.eulerAngles.z;

		Debug.Log ("Card Front Text X, Y and Z rotations are " + m_currentXRot + " " + m_currentYRot + " " + m_currentZRot);

		//TODO - get the current materials on both sides of the card and as the card turns over change the material on the back of the card to be the material on the front of the card
//		tmpStr = gameObject.transform.Find("Card").GetComponent<Renderer>().materials.ToString();
//		mats = gameObject.transform.Find ("Card").GetComponent<Renderer> ().materials;
//		print ("Component materials = " +tmpStr);
//		Material tmpMats = mats [0];
//		mats [0] = mats [1];
//		mats [1] = tmpMats;
//		gameObject.transform.Find ("Card").GetComponent<Renderer>().materials = mats;
	}

	//Update is called once per frame
	//note that this will rotate to an absolute amount not a relative amount
	void Update(){

		//Rotate card
		if ((m_currentXRot <= (m_initialXRot + m_finalXValue) * 0.99))
		{
			m_currentXRot += Time.deltaTime * m_finalXValue / m_tiltSpeed;
		}

		if ((m_currentYRot <= (m_initialYRot + m_finalYValue) * 0.99))
		{
			m_currentYRot += Time.deltaTime * m_finalYValue / m_tiltSpeed;
		}

		if ((m_currentZRot <= (m_initialZRot + m_finalZValue) * 0.99))
		{
			m_currentZRot += Time.deltaTime * m_finalZValue / m_tiltSpeed;
		}
				
		//make sure that if X is close the target angle of rotation it doesn't over or under rotate
		if (m_currentXRot > (m_initialXRot + m_finalXValue) * 0.98)
		{
			m_currentXRot = m_initialXRot + m_finalXValue;
		}

		//make sure that if Y is close the target angle of rotation it doesn't over or under rotate
		if (m_currentYRot > (m_initialYRot + m_finalYValue) * 0.98)
		{
			m_currentYRot = m_initialYRot + m_finalYValue;
		}

		//make sure that if Z is close the target angle of rotation it doesn't over or under rotate
		if (m_currentZRot > (m_initialZRot + m_finalZValue) * 0.98)
		{
			m_currentZRot = m_initialZRot + m_finalZValue;
		}

		transform.localRotation = Quaternion.Euler (m_currentXRot, m_currentYRot, m_currentZRot);

//		//rotate text on card
//		if ((m_currentTextXRot <= (m_initialTextXRot + m_finalTextXValue) * 0.99))
//		{
//			m_currentTextXRot += Time.deltaTime * m_finalTextXValue / m_tiltSpeed;
//		}
//
//		if ((m_currentTextYRot <= (m_initialTextYRot + m_finalTextYValue) * 0.99))
//		{
//			m_currentTextYRot += Time.deltaTime * m_finalTextYValue / m_tiltSpeed;
//		}
//
//		if ((m_currentTextZRot <= (m_initialTextZRot + m_finalTextZValue) * 0.99))
//		{
//			m_currentTextZRot += Time.deltaTime * m_finalTextZValue / m_tiltSpeed;
//		}
//
//		//make sure that if X is close the target angle of rotation it doesn't over or under rotate
//		if (m_currentTextXRot > (m_initialTextXRot + m_finalTextXValue) * 0.98)
//		{
//			m_currentTextXRot = m_initialTextXRot + m_finalTextXValue;
//		}
//
//		//make sure that if Y is close the target angle of rotation it doesn't over or under rotate
//		if (m_currentTextYRot > (m_initialTextYRot + m_finalTextYValue) * 0.98)
//		{
//			m_currentTextYRot = m_initialTextYRot + m_finalTextYValue;
//		}
//
//		//make sure that if Z is close the target angle of rotation it doesn't over or under rotate
//		if (m_currentTextZRot > (m_initialTextZRot + m_finalTextZValue) * 0.98)
//		{
//			m_currentTextZRot = m_initialTextZRot + m_finalTextZValue;
//		}
//
//		transform.Find("PlayingCardWordBack").transform.localRotation = Quaternion.Euler (m_currentTextXRot, m_currentTextYRot, m_currentTextZRot); 
//		transform.Find("PlayingCardWordFront").transform.localRotation = Quaternion.Euler (m_currentTextXRot, m_currentTextYRot, m_currentTextZRot); 
	}
	
	public void ChangeMaterial(string newCardType)
	{
		mats = gameObject.transform.Find("Card").GetComponent<Renderer> ().materials;
		cardType = newCardType;

		switch (cardType)
		{
		case CS.RED_TEAM:
			mats [CS.IDCARDFRONTMATERIAL] = redMaterial;
			break;

		case CS.BLUE_TEAM:
			mats [CS.IDCARDFRONTMATERIAL] = blueMaterial;
			break;

		case CS.DEATH_TEAM:
			mats [CS.IDCARDFRONTMATERIAL] = deathMaterial;
			break;

		case CS.CIVIL_TEAM:
			mats [CS.IDCARDFRONTMATERIAL] = civilMaterial;
			break;
		}
		gameObject.transform.Find ("Card").GetComponent<Renderer>().materials = mats;
	}
		
	public void ChangeHighlight()
	{
		// TODO - Remove hardcoding of number of materials and names
		mats = gameObject.transform.Find("Card").GetComponent<Renderer> ().materials;
		mats [2].color = Color.green;
		gameObject.transform.Find ("Card").GetComponent<Renderer>().materials = mats;
	}
}