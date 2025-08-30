using LogoTcg;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Threading.Tasks;

namespace LogosTcg
{
    public class CardLoader : MonoBehaviour
    {
        public static CardLoader instance;

        [Header("hook these up in Inspector")]
        ListManager lm;



        public Dictionary<string, AsyncOperationHandle<CardDef>> loadedAssets = new();


        GridManager gm;

        void Awake() => instance = this;
        //async void Start() => await RefreshGridAsync();
        void Start()
        {
            gm = GridManager.instance;
            lm = ListManager.instance;

        }

        public async Task LoadNewKeys(HashSet<string> newKeys)
        {
            var keysToLoad = newKeys.Except(loadedAssets.Keys).ToList();
            var loadTasks = new List<Task<CardDef>>();

            foreach (var key in keysToLoad)
                loadTasks.Add(LoadCardDefAsync(key));

            var results = await Task.WhenAll(loadTasks);

            for (int i = 0; i < results.Length; i++)
            {
                var key = keysToLoad[i];
                var cardDef = results[i];

                if (cardDef == null || lm.listItems.ContainsKey(key)) continue;

                GameObject go = gm.InstGo(cardDef);
                //var card = Instantiate(gm.gridCardPrefab, gm.cardGridTf).GetComponent<Card>();
                var card = go.GetComponent<Card>();
                card.addressableKey = key;
                card.Apply(cardDef);
                card.SetFacing(true);
                card.GetComponent<Gobject>().draggable = false;

                gm.gridItems[key] = card.gameObject;
            }

            gm.ReorderGrid();
        }


        public async Task<CardDef> LoadCardDefAsync(string key)
        {
            var handle = Addressables.LoadAssetAsync<CardDef>(key);
            loadedAssets[key] = handle;
            await handle.Task;
            return handle.Status == AsyncOperationStatus.Succeeded ? handle.Result : null;
        }

        public void RemoveCardMapping(string key, bool unloadBool)
        {
            if (gm.gridItems.ContainsKey(key))
            {
                GameObject obj = gm.gridItems[key];
                gm.gridItems.Remove(key);
                Destroy(obj);
            }
            if (lm.listItems.ContainsKey(key))
            {
                GameObject obj = lm.listItems[key];
                lm.listItems.Remove(key);
                Destroy(obj);
            }
            if (unloadBool)
            {
                Addressables.Release(loadedAssets[key]);
                loadedAssets.Remove(key);
            }

            /*
            if (cl.loadedAssets.TryGetValue(key, out var handle))
            {
                Addressables.Release(handle);  // Safe if no one else uses it
                cl.loadedAssets.Remove(key);
            }
            */
        }
    }
}
