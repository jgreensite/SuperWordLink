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

    [XmlRoot("TeamPlayer")]
    public class TeamPlayer
    {
        [XmlAttribute] public String id { get; set; }
        [XmlAttribute] public String name { get; set; }
        
        public bool isHost { get; set; }
        public bool isPlayer { get; set; }
        //public bool isRedTeam { get; set; }

        public GameHandDeck gameHandDeck = new GameHandDeck();

        public void Save(string path)
        {
            var serializer = new XmlSerializer(typeof(TeamPlayer));
            using (var stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, this);
            }
        }

        public string SaveToText()
        {
            var xmlSerializer = new XmlSerializer(typeof(TeamPlayer));

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }
        }


        public static TeamPlayer Load(string path)
        {
            var serializer = new XmlSerializer(typeof(TeamPlayer));
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return serializer.Deserialize(stream) as TeamPlayer;
            }
        }

        //Loads the xml directly from the given string. Useful in combination with www.text.
        public static TeamPlayer LoadFromText(string text)
        {
            var serializer = new XmlSerializer(typeof(TeamPlayer));
            return serializer.Deserialize(new StringReader(text)) as TeamPlayer;
        }
    }
}