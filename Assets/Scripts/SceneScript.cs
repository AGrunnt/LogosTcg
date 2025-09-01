using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogosTcg
{
    public class SceneScript : NetworkBehaviour
    {
        [SerializeField] DeckDefinition encounter;
        [SerializeField] DeckDefinition location;
        [SerializeField] List<DeckDefinition> faithfulList;

        public void StartGame()
        {
            

            //return;
            if (NetworkManager.Singleton == null)
            {
                encounter.CardCollection =
                    GetComponent<DeckSceneManager>()
                      .encounterListTf
                      .GetComponentsInChildren<CardLine>()
                      .Select(l => l.cardDef)
                      .ToList();

                location.CardCollection =
                    GetComponent<DeckSceneManager>()
                      .locationListTf
                      .GetComponentsInChildren<CardLine>()
                      .Select(l => l.cardDef)
                      .ToList();

                for (int i = 0; i < StaticData.playerNums; i++)
                {
                    //Debug.Log($"list item {i}");
                    faithfulList[i].CardCollection =
                        GetComponent<DeckSceneManager>()
                          .faithfulListTf[i]
                          .GetComponentsInChildren<CardLine>()
                          .Select(l => l.cardDef)
                          .ToList();
                }

                SceneManager.LoadSceneAsync("PlayBoard", LoadSceneMode.Single);
                //NetworkManager.Singleton.SceneManager.LoadScene("PlayBoard", LoadSceneMode.Single);
            }
            else
            {
                SetSoClientRpc();

                if (NetworkManager.Singleton.IsHost)
                    StartCoroutine(DelayedSceneLoadCoroutine());
            }
        }

        [ClientRpc]
        void SetSoClientRpc()
        {
            encounter.CardCollection =
                    GetComponent<DeckSceneManager>()
                      .encounterListTf
                      .GetComponentsInChildren<CardLine>()
                      .Select(l => l.cardDef)
                      .ToList();

            location.CardCollection =
                GetComponent<DeckSceneManager>()
                  .locationListTf
                  .GetComponentsInChildren<CardLine>()
                  .Select(l => l.cardDef)
                  .ToList();

            for (int i = 0; i < StaticData.playerNums; i++)
            {
                faithfulList[i].CardCollection =
                    GetComponent<DeckSceneManager>()
                      .faithfulListTf[i]
                      .GetComponentsInChildren<CardLine>()
                      .Select(l => l.cardDef)
                      .ToList();
            }
        }

        private IEnumerator DelayedSceneLoadCoroutine()
        {
            // Give RPC a chance to propagate
            yield return new WaitForSeconds(0.5f);  // Half a second delay to allow RPC to send
            NetworkManager.Singleton.SceneManager.LoadScene("PlayBoard", LoadSceneMode.Single);
        }
    }
}
