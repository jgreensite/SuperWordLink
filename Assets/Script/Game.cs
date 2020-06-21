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

        [XmlElement("GameParmeters")]
        public GameParameters gameParameters = new GameParameters();

        [XmlArray("GameBoardDecks")] [XmlArrayItem("GameBoardDeck")]
        public List<GameBoardDeck> GameBoardDecks = new List<GameBoardDeck>();

        [XmlArray("GameTeams")] [XmlArrayItem("GameTeam")]
        public List<GameTeam> gameTeams = new List<GameTeam>();
        
        [XmlArray("GameMessages")] [XmlArrayItem("GameMessages")]
        public List<GameMessage> GameMessages = new List<GameMessage>();

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

        public void FindPlayerTeam (String fplayerID, ref GameTeam fTeam)
        {
            foreach (var cntT in this.gameTeams)
            {
                var cntP = cntT.teamPlayers.FirstOrDefault(player => player.id == fplayerID);
                if (cntP != null)
                {
                    fTeam = cntT;
                }
            }

            if ((fTeam.id == null) || (fTeam.id == ""))
            {
                fTeam = null;
            }
        }
        
        public void MovePlayerTeam (String mplayerID, string mTeamID)
        //upserts a player and a team as specified by the ID
        {
            GameTeam fTeam = new GameTeam();
            TeamPlayer fPlayer = new TeamPlayer();
            
            //find the player in its existing team
            foreach (var cntT in this.gameTeams)
            {
                var cntP = cntT.teamPlayers.FirstOrDefault(player => player.id == mplayerID);
                if (cntP != null)
                {
                    fPlayer = cntP;
                    fTeam = cntT;
                }
            }

            //if no player is found then create a new player
            if (fPlayer.id == null) fPlayer = new TeamPlayer();
            fPlayer.id = mplayerID;

            //remove the player from its old team if it existed in a different team to the one wanted
            if (fTeam.id != mTeamID)
            {
                //make sure the team you are moving to exists, if not create it
                var checkTeam = gameTeams.FirstOrDefault(team => team.id == mTeamID);
                if (checkTeam == null)
                {
                    GameTeam newTeam = new GameTeam();
                    newTeam.id = mTeamID;
                    gameTeams.Add(newTeam);
                }
                
                //remove from old team, will remove it if found
                fTeam.teamPlayers.Remove(fPlayer);
                
                //add to new team
                gameTeams.FirstOrDefault(team => team.id == mTeamID).teamPlayers.Add(fPlayer);
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