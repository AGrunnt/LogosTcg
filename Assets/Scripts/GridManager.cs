using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using LogoTcg;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;


namespace LogosTcg
{
    public class GridManager : MonoBehaviour
    {
        public Dictionary<string, GameObject> gridItems = new();
        public Transform cardGridTf;
        [SerializeField] bool test = false;
        public GameObject gridCardPrefab;
        [SerializeField] private GameObject cardPrefabNeutral;
        [SerializeField] private GameObject cardPrefabSupport;
        [SerializeField] private GameObject cardPrefabFaithful;
        [SerializeField] private GameObject cardPrefabFaithless;
        [SerializeField] private GameObject cardPrefabLocation;
        [SerializeField] private GameObject cardPrefabTrap;
        [SerializeField] private GameObject cardPrefabEventBase;
        [SerializeField] private GameObject cardPrefabEventValue;

        public LayoutGroup lg = null;
        public Mask mask;

        public static GridManager instance;
        void Awake() => instance = this;

        CardLoader cl;
        FilterLabels fl;
        ListManager lm;

        public List<GameObject> gridItemsView = new();

        private void Update()
        {
            gridItemsView = gridItems.Values.ToList();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        async void Start()
        {
            cl = CardLoader.instance;
            fl = FilterLabels.instance;
            lm = ListManager.instance;

            await RefreshGridAsync();
        }

        // Update is called once per frame

        [ContextMenu("Refresh Grid asny")]
        public async Task RefreshGridAsync()
        {
            var filteredLocations = await fl.GetFilteredLocationsAsync();
            var newKeys = new HashSet<string>(
                filteredLocations.Select(loc => loc.PrimaryKey)
                .Concat(lm.listItems.Keys)
            );

            DestroyItems(cl.loadedAssets.Keys.Except(newKeys).ToList());
            await cl.LoadNewKeys(newKeys);
            InstantiateLoadedCardsNotInListOrGrid();

        }

        public bool AddCardToGrid(string key) 
        {
            if (lm.listItems.ContainsKey(key) || gridItems.ContainsKey(key))
            {
                SpawnGridCard(key);
                return true;
            }
            else return false;
        }

        [ContextMenu("reorder Grid")]
        public void ReorderGrid()
        {
            var sorted = cl.loadedAssets
                .Where(kv => gridItems.ContainsKey(kv.Key) && kv.Value.IsDone && kv.Value.Status == AsyncOperationStatus.Succeeded)
                .Select(kv => new { Key = kv.Key, Name = kv.Value.Result.name })
                .OrderBy(x => x.Name)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
                gridItems[sorted[i].Key].transform.SetSiblingIndex(i);

            /*
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(lg.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();
            lg.SetLayoutHorizontal();
            lg.SetLayoutVertical(); 
            */

            //transform.parent.GetComponent<Mask>().enabled = false;
            //transform.parent.GetComponent<Mask>().enabled = true;


            //mask.enabled = true;
            StartCoroutine(delaysec());

        }

        IEnumerator delaysec()
        {
            //yield return new WaitForSeconds(1.0f);
            yield return new WaitForEndOfFrame();
            mask.enabled = false; //need to dirty the layout stuff.

            Debug.Log("testtest");
            mask.enabled = true;
            
        }

        public void DestroyItems(List<string> keyList)
        {
            foreach (var key in keyList)
            {
                cl.RemoveCardMapping(key, true);
            }
        }

        public void SpawnGridCard(string key)
        {
            CardDef cd = cl.loadedAssets[key].Result;

            GameObject go = InstGo(cd);

            var c = go.GetComponent<Card>();
            c.addressableKey = key;
            c.Apply(cd);
            c.SetFacing(true);
            go.GetComponent<Gobject>().draggable = false;

            gridItems[key] = go;
            ReorderGrid();
        }

        public GameObject InstGo(CardDef cd)
        {
            GameObject returnObj = null;

            if (!test)
            {
                //Debug.Log("no test");
                returnObj = Instantiate(gridCardPrefab, cardGridTf);
                return returnObj;
            }

            //Debug.Log("test");


            switch (cd.Type[0])
            {
                case "Location":
                    returnObj = Instantiate(cardPrefabLocation, cardGridTf);
                    break;
                case "Faithful":
                    returnObj = Instantiate(cardPrefabFaithful, cardGridTf);
                    break;
                case "Support":
                    returnObj = Instantiate(cardPrefabSupport, cardGridTf);
                    break;
                case "Trap":
                    returnObj = Instantiate(cardPrefabTrap, cardGridTf);
                    break;
                case "Neutral":
                    returnObj = Instantiate(cardPrefabNeutral, cardGridTf);
                    break;
                case "Faithless":
                    returnObj = Instantiate(cardPrefabFaithless, cardGridTf);
                    break;
                case "Event":
                    if (cd.Value == 0)
                        returnObj = Instantiate(cardPrefabEventBase, cardGridTf);
                    else
                        returnObj = Instantiate(cardPrefabEventValue, cardGridTf);

                    break;

                default:
                    //Debug.Log($"error {cd.Type[0]}");
                    returnObj = Instantiate(cardPrefabFaithful, cardGridTf);
                    break;
            }

            return returnObj;
        }

        public void InstantiateLoadedCardsNotInListOrGrid()
        {
            foreach (var kvp in cl.loadedAssets)
            {
                var key = kvp.Key;
                var handle = kvp.Value;

                if (handle.IsDone &&
                    handle.Status == AsyncOperationStatus.Succeeded &&
                    !lm.listItems.ContainsKey(key) &&
                    !gridItems.ContainsKey(key))
                {
                    SpawnGridCard(key);
                }
            }
        }
    }
}
