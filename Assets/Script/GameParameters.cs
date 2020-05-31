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

    [XmlRoot("GameParameters")]
    public class GameParameters
    {
        [XmlElement] public string teamTurnId { get; set; }
        [XmlElement] public string winStatus { get; set; }
        [XmlElement] public int howManyPlaying { get; set; }
        [XmlElement] public int sizeOfXDim { get; set; }
        [XmlElement] public int sizeOfYDim{ get; set; }

        public void Save(string path)
        {
            var serializer = new XmlSerializer(typeof(GameParameters));
            using (var stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, this);
            }
        }

        public string SaveToText()
        {
            var xmlSerializer = new XmlSerializer(typeof(GameParameters));

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }
        }


        public static GameParameters Load(string path)
        {
            var serializer = new XmlSerializer(typeof(GameParameters));
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return serializer.Deserialize(stream) as GameParameters;
            }
        }

        //Loads the xml directly from the given string. Useful in combination with www.text.
        public static GameParameters LoadFromText(string text)
        {
            var serializer = new XmlSerializer(typeof(GameMessage));
            return serializer.Deserialize(new StringReader(text)) as GameParameters;
        }
    }
}