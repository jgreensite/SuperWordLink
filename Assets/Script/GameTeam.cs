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

    [XmlRoot("GameTeam")]
    public class GameTeam
    {
        [XmlAttribute] public String id { get; set; }
        [XmlAttribute] public String name { get; set; }
        
        [XmlArray("TeamPlayers")] [XmlArrayItem("TeamPlayer")]
        public List<TeamPlayer> teamPlayers = new List<TeamPlayer>();

        public void Save(string path)
        {
            var serializer = new XmlSerializer(typeof(GameTeam));
            using (var stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, this);
            }
        }

        public string SaveToText()
        {
            var xmlSerializer = new XmlSerializer(typeof(GameTeam));

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }
        }


        public static GameTeam Load(string path)
        {
            var serializer = new XmlSerializer(typeof(GameTeam));
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return serializer.Deserialize(stream) as GameTeam;
            }
        }

        //Loads the xml directly from the given string. Useful in combination with www.text.
        public static GameTeam LoadFromText(string text)
        {
            var serializer = new XmlSerializer(typeof(GameTeam));
            return serializer.Deserialize(new StringReader(text)) as GameTeam;
        }
    }
}