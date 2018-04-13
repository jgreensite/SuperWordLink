using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssemblyCSharp;


public class Card : MonoBehaviour {

	public float rotateFaceUp = -1;

	public float m_maximumTilt = 45f;
	public float m_tiltSpeed = 50f;

	private float m_initialXRot;
	private float m_initialYRot;
	private float m_initialZRot;

	private float m_currentXRot;
	private float m_currentYRot;
	private float m_currentZRot;

	private float m_finalXValue;
	private float m_finalYValue;
	private float m_finalZValue;

	public string cardType;
	public bool isCardUp;
	int cnt = 0;

	public Material[] mats = new Material[CS.NUMCARDMATERIALS];
	public Material redMaterial;
	public Material blueMaterial;
	public Material civilMaterial;
	public Material deathMaterial;

	public string tmpStr = CS.EMPTY;

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

	// Call this to start the rotation
	public void makeFaceUp(int x, int z, Card siblingCaller){

		// This starts the rotation, could also use a boolean flag
		rotateFaceUp = 0;
		ChangeHighlight ();
		siblingCaller.ChangeHighlight();
	}

	// Use this for initialization
	void Start () {
		cnt = cnt + 1;
		//get the Initial rotation of the board and store it for reference and for setting up the current rotations
		m_currentXRot = m_initialXRot = transform.rotation.eulerAngles.x;
		m_currentYRot = m_initialYRot = transform.rotation.eulerAngles.y;
		m_currentZRot = m_initialZRot = transform.rotation.eulerAngles.z;

		Debug.Log ("X, Y and Z rotations are " + m_currentXRot + " " + m_currentYRot + " " + m_currentZRot);

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
		m_finalXValue = +180f;
		m_finalZValue = 0f;
		m_finalYValue = 0f;
		if ((m_currentXRot <= (m_initialXRot + m_finalXValue)*0.99) && rotateFaceUp >= 0)
		{
			rotateFaceUp += Time.deltaTime * m_finalXValue/m_tiltSpeed;

			m_currentXRot += rotateFaceUp;
			m_currentYRot += 0;
			m_currentZRot += 0;
 			
			//make sure that if it is close the target angle of rotation it doesn't over or under rotate
			if (m_currentXRot > (m_initialXRot + m_finalXValue) * 0.98)
			{
				m_currentXRot = m_initialXRot + m_finalXValue;
			}

			transform.localRotation = Quaternion.Euler (m_currentXRot, m_currentYRot, m_currentZRot);
		}        
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