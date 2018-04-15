using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddressDirectory
{
	public string cardSuit { get; set; }
	public string cardLocation { get; set; }
	public string cardRevealed { get; set; }
	[XmlElement("cardPlayable")]
	public List<CardPlayable> addressList = new List<CardPlayable>(); 
}

public class CardPlayable
//Defines when the card is playable e.g. anytime, only on your tunr
{
	public string cardPlayableAttribute { get; set; }
	public string cardPlayableAttributeNumTimes { get; set; }
}

public class CardAffect
//Defines the affects the card has e.g. pick up two
{
	public string cardAffectAttribute { get; set; }
	public string cardAffectAttributeNumTimes { get; set; }
}