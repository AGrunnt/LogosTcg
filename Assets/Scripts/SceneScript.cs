using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogosTcg
{
    public class SceneScript : MonoBehaviour
    {
        [SerializeField] DeckDefinition encounter;
        [SerializeField] DeckDefinition location;
        [SerializeField] List<DeckDefinition> faithfulList;

        public void StartGame()
        {
            if(NetworkManager.Singleton == null)
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

                for (int i = 0; i <4; i++)
                {
                    faithfulList[i].CardCollection =
                        GetComponent<DeckSceneManager>()
                          .faithfulListTf[i]
                          .GetComponentsInChildren<CardLine>()
                          .Select(l => l.cardDef)
                          .ToList();
                }
                SceneManager.LoadSceneAsync("CreateDecks", LoadSceneMode.Single);
            } else
            {
                StartCoroutine(LoadPlayBoardCoroutine());
            }
        }

        private IEnumerator LoadPlayBoardCoroutine()
        {
            yield return null;
            // 3) now that everyone has their StaticData.playerNums set, load the scene
            NetworkManager.Singleton.SceneManager.LoadScene("CreateDecks", LoadSceneMode.Single);
        }
    }
}
