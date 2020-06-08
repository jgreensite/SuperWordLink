using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Linq;

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

    [XmlRoot("GameMessage")]
    public class GameMessage
    {
        [XmlAttribute] public String id { get; set; }
        
        [XmlAttribute] public String type { get; set; }
        
        [XmlAttribute] public String name { get; set; }
        
        [XmlElement("Sender")]
        public Sender sender = new Sender();
        
        [XmlElement("Receiver")]
        public Receiver receiver = new Receiver();
        
        [XmlElement("GameParameters")]
        public GameParameters gameParameters = new GameParameters();
        
        [XmlArray("GameBoardDecks")] [XmlArrayItem("GameBoardDeck")]
        public List<GameBoardDeck> GameBoardDecks = new List<GameBoardDeck>();
        
        [XmlArray("GameTeams")] [XmlArrayItem("GameTeam")]
        public List<GameTeam> gameTeams = new List<GameTeam>();
        public void FindPlayer(String fplayerID, ref TeamPlayer fPlayer)
        {
            foreach (var cntT in this.gameTeams)
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
        
        [XmlRoot("Sender")]
        public class Sender
        {
            [XmlAttribute] public String id { get; set; }

            [XmlAttribute] public String name { get; set; }
        }
        
        [XmlRoot("Receiver")]
        public class Receiver
        {
            [XmlAttribute] public String id { get; set; }

            [XmlAttribute] public String name { get; set; }
        }
        
        public void Save(string path)
        {
            var serializer = new XmlSerializer(typeof(GameMessage));
            using (var stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, this);
            }
        }

        public string SaveToText()
        {
            var xmlSerializer = new XmlSerializer(typeof(GameMessage));

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }
        }
        
        public static GameMessage Load(string path)
        {
            var serializer = new XmlSerializer(typeof(GameMessage));
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return serializer.Deserialize(stream) as GameMessage;
            }
        }

        //Loads the xml directly from the given string. Useful in combination with www.text.
        public static GameMessage LoadFromText(string text)
        {
            var serializer = new XmlSerializer(typeof(GameMessage));
            return serializer.Deserialize(new StringReader(text)) as GameMessage;
        }
    }
}