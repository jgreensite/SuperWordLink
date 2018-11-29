using System.Collections.Generic;
using System.Xml.Serialization;


//namespace AssemblyCSharp
//{
[XmlRoot("GameCard")]
public class GameCard
{
    [XmlElement("CardWhenPlayable")] public List<CardWhenPlayable> cardWhenPlayable = new List<CardWhenPlayable>();

    [XmlElement("CardEffectPlayable")]
    public List<CardEffectPlayable> cardEffectPlayable = new List<CardEffectPlayable>();

    public string cardID { get; set; }
    public string cardPlayerNum { get; set; }
    public string cardClientID { get; set; }
    public string cardSuit { get; set; }
    public string cardLocation { get; set; }
    public string cardRevealed { get; set; }
}

public class CardWhenPlayable
    //Defines when the card is playable e.g. anytime, only on your tunr
{
    public string turnStage { get; set; }
    public int numTimes { get; set; }
}

public class CardEffectPlayable
    //Defines the affects the card has e.g. pick up two
{
    public string effectName { get; set; }
    public string affectWhat { get; set; }
    public int numTimes { get; set; }
}
//}