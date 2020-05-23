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
        public void SetLobbyLineItem(GameTeam gt,TeamPlayer tp)
        
        {
            lineItemText.text = tp.name;
            isCaller.isOn = !tp.isPlayer;
            isRedTeam.isOn = gt.id==CS.RED_TEAM;
            isBlueTeam.isOn = !isRedTeam.isOn;
            clientID = tp.id;
        }

        private void SetGameClient()
        {
            var client = FindObjectOfType<Client>();

            for (var cntTeam = 0; cntTeam < client.gClientGame.gameTeam.Count; cntTeam++)
                for (var cntPlayer = 0; cntPlayer < client.gClientGame.gameTeam[cntTeam].teamPlayers.Count; cntPlayer++)
                if (string.Equals(client.gClientGame.gameTeam[cntTeam].teamPlayers[cntPlayer].id, client.gClientPlayer.id))
                {
                    //client.gClientGame.gameTeam[cntTeam].teamPlayers[cntPlayer].name = lineItemText.text;
                    client.gClientGame.gameTeam[cntTeam].teamPlayers[cntPlayer].isPlayer = !isCaller.isOn;
                    client.gClientGame.gameTeam[cntTeam].id = isRedTeam.isOn?CS.RED_TEAM:CS.BLUE_TEAM;
                    //client.gClientGame.gameTeam[cntTeam].teamPlayers[cntPlayer].id = client.gClientPlayer.id;
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
                if (redCallerCnt > 1)
                {
                    MyLobbyGroup.LobbyLineItems[cnt].isCaller.isOn = false;
                    redCallerCnt--;
                }

                if (MyLobbyGroup.LobbyLineItems[cnt].isCaller.isOn &&
                    MyLobbyGroup.LobbyLineItems[cnt].isRedTeam.isOn == false) blueCallerCnt += 1;
                if (blueCallerCnt > 1)
                {
                    MyLobbyGroup.LobbyLineItems[cnt].isCaller.isOn = false;
                    blueCallerCnt--;
                }
            }

            SetGameClient();
        }
    }
}