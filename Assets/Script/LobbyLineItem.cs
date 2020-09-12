using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Script
{
    public class LobbyLineItem : MonoBehaviour
    {
        public string clientID;
        public ToggleGroup toggleGroup;
        //public Toggle isBlueTeam;
        public Toggle isCaller;
        //public Toggle isRedTeam;
        public Text lineItemText;
        
        public GameObject ToggleTeamPrefab;
        public List<Toggle> ToggleTeamItems = new List<Toggle>();
        
        private string teamID;
        
        //used to position UI elements in space
        private readonly Vector3 toggleOffset = new Vector3(50.0f, 0f, 0f);
        
        // Use this for initialization
        private void Start()
        {
            //var line = Instantiate(lobbyLineItemPrefab).GetComponent<LobbyLineItem>();
            
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
            teamID = gt.id;
            lineItemText.text = tp.name;
            isCaller.isOn = !tp.isPlayer;
            
            //ToggleGroup toggleTeamGroup= GetComponent<ToggleGroup>();
            //Note we are using the "name" property of the team as it's ID
            foreach (var tempTeam in client.gClientGame.gameTeams.ToList())
            {
                var toggleTeam = Instantiate(ToggleTeamPrefab.GetComponent<Toggle>());
                ToggleTeamItems.Add(toggleTeam);
                toggleTeam.transform.SetParent(transform, false);
                toggleTeam.transform.Translate(toggleOffset*(ToggleTeamItems.Count-1));
                toggleTeam.name = tempTeam.id;
                toggleTeam.group = toggleGroup;

                // isRedTeam.isOn = gt.name == CS.RED_TEAM;
                // isRedTeam.name = "fixthis";
                // isBlueTeam.isOn = !isRedTeam.isOn;
                // isBlueTeam.name = gt.id;

                toggleTeam.isOn = toggleTeam.name == gt.id;

                //Having set defaults for what to display in the UI, reflect these back in the data structures
                //Obtain the Team ID, reading it back from the UI
                //TODO - Remove hardcoding of TEAMS
                //var tmpTeamId = (isRedTeam.isOn ? CS.RED_TEAM : (isBlueTeam.isOn ? CS.BLUE_TEAM:CS.NO_TEAM));
            }

            //Move the player to the correct team
            client.gClientGame.MovePlayerTeam(clientID, teamID);

            //Have to also set the GameClient
            //SetGameClient();
        }

        private void SetGameClient()
        {
            var client = FindObjectOfType<Client>();
            var MyLobbyGroup = FindObjectOfType<Lobby>();
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
                //START HERE - This returns a NULL
                var tmpTeamId = toggleGroup.ActiveToggles().First().name;

                //Move the player to the correct team
                client.gClientGame.MovePlayerTeam(clientID, tmpTeamId);
            }
        }

        public void SelectUIComponent() 
        //ensures the right number of callers are allowed
        {
            //TODO - Must be a more elegent way to achieve this rather than repeated scans
            var MyLobbyGroup = FindObjectOfType<Lobby>();

            //Count the number of times the caller toggles occur, if it occurs more than once uncheck 
            for (int i = 0; i < MyLobbyGroup.LobbyLineItems.Count; i++)
            {
                //Get count of current element to before:
                int count = MyLobbyGroup.LobbyLineItems.Take(i+1)
                    .Count(r =>
                    (
                        r.isCaller.isOn &&
                        r.toggleGroup.ActiveToggles().First().name== MyLobbyGroup.LobbyLineItems[i].toggleGroup.ActiveToggles().First().name)
                    );
                if(count > 1) MyLobbyGroup.LobbyLineItems[i+1].isCaller.isOn=false;
            }
            SetGameClient();
        }
    }
}