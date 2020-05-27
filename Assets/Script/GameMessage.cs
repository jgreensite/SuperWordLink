using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System;

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
        [XmlElement] public Sender sender { get; set; }
        
        [XmlElement] public Receiver receiver { get; set; }
        [XmlElement] public GameParameters gameParameters { get; set; }
        
        [XmlArray("GameBoardDecks")] [XmlArrayItem("GameBoardDeck")]
        public List<GameBoardDeck> gameBoardDeck = new List<GameBoardDeck>();
        
        [XmlArray("GameTeams")] [XmlArrayItem("GameTeam")]
        public List<GameTeam> gameTeam = new List<GameTeam>();

        public class Sender
        {
            [XmlAttribute] public String id { get; set; }

            [XmlAttribute] public String name { get; set; }
        }

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