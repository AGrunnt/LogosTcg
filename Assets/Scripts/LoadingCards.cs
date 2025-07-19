// CardSelectionManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEditor.AddressableAssets.Build.Layout;
using LogoTcg;

namespace LogosTcg
{
    public class LoadingCards : MonoBehaviour
    {
        List<string> activeLabels = new List<string> { "BaseSet" };
        Transform faithfulListTf;
        Transform encounterListTf;
        Transform locationListTf;
        public Transform cardGridTf;

        public GameObject cardPrefab;

        //Dictionary<CardDef, GameObject> gridItems = new Dictionary<CardDef, GameObject>();
        List<CardDef> locCardDefs;
        List<CardDef> EncounterCardDefs;
        List<CardDef> FaithfulCardDefs;

        private Dictionary<string, AsyncOperationHandle<CardDef>> loadedAssets
    = new Dictionary<string, AsyncOperationHandle<CardDef>>();

        private Dictionary<string, GameObject> gridItems
            = new Dictionary<string, GameObject>();

        AsyncOperationHandle<IList<CardDef>> currentHandle;
        Coroutine refreshCoroutine;

        public void toggleLabel(string label)
        {
            if(activeLabels.Contains(label))
            {
                activeLabels.Remove(label);
            } else
            {
                activeLabels.Add(label);
            }

            RefreshGrid();
        }

        void Start()
        {
            RefreshGrid();
        }

        void RefreshGrid()
        {
            // ensure only one refresh at a time //cool trick. note
            if (refreshCoroutine != null)
                StopCoroutine(refreshCoroutine);
            refreshCoroutine = StartCoroutine(DoRefreshGrid());
        }

        IEnumerator DoRefreshGrid()
        {
            var locHandle = Addressables.LoadResourceLocationsAsync(
                    activeLabels.ToArray(),
                    Addressables.MergeMode.Intersection
                );
            yield return locHandle;

            var locations = locHandle.Result;
            Addressables.Release(locHandle);
            var newKeys = new HashSet<string>(locations.Select(loc => loc.PrimaryKey));

            var toRemove = loadedAssets.Keys.Except(newKeys).ToList();
            foreach (var key in toRemove)
            {
                // only proceed if we have a spawned GameObject for this key…
                if (gridItems.TryGetValue(key, out var go))
                {
                    // …and it lives under cardGridTf in the scene
                    if (go.transform.IsChildOf(cardGridTf))
                    {
                        // release the asset handle
                        Addressables.Release(loadedAssets[key]);
                        loadedAssets.Remove(key);

                        // destroy the UI element
                        Destroy(go);
                        gridItems.Remove(key);
                    }
                }
            }


            foreach (var key in newKeys.Except(loadedAssets.Keys))
            {
                var handle = Addressables.LoadAssetAsync<CardDef>(key);
                loadedAssets[key] = handle;
                handle.Completed += op => {
                    // once the asset’s actually loaded:
                    var cd = op.Result;
                    var go = Instantiate(cardPrefab, cardGridTf);
                    gridItems[key] = go;
                    Gobject gobject = go.GetComponent<Gobject>();
                    go.GetComponent<Card>().Apply(cd);
                    gobject.draggable = false;
                    ReorderGrid();
                };
            }
        }

        private void ReorderGrid()
        {
            // collect only fully?loaded cards, sort by cardName, then materialize into a List<>
            var sortedEntries = loadedAssets
                .Where(kv =>
                    gridItems.ContainsKey(kv.Key)
                    && kv.Value.IsDone
                    && kv.Value.Status == AsyncOperationStatus.Succeeded)
                .Select(kv => new { Key = kv.Key, Name = kv.Value.Result.name })
                .OrderBy(e => e.Name)
                .ToList();   // <-- make sure the () are here!

            // then reparent in the new order
            for (int i = 0; i < sortedEntries.Count; i++)
            {
                var key = sortedEntries[i].Key;
                gridItems[key].transform.SetSiblingIndex(i);
            }
        }
    }
}
