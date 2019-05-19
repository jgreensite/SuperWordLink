﻿using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

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

    [XmlRoot("GameCardDeck")]
    public class GameHandDeck
    {
        [XmlArray("GameCards")] [XmlArrayItem("GameCard")]
        public List<GameCard> gameCards = new List<GameCard>();

        public void Save(string path)
        {
            var serializer = new XmlSerializer(typeof(GameHandDeck));
            using (var stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, this);
            }
        }


        public string SaveToText()
        {
            var xmlSerializer = new XmlSerializer(typeof(GameHandDeck));

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }
        }


        public static GameHandDeck Load(string path)
        {
            var serializer = new XmlSerializer(typeof(GameHandDeck));
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return serializer.Deserialize(stream) as GameHandDeck;
            }
        }

        //Loads the xml directly from the given string. Useful in combination with www.text.
        public static GameHandDeck LoadFromText(string text)
        {
            var serializer = new XmlSerializer(typeof(GameHandDeck));
            return serializer.Deserialize(new StringReader(text)) as GameHandDeck;
        }
    }
}