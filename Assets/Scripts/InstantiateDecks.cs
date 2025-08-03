using UnityEngine;
using System.Collections.Generic;

namespace LogosTcg
{
    public class InstantiateDecks : MonoBehaviour
    {
        BoardElements be;

        [SerializeField] public List<DeckDefinition> deckFaithfulSo;
        [SerializeField] public DeckDefinition deckLocationSo;
        [SerializeField] public DeckDefinition deckEncounterSo;

        [SerializeField] private GameObject cardPrefab;

        private void Start()
        {
            be = BoardElements.instance;
        }

        public void SetUpDecks()
        {
            for (int i = 0; i < StaticData.playerNums; i++)
            {
                foreach (CardDef card in deckFaithfulSo[i].CardCollection)
                {
                    GameObject newCard = Instantiate(cardPrefab, be.faithfulDecks[i]);
                    newCard.GetComponent<Card>().Apply(card);

                }
            }

            foreach (CardDef card in deckEncounterSo.CardCollection)
            {
                GameObject newCard = Instantiate(cardPrefab, be.encountersDeck);
                newCard.GetComponent<Card>().Apply(card);
                
            }

            foreach (CardDef card in deckLocationSo.CardCollection)
            {
                GameObject newCard = Instantiate(cardPrefab, be.locDeck);
                newCard.GetComponent<Card>().Apply(card);
                
            }
        }
    }
}
