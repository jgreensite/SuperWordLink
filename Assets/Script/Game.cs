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

    [XmlRoot("Game")]
    public class Game
    {
        [XmlAttribute] public String id { get; set; }
        [XmlAttribute] public String name { get; set; }
        
        public string teamTurnId { get; set; }
        public string winStatus { get; set; }
        public int howManyPlaying { get; set; }
        public int sizeOfXDim { get; set; }
        public int sizeOfYDim{ get; set; }

        [XmlArray("GameEvents")] [XmlArrayItem("GameEvent")]
        public List<GameEvent> gameEvent = new List<GameEvent>();
        
        [XmlArray("GameTeams")] [XmlArrayItem("GameTeam")]
        public List<GameTeam> gameTeam = new List<GameTeam>();

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