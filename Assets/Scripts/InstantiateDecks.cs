using UnityEngine;
using System;
using System.Collections.Generic;

namespace LogosTcg
{
    public class InstantiateDecks : MonoBehaviour
    {
        BoardElements be;

        [SerializeField] public List<DeckDefinition> deckFaithfulSo;
        [SerializeField] public DeckDefinition deckLocationSo;
        [SerializeField] public DeckDefinition deckEncounterSo;

        [SerializeField]
        private GameObject cardPrefabNeutral, cardPrefabSupport, cardPrefabFaithful,
                                            cardPrefabFaithless, cardPrefabLocation, cardPrefabTrap,
                                            cardPrefabEventBase, cardPrefabEventValue;

        void Start() => be = BoardElements.instance;

        public void SetUpDecks()
        {
            // --- Faithful (per-player shuffle) ---
            for (int i = 0; i < StaticData.playerNums; i++)
            {
                // copy then shuffle with a seed that’s stable but different per player
                var cards = new List<CardDef>(deckFaithfulSo[i].CardCollection);
                Shuffle(cards, seed: StaticData.seedNum + 1009 * i);

                foreach (var card in cards)
                {
                    var go = Instantiate(cardPrefabFaithful, be.faithfulDecks[i], false);
                    go.GetComponent<Card>().Apply(card);
                    // Optional: control top/bottom
                    // go.transform.SetAsLastSibling();   // top = last
                }
            }

            // --- Encounter deck ---
            {
                var cards = new List<CardDef>(deckEncounterSo.CardCollection);
                Shuffle(cards, seed: StaticData.seedNum + 0xE11C);   // different salt

                foreach (var card in cards)
                {
                    var prefab = PickEncounterPrefab(card);
                    var go = Instantiate(prefab, be.encountersDeck, false);
                    go.GetComponent<Card>().Apply(card);
                }
            }

            // --- Location deck ---
            {
                var cards = new List<CardDef>(deckLocationSo.CardCollection);
                Shuffle(cards, seed: StaticData.seedNum + 0x10CA1);

                foreach (var card in cards)
                {
                    var go = Instantiate(cardPrefabLocation, be.locDeck, false);
                    go.GetComponent<Card>().Apply(card);
                }
            }
        }

        static void Shuffle<T>(IList<T> list, int seed)
        {
            var rng = new System.Random(seed);              // local RNG; does not touch UnityEngine.Random
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);                   // 0..i inclusive
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        GameObject PickEncounterPrefab(CardDef card)
        {
            switch (card.Type[0])
            {
                case "Neutral": return cardPrefabNeutral;
                case "Faithless": return cardPrefabFaithless;
                case "Support": return cardPrefabSupport;
                case "Trap": return cardPrefabTrap;
                case "Event": return card.Value == 0 ? cardPrefabEventBase : cardPrefabEventValue;
                default: Debug.LogWarning("Unknown type, defaulting to Faithful"); return cardPrefabFaithful;
            }
        }
    }
}


/*
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
        [SerializeField] private GameObject cardPrefabEventBase;
        [SerializeField] private GameObject cardPrefabEventValue;

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
                    case "Event":
                        if(card.Value == 0)
                            prefabType = cardPrefabEventBase;
                        else
                            prefabType = cardPrefabEventValue;

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
*/