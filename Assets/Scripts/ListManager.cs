using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using UnityEngine.AddressableAssets;

namespace LogosTcg
{
    public class ListManager : MonoBehaviour
    {
        public Dictionary<string, GameObject> listItems = new();
        //public Transform cardListTf;
        //public HashSet<string> listAssigned = new();

        public static ListManager instance;
        ListOnlineManager lom;
        FaithfulListsManager flm;
        GridManager gm;
        CardLoader cl;

        void Awake()
        {
            instance = this;
            lom = GetComponent<ListOnlineManager>();
            flm = FaithfulListsManager.instance;
            gm = GridManager.instance;
            cl = CardLoader.instance;
        }
        
        DeckSceneManager dsm;
        public GameObject cardLinePrefab;

        

        void Start()
        {
            dsm = DeckSceneManager.instance;
        }

        // ---------------------------------------------------
        // Move a grid?card into one of the three lists:
        public void AddToList(string key)
        {
            GameObject obj = gm.gridItems[key];
            Card c = obj.GetComponent<Card>();
            var cd = c._definition;

            bool isFaithful = cd.Type.Contains("Faithful");
            bool isLocation = cd.Type.Contains("Location");
            bool isEncounter = !isFaithful && !isLocation;

            // ?? enforce per?list & per?rarity caps ???????????????????????
            if (isEncounter && dsm.encounterListTf.childCount >= 54)
            {
                Debug.LogWarning("Cannot add more than 54 Encounter cards.");
                return;
            }

            if (isLocation && dsm.locationListTf.childCount >= 10)
            {
                Debug.LogWarning("Cannot add more than 10 Location cards.");
                return;
            }

            if (isFaithful && !flm.ValidFaithful(cd))
            {
                return;
            }
            // ?????????????????????????????????????????????????????????????

            int listType = c._definition.Type.Contains("Faithful") ? 0
                         : c._definition.Type.Contains("Location") ? 1
                         : 2;

            if (NetworkManager.Singleton != null)
            {
                lom.AddToOnlineListServerRpc(key, dsm.currPlayer, listType);
                return;
            }

            // 3) spawn a CardLine in the right list
            var parent = isFaithful ? dsm.faithfulListTf[GetComponent<DeckSceneManager>().currPlayer]
                       : isLocation ? dsm.locationListTf
                                     : dsm.encounterListTf;

            var line = Instantiate(cardLinePrefab, parent)
                       .GetComponent<CardLine>();

            line.cardDef = cd;
            line.Apply();
            line.addressableKey = key;

            // 4) destroy the grid object + clear our internal map
            CardLoader.instance.RemoveCardMapping(key, false);

            // 2) mark it assigned so loader never spawns it
            //listAssigned.Add(key);
            listItems.Add(key, line.gameObject);

            // 5) update our stats display
            if (isFaithful) GetComponent<DeckSceneManager>().UpdateFaithfulStats();
        }

        public void RemoveFromList(string key)
        {
            string test = key;
            GameObject obj = listItems[key];
            var line = obj.GetComponent<CardLine>();
            var cd = line.cardDef;
            //var key = line.addressableKey;
            int listType = line.cardDef.Type.Contains("Faithful") ? 0
                         : line.cardDef.Type.Contains("Location") ? 1
                         : 2;

            if (NetworkManager.Singleton != null)
            {
                lom.RemoveFromOnlineListServerRpc(key, listType, dsm.currPlayer);
                return;
            }
            bool added = gm.AddCardToGrid(key);
            cl.RemoveCardMapping(key, !added);

            // 4) if it was a Faithful line, update stats
            if (listType == 0)
                GetComponent<DeckSceneManager>().UpdateFaithfulStats();

        }

    }
}
