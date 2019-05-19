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

        public void SetLobbyLineItem(GameClient gc)
        {
            lineItemText.text = gc.name;
            isCaller.isOn = !gc.isPlayer;
            isRedTeam.isOn = gc.isRedTeam;
            isBlueTeam.isOn = !gc.isRedTeam;
            clientID = gc.clientID;
        }

        private void SetGameClient()
        {
            var client = FindObjectOfType<Client>();

            for (var cnt = 0; cnt < client.players.Count; cnt++)
                if (string.Equals(client.players[cnt].clientID, clientID))
                {
                    client.players[cnt].name = lineItemText.text;
                    client.players[cnt].isPlayer = !isCaller.isOn;
                    client.players[cnt].isRedTeam = isRedTeam.isOn;
                    client.players[cnt].clientID = clientID;
                }
        }

        public void SelectUIComponent()
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