using System.Collections.Generic;
using System.Xml.Serialization;

namespace Script
{

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
        public int cardXPos { get; set; }
        public int cardZPos { get; set; }
        public string cardWord { get; set; }

        public string ValidMove(bool isRedTurn)
            // Check move is valid
        {
            var retVal = CS.ERROR;
            switch (cardSuit)
            {
                case CS.RED_TEAM:
                    if (isRedTurn)
                        retVal = CS.GOOD;
                    else
                        retVal = CS.BAD;
                    break;

                case CS.BLUE_TEAM:
                    if (!isRedTurn)
                        retVal = CS.GOOD;
                    else
                        retVal = CS.BAD;
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

            return retVal;
        }
    }

    public class CardWhenPlayable
        //Defines when the card is playable e.g. anytime, only on your turn
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
}