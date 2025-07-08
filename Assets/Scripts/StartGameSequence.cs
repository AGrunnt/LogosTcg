using LogoTcg;
using System.Collections;
using UnityEngine;

namespace LogosTcg
{
    public class StartGameSequence : MonoBehaviour
    {
        InstantiateDecks instantiateDecks;
        PopulateDecks populateDecks;

        void Start()
        {
            instantiateDecks = GetComponent<InstantiateDecks>();
            populateDecks = GetComponent<PopulateDecks>();


            StartCoroutine(Sequence());
        }

        private IEnumerator Sequence()
        {

            yield return populateDecks.LoadAndPartitionBaseSet();

            GetComponent<InitializeBoards>().SetUpBoards();

            instantiateDecks.SetUpDecks();


        }
    }
}
