using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Linq;
using UnityEditorInternal.VersionControl;

//static public void Serialize(AddressDetails details)
//{ 
//	XmlSerializer serializer = new XmlSerializer(typeof(AddressDetails)); 
//	using (TextWriter writer = new StreamWriter(@"C:\Xml.xml"))
//	{
//		serializer.Serialize(writer, details); 
//	} 
//}

//public static void ReadFromFile(string[] args)
//{
//	XmlSerializer deserializer = new XmlSerializer(typeof(Address));
//	TextReader reader = new StreamReader(@"tmp/myXml.xml");
//	object obj = deserializer.Deserialize(reader);
//	Address XmlData = (Address)obj;
//	reader.Close();
//}
namespace Script
{

    [XmlRoot("Game")]
    public class Game
    {
        [XmlAttribute] public String id { get; set; }
        [XmlAttribute] public String name { get; set; }

        [XmlElement("GameParmetrs")]
        public GameParameters gameParameters = new GameParameters();

        [XmlArray("GameBoardDecks")] [XmlArrayItem("GameBoardDeck")]
        public List<GameBoardDeck> gameBoardDeck = new List<GameBoardDeck>();

        [XmlArray("GameTeams")] [XmlArrayItem("GameTeam")]
        public List<GameTeam> gameTeam = new List<GameTeam>();
        
        [XmlArray("GameMessages")] [XmlArrayItem("GameMessages")]
        public List<GameMessage> GameMessages = new List<GameMessage>();

        public void FindPlayer(String fplayerID, ref TeamPlayer fPlayer)
        {
            foreach (var cntT in this.gameTeam)
            {
                var cntP = cntT.teamPlayers.FirstOrDefault(player => player.id == fplayerID);
                if (cntP != null)
                {
                    fPlayer = cntP;
                }
            }

            if ((fPlayer.id == null) || (fPlayer.id == ""))
            {
                fPlayer = null;
            }
        }

        public void Save(string path)
        {
            var serializer = new XmlSerializer(typeof(Game));
            using (var stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, this);
            }
        }

        public string SaveToText()
        {
            var xmlSerializer = new XmlSerializer(typeof(Game));

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }
        }


        public static Game Load(string path)
        {
            var serializer = new XmlSerializer(typeof(Game));
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return serializer.Deserialize(stream) as Game;
            }
        }

        //Loads the xml directly from the given string. Useful in combination with www.text.
        public static Game LoadFromText(string text)
        {
            var serializer = new XmlSerializer(typeof(Game));
            return serializer.Deserialize(new StringReader(text)) as Game;
        }
    }
}