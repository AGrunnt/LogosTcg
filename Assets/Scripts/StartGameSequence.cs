using LogoTcg;
using System.Collections;
using UnityEngine;

namespace LogosTcg
{
    public class StartGameSequence : MonoBehaviour
    {
        InstantiateDecks instantiateDecks;
        PopulateDecks populateDecks;
        public bool test = false;

        void Start()
        {
            instantiateDecks = GetComponent<InstantiateDecks>();
            populateDecks = GetComponent<PopulateDecks>();

            StartCoroutine(Sequence());
        }

        private IEnumerator Sequence()
        {
            yield return null;
            
            if(test)
                yield return populateDecks.LoadAndPartitionBaseSet();

            GetComponent<InitializeBoards>().SetUpBoards();
            instantiateDecks.SetUpDecks();
            //GetComponent<DealCards>().SetHands();
            GetComponent<DealCards>().StartingDeal();

            foreach (SlotScript slotScript in FindObjectsByType<SlotScript>(sortMode: FindObjectsSortMode.None))
            {
                slotScript.InitializeSlots();
                slotScript.SetLastCardSettings();
            }

            GameManager.Instance.setUpFinished = true;
        }

    }
}
