using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.Netcode;

namespace LogosTcg
{
    public class InitializeBoards : NetworkBehaviour
    {
        public List<Transform> playerBoards;


        public void SetUpBoards()
        {
            // Only the server/host should drive despawning
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost) //check if exists
            {
                Debug.Log("hostSetup");
                StartCoroutine(DespawnNetworkBoard()); //corountine allows pause to let boards spawn
            }
            else if (NetworkManager.Singleton == null)
            {
                Debug.Log("offSetup");
                StartCoroutine(DespawnOfflineBoard());
            }
        }

        private IEnumerator DespawnNetworkBoard()
        {
            yield return null;

            // For each board index >= activeBoardCount, tear it down
            for (int i = 3; i > StaticData.playerNums - 1; i--)
            {
                var boardTransform = playerBoards[i];
                Debug.Log($"Despawn {boardTransform.transform.name}");

                // 1) Find all slot NetworkObjects under this board
                var slotNetObjs = boardTransform
                    .GetComponentsInChildren<NetworkObject>()
                    // optionally skip the board itself if it ever had a NO
                    .Where(no => no.IsSpawned)
                    .ToList();

                // 2) Despawn each slot (destroy:true tells Netcode to destroy the GO on all clients)
                foreach (var slot in slotNetObjs)
                {
                    Debug.Log($"Despawn {slot.transform.name}");
                    slot.Despawn(destroy: true);
                }
            }

            // 3) Finally, destroy the (non-networked) board container
            DestroyUnusedGroupsClientRpc();
        }

        [ClientRpc]
        public void DestroyUnusedGroupsClientRpc()
        {
            StartCoroutine(DespawnOfflineBoard());
        }

        private IEnumerator DespawnOfflineBoard()
        {
            Debug.Log("deleteBoards");
            yield return null;

            // For each board index >= activeBoardCount, tear it down
            for (int i = 3; i > StaticData.playerNums -1; i--)
            {
                var boardTransform = playerBoards[i];
                Debug.Log($"delete {boardTransform.transform.name}");
                // 3) Finally, destroy the (non-networked) board container
                Destroy(boardTransform.gameObject);
            }
        }
    }
}