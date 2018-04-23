using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;
using AssemblyCSharp;  

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

[XmlRoot("GameCardDeck")]
public class GameCardDeck
{
	[XmlArray("GameCards"),XmlArrayItem("GameCard")]
	public List<GameCard> gameCards = new List<GameCard>();

	public void Save(string path)
	{
		var serializer = new XmlSerializer(typeof(GameCardDeck));
		using(var stream = new FileStream(path, FileMode.Create))
		{
			serializer.Serialize(stream, this);
		}
	}

	public static GameCardDeck Load(string path)
	{
		var serializer = new XmlSerializer(typeof(GameCardDeck));
		using(var stream = new FileStream(path, FileMode.Open))
		{
			return serializer.Deserialize(stream) as GameCardDeck;
		}
	}

	//Loads the xml directly from the given string. Useful in combination with www.text.
	public static GameCardDeck LoadFromText(string text) 
	{
		var serializer = new XmlSerializer(typeof(GameCardDeck));
		return serializer.Deserialize(new StringReader(text)) as GameCardDeck;
	}
}