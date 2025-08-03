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
        BoardElements be;

        private void Start()
        {
            be = BoardElements.instance;
        }

        public IEnumerator SetUpBoards()
        {
            // Only the server/host should drive despawning
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost) //check if exists
            {
                yield return StartCoroutine(DespawnNetworkBoard()); //corountine allows pause to let boards spawn
            }
            else if (NetworkManager.Singleton == null)
            {
                yield return StartCoroutine(DespawnOfflineBoard());
            }
        }

        private IEnumerator DespawnNetworkBoard()
        {
            yield return null;

            // For each board index >= activeBoardCount, tear it down
            for (int i = 3; i > StaticData.playerNums - 1; i--)
            {
                var boardTransform = be.playerBoards[i];

                // 1) Find all slot NetworkObjects under this board
                var slotNetObjs = boardTransform
                    .GetComponentsInChildren<NetworkObject>()
                    // optionally skip the board itself if it ever had a NO
                    .Where(no => no.IsSpawned)
                    .ToList();

                // 2) Despawn each slot (destroy:true tells Netcode to destroy the GO on all clients)
                foreach (var slot in slotNetObjs)
                {
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
            yield return null;

            // For each board index >= activeBoardCount, tear it down
            for (int i = 3; i > StaticData.playerNums -1; i--)
            {
                var boardTransform = be.playerBoards[i];
                // 3) Finally, destroy the (non-networked) board container
                Destroy(boardTransform.gameObject);
            }
        }
    }
}