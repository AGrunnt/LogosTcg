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

        [SerializeField] private GameObject cardPrefabNeutral;
        [SerializeField] private GameObject cardPrefabSupport;
        [SerializeField] private GameObject cardPrefabFaithful;
        [SerializeField] private GameObject cardPrefabFaithless;
        [SerializeField] private GameObject cardPrefabLocation;
        [SerializeField] private GameObject cardPrefabTrap;

        CardDef ca;

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
                    GameObject newCard = Instantiate(cardPrefabFaithful, be.faithfulDecks[i]);
                    newCard.GetComponent<Card>().Apply(card);

                }
            }

            foreach (CardDef card in deckEncounterSo.CardCollection)
            {
                ca = card;
                GameObject prefabType = null;

                switch(card.Type[0])
                {
                    case "Neutral":
                        prefabType = cardPrefabNeutral;
                        break;
                    case "Faithless":
                        prefabType = cardPrefabFaithless;
                        break;
                    case "Support":
                        prefabType = cardPrefabSupport;
                        break;
                    case "Trap":
                        prefabType = cardPrefabTrap;
                        break;
                    default:
                        Debug.Log("error");
                        prefabType = cardPrefabFaithful;
                        break;
                }


                GameObject newCard = Instantiate(prefabType, be.encountersDeck);
                newCard.GetComponent<Card>().Apply(card);
                
            }

            foreach (CardDef card in deckLocationSo.CardCollection)
            {
                GameObject newCard = Instantiate(cardPrefabLocation, be.locDeck);
                newCard.GetComponent<Card>().Apply(card);
                
            }
        }
    }
}
