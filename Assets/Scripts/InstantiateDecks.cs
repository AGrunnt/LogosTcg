using UnityEngine;

namespace LogosTcg
{
    public class InstantiateDecks : MonoBehaviour
    {
        PopulateDecks populateDecks;
        public Transform faithful;
        public Transform locations;
        public Transform encounters;

        [SerializeField] private GameObject cardPrefab;

        private void Start()
        {
            populateDecks = GetComponent<PopulateDecks>();
        }

        public void SetUpDecks()
        {   
            foreach(CardDef card in populateDecks.deckFaithful.CardCollection)
            {
                GameObject newCard = Instantiate(cardPrefab, faithful);
                newCard.GetComponent<Card>().Apply(card);
                
            }

            foreach (CardDef card in populateDecks.deckEncounter.CardCollection)
            {
                GameObject newCard = Instantiate(cardPrefab, encounters);
                newCard.GetComponent<Card>().Apply(card);
                
            }

            foreach (CardDef card in populateDecks.deckLocation.CardCollection)
            {
                GameObject newCard = Instantiate(cardPrefab, locations);
                newCard.GetComponent<Card>().Apply(card);
                
            }

            foreach( SlotScript slotScript in FindObjectsByType<SlotScript>(sortMode: FindObjectsSortMode.None))
            {
                slotScript.InitializeSlots();
                slotScript.SetLastCardSettings();
                slotScript.networkActive = true;
            }
        }
    }
}
