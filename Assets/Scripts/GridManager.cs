using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using LogoTcg;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;


namespace LogosTcg
{
    public class GridManager : MonoBehaviour
    {
        public Dictionary<string, GameObject> gridItems = new();
        public Transform cardGridTf;
        public GameObject gridCardPrefab;

        public static GridManager instance;
        void Awake() => instance = this;

        CardLoader cl;
        FilterLabels fl;
        ListManager lm;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        async void Start()
        {
            cl = CardLoader.instance;
            fl = FilterLabels.instance;
            lm = ListManager.instance;

            await RefreshGridAsync();
        }

        // Update is called once per frame

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

        public void ReorderGrid()
        {
            var sorted = cl.loadedAssets
                .Where(kv => gridItems.ContainsKey(kv.Key) && kv.Value.IsDone && kv.Value.Status == AsyncOperationStatus.Succeeded)
                .Select(kv => new { Key = kv.Key, Name = kv.Value.Result.name })
                .OrderBy(x => x.Name)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
                gridItems[sorted[i].Key].transform.SetSiblingIndex(i);
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
            var go = Instantiate(gridCardPrefab, cardGridTf);
            var c = go.GetComponent<Card>();
            c.addressableKey = key;
            c.Apply(cd);
            c.SetFacing(true);
            go.GetComponent<Gobject>().draggable = false;

            gridItems[key] = go;
            ReorderGrid();
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
