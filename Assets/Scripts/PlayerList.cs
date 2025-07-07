using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace LogosTcg
{
    public class PlayerList : NetworkBehaviour
    {
        public GameObject playerListItemTemplate; // Assign the inactive Session Player List Item in the Inspector
        public Transform contentTransform; // Assign the "Content" GameObject in the Inspector

        public TextMeshProUGUI hostName;
        public TextMeshProUGUI clientName;

        public void resetContent()
        {
            foreach (Transform child in contentTransform)
            {
                Destroy(child.gameObject);
            }
        }

        public void AddPlayerItem(string playerName)
        {
            if (playerListItemTemplate != null && contentTransform != null)
            {
                // Instantiate a copy of the inactive template
                GameObject newItem = Instantiate(playerListItemTemplate, contentTransform);

                // Activate the new item
                newItem.SetActive(true);

                // Find the Player Name child object and update its TextMeshPro text
                TextMeshProUGUI playerNameText = newItem.GetComponent<TextMeshProUGUI>(); //newItem.transform.Find("Row/Name Container/Player Name").GetComponent<TextMeshProUGUI>();
                if (playerNameText != null)
                {
                    playerNameText.text = playerName;
                }
                else
                {
                    Debug.LogError("Player Name TextMeshPro component not found!");
                }
            }
            else
            {
                Debug.LogError("Missing references: Assign the inactive list item and content transform in the inspector.");
            }
        }

        public override void OnNetworkSpawn()
        {
            string nameStr;
            if (IsHost)
            {
                if (hostName.text.Length > 1)
                {
                    nameStr = hostName.text;
                }
                else
                {
                    nameStr = $"Player {NetworkManager.LocalClientId}";
                }
            }
            else
            {
                if (clientName.text.Length > 1)
                {
                    nameStr = clientName.text;
                }
                else
                {
                    nameStr = $"Player {NetworkManager.LocalClientId}";
                }
            }

            if (IsHost)
            {
                StaticData.playerNamesList = new List<string>();
                StaticData.playerNamesList.Add(nameStr);
                setPlayerList();
            }
            else
            {
                SendPlayerNameServerRpc(nameStr);
            }

        }

        
        [ServerRpc(RequireOwnership = false)]
        public void SendPlayerNameServerRpc(string name)
        {
            StaticData.playerNamesList.Add(name);
            SetGameVarsClientRpc(StaticData.seedNum, string.Join("~", StaticData.playerNamesList));

        }

        public void setPlayerList()
        {
            resetContent();
            foreach (var player in StaticData.playerNamesList)
            {
                AddPlayerItem(player.ToString());
            }
        }


        [ClientRpc]
        public void SetGameVarsClientRpc(int seedInt, string playerList)
        {

            StaticData.seedNum = seedInt;

            StaticData.playerNamesList = playerList.Split('~').ToList();
            setPlayerList();
        }
    }
}