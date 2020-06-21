using UnityEngine;
using UnityEngine.UI;

namespace Script
{
    public class LobbyLineItem : MonoBehaviour
    {
        public string clientID;
        public Toggle isBlueTeam;
        public Toggle isCaller;
        public Toggle isRedTeam;

        public Text lineItemText;

        // Use this for initialization
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }

        //public void SetLobbyLineItem(GameClient gc)
        public void SetLobbyLineItem(GameTeam gt, TeamPlayer tp)
        
        {
            var client = FindObjectOfType<Client>();
            
            //Set Default UI display for the LobbyLineItem
            clientID = tp.id;
            lineItemText.text = tp.name;
            isCaller.isOn = !tp.isPlayer;
            //TO DO - Remove harcoding of teams
            isRedTeam.isOn = gt.id==CS.RED_TEAM;
            isBlueTeam.isOn = !isRedTeam.isOn;
            
            //Having set defaults for what to display in the UI, reflect these back in the data structures
            //Obtain the Team ID, reading it back from the UI
            //TODO - Remove hardcoding of TEAMS
            var tmpTeamId = (isRedTeam.isOn ? CS.RED_TEAM : (isBlueTeam.isOn ? CS.BLUE_TEAM:CS.NO_TEAM));

            //Move the player to the correct team
            client.gClientGame.MovePlayerTeam(clientID, tmpTeamId);

            //Have to also set the GameClient
            //SetGameClient();
        }

        private void SetGameClient()
        {
            var client = FindObjectOfType<Client>();
            TeamPlayer gIncomingPlayer = new TeamPlayer();
            
            //Find the player associated with this lineitem
            client.gClientGame.FindPlayer(clientID, ref gIncomingPlayer);

            //Check that the player is found
            if (gIncomingPlayer != null)
            {
                //Set the player's caller status
                //TODO - Is this line needed?
                gIncomingPlayer.isPlayer = !isCaller.isOn;
                
                //Obtain the Team ID
                //todo - REMOVE HARDCODING FOR TWO TEAM IDS
                var tmpTeamId = (isRedTeam.isOn ? CS.RED_TEAM : (isBlueTeam.isOn ? CS.BLUE_TEAM:CS.NO_TEAM));

                //Move the player to the correct team
                client.gClientGame.MovePlayerTeam(clientID, tmpTeamId);
            }
        }

        public void SelectUIComponent()
        //ensures the right number of callers are allowed
        //todo - make the number of callers a user configurable parameter, like grid size
        {
            var MyLobbyGroup = FindObjectOfType<Lobby>();

            var redCallerCnt = 0;
            var blueCallerCnt = 0;
            for (var cnt = 0; cnt < MyLobbyGroup.LobbyLineItems.Count; cnt++)
            {
                if (MyLobbyGroup.LobbyLineItems[cnt].isCaller.isOn && MyLobbyGroup.LobbyLineItems[cnt].isRedTeam.isOn)
                    redCallerCnt += 1;
                if (redCallerCnt > PREFS.getPrefInt("NumCallers"))
                {
                    MyLobbyGroup.LobbyLineItems[cnt].isCaller.isOn = false;
                    redCallerCnt--;
                }

                if (MyLobbyGroup.LobbyLineItems[cnt].isCaller.isOn &&
                    MyLobbyGroup.LobbyLineItems[cnt].isRedTeam.isOn == false) blueCallerCnt += 1;
                if (blueCallerCnt > PREFS.getPrefInt("NumCallers"))
                {
                    MyLobbyGroup.LobbyLineItems[cnt].isCaller.isOn = false;
                    blueCallerCnt--;
                }
            }
            SetGameClient();
        }
    }
}