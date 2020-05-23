using System.Collections.Generic;
using UnityEngine;

namespace Script
{
    public class Lobby : MonoBehaviour
    {
        public GameObject lobbyLineItemPrefab;
        public List<LobbyLineItem> LobbyLineItems = new List<LobbyLineItem>();

        // Use this for initialization
        private void Start()
        {
            var client = FindObjectOfType<Client>();
            //Create each LineItem and them to the List of LineItems
            foreach (var tempTeam in client.gClientGame.gameTeam) foreach (var tempPlayer in tempTeam.teamPlayers)
            {
                var line = Instantiate(lobbyLineItemPrefab).GetComponent<LobbyLineItem>();
                LobbyLineItems.Add(line);
                line.transform.SetParent(transform, false);
                line.SetLobbyLineItem(tempTeam, tempPlayer);
            }
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}