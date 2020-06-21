using System.Collections.Generic;
using System.Linq;
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
            //Create each LobbyLineItem and them to the List of LineItems
            //Note the need to use ToList as players will be moved between teams inside the loop changing the collections and causing an error 
            foreach (var tempTeam in client.gClientGame.gameTeams.ToList()) foreach (var tempPlayer in tempTeam.teamPlayers.ToList())
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