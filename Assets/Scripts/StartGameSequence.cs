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

            yield return GetComponent<InitializeBoards>().SetUpBoards();

            instantiateDecks.SetUpDecks();

            GetComponent<DealCards>().SetHands();
            GetComponent<DealCards>().StartingDeal();

            foreach (SlotScript slotScript in FindObjectsByType<SlotScript>(sortMode: FindObjectsSortMode.None))
            {
                slotScript.InitializeSlots();
                slotScript.SetLastCardSettings();
            }

            GameManager.Instance.slotChangeActionsActive = true;
        }

    }
}
