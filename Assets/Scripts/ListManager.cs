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
        public bool offline = true;
        public List<string> tempListItems;
        //public Transform cardListTf;
        //public HashSet<string> listAssigned = new();

        public static ListManager instance;
        ListOnlineManager lom;
        FaithfulListsManager flm;
        GridManager gm;
        CardLoader cl;

        public List<GameObject> listItemsView = new();

        void Awake()
        {
            instance = this;

        }

        private void Update()
        {
            tempListItems = listItems.Keys.ToList();
            listItemsView = listItems.Values.ToList();
        }



        DeckSceneManager dsm;
        public GameObject cardLinePrefab;

        

        void Start()
        {
            lom = GetComponent<ListOnlineManager>();
            flm = FaithfulListsManager.instance;
            gm = GridManager.instance;
            cl = CardLoader.instance;
            dsm = DeckSceneManager.instance;
            //dsm = DeckSceneManager.instance;
        }

        // ---------------------------------------------------
        // Move a grid?card into one of the three lists:
        public void AddToList(string key)
        {
            if (!TryGetCardDefForKey(key, out var cd))
            {
                Debug.LogWarning($"AddToList: missing CardDef for key {key}");
                return;
            }

            bool isFaithful = cd.Type.Contains("Faithful");
            bool isLocation = cd.Type.Contains("Location");
            bool isEncounter = !isFaithful && !isLocation;

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
            if (isFaithful && !flm.ValidFaithful(cd)) return;

            int listType = isFaithful ? 0 : isLocation ? 1 : 2;

            if (NetworkManager.Singleton != null)
            {
                lom.AddToOnlineListServerRpc(key, dsm.currPlayer, listType);
                return;
            }

            var parent = isFaithful ? dsm.faithfulListTf[GetComponent<DeckSceneManager>().currPlayer]
                                    : isLocation ? dsm.locationListTf
                                                 : dsm.encounterListTf;

            var line = Instantiate(cardLinePrefab, parent).GetComponent<CardLine>();
            line.cardDef = cd;
            line.Apply();
            line.addressableKey = key;

            // Remove any grid/list mapping (no unload; we keep the handle) :contentReference[oaicite:12]{index=12}
            CardLoader.instance.RemoveCardMapping(key, unloadAddressable: false);

            // Track in listItems so refresh will skip spawning it while it’s in a list
            listItems[key] = line.gameObject;

            if (isFaithful) GetComponent<DeckSceneManager>().UpdateFaithfulStats();
        }

        // Replace RemoveFromList with this:
        public void RemoveFromList(string key)
        {
            if (!listItems.TryGetValue(key, out var obj) || obj == null)
            {
                // If the UI already got cleaned up, just attempt to add to grid (if allowed) and unload if not.
                bool addedAnyway = gm.AddCardToGrid(key); // respects filters via CanShowInGrid
                cl.RemoveCardMapping(key, unloadAddressable: !addedAnyway);
                if (addedAnyway && key != null) GetComponent<DeckSceneManager>().UpdateFaithfulStats();
                return;
            }

            var line = obj.GetComponent<CardLine>();
            var cd = line != null ? line.cardDef : null;

            int listType = cd != null && cd.Type.Contains("Faithful") ? 0
                          : cd != null && cd.Type.Contains("Location") ? 1
                          : 2;

            if (NetworkManager.Singleton != null)
            {
                lom.RemoveFromOnlineListServerRpc(key, listType, dsm.currPlayer);
                return;
            }

            // Try to return it to the grid only if current filters allow it
            bool added = gm.AddCardToGrid(key);         // internally checks gm.CanShowInGrid(key)

            // Remove UI + mapping; unload Addressable only if not added back to grid :contentReference[oaicite:13]{index=13}
            cl.RemoveCardMapping(key, unloadAddressable: !added);

            if (listType == 0)
                GetComponent<DeckSceneManager>().UpdateFaithfulStats();
        }

        private bool TryGetCardDefForKey(string key, out CardDef cd)
        {
            cd = null;
            // Prefer the grid GO if it still exists (e.g., clicked from the grid)
            if (gm != null && gm.gridItems.TryGetValue(key, out var gridGo) && gridGo != null)
            {
                var card = gridGo.GetComponent<Card>();
                if (card != null && card._definition != null)
                {
                    cd = card._definition;
                    return true;
                }
            }
            // Fallback to loader handle (safe during refresh)
            if (cl != null && cl.loadedAssets.TryGetValue(key, out var handle) &&
                handle.IsDone && handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded &&
                handle.Result != null)
            {
                cd = handle.Result;
                return true;
            }
            return false;
        }

    }
}
