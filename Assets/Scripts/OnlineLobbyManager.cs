using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace LogosTcg
{
    public class OnlineLobbyManager : NetworkBehaviour
    {
        public void LoadPlayBoard()
        {
            // start the single coroutine that does everything in order
            StartCoroutine(LoadPlayBoardCoroutine());
        }

        private IEnumerator LoadPlayBoardCoroutine()
        {
            // 1) send the RPC to set StaticData on ALL clients (host included)
            //    note: pass the count as a parameter so you don’t have to read
            //    NetworkManager on the client side
            SetStaticDataClientRpc(NetworkManager.Singleton.ConnectedClients.Count);

            // 2) wait a frame so that the RPC has actually been sent & applied
            //    you can also yield return new WaitForSeconds(0.1f) if you find
            //    that one frame isn’t always enough
            //yield return null;
            yield return new WaitForSeconds(0.1f);

            // 3) now that everyone has their StaticData.playerNums set, load the scene
            NetworkManager.Singleton.SceneManager.LoadScene("CreateDecks", LoadSceneMode.Single);
        }

        // include the player count as an argument so clients don't need to query NM
        [ClientRpc]
        void SetStaticDataClientRpc(int count, ClientRpcParams rpcParams = default)
        {
            StaticData.playerNums = count;
        }
    }

}
