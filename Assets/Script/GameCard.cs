using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;


//namespace AssemblyCSharp
//{
	[XmlRoot("GameCard")]
	public class GameCard
	{
		public string cardSuit { get; set; }
		public string cardLocation { get; set; }
		public string cardRevealed { get; set; }
		[XmlElement("CardWhenPlayable")]
		public List<CardWhenPlayable> cardWhenPlayable = new List<CardWhenPlayable>();
		[XmlElement("CardEffectPlayable")]
		public List<CardEffectPlayable> cardEffectPlayable = new List<CardEffectPlayable>();
	}

	public class CardWhenPlayable
	//Defines when the card is playable e.g. anytime, only on your tunr
	{
		public string turnStage{ get; set; }
		public string numTimes { get; set; }
	}

	public class CardEffectPlayable
	//Defines the affects the card has e.g. pick up two
	{
		public string effectName { get; set; }
		public string numTimes { get; set; }
	}
//}