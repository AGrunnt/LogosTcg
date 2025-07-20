using LogoTcg;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace LogosTcg
{
    public class LoadingCards : MonoBehaviour
    {
        public static LoadingCards instance;

        [Header("hook these up in Inspector")]
        public Transform cardGridTf;
        public GameObject cardPrefab;

        public List<string> primLabels;
        public List<string> secLabels;
        Dictionary<string, AsyncOperationHandle<CardDef>> loadedAssets = new();
        Dictionary<string, GameObject> gridItems = new();

        // ? cards currently “in a list” will be parked here
        public HashSet<string> listAssigned = new HashSet<string>();

        void Awake() => instance = this;
        void Start() => RefreshGrid();

        public void togglePrimLabel(string label)
        {
            if (primLabels.Contains(label)) primLabels.Remove(label);
            else primLabels.Add(label);
            RefreshGrid();
            UpdateLabelDisplay();
        }

        public void toggleSecLabel(string label)
        {
            if (secLabels.Contains(label)) secLabels.Remove(label);
            else secLabels.Add(label);
            RefreshGrid();
            UpdateLabelDisplay();
        }

        public TextMeshProUGUI primLabs;
        public TextMeshProUGUI secLabs;

        void UpdateLabelDisplay()
        {
            primLabs.text = primLabels.Count > 0
                ? string.Join("\n", primLabels)
                : "<none>";
            secLabs.text = secLabels.Count > 0
                ? string.Join("\n", secLabels)
                : "<none>";
        }

        public void RefreshGrid()
        {
            StopAllCoroutines();
            StartCoroutine(DoRefreshGrid());
        }
        /*
        IEnumerator DoRefreshGrid()
        {
            // 1) fetch all matching locations
            var locHandle = Addressables.LoadResourceLocationsAsync(
                                activeLabels.ToArray(),
                                Addressables.MergeMode.Intersection);
            yield return locHandle;
            var locations = locHandle.Result;
            Addressables.Release(locHandle);

            var newKeys = new HashSet<string>(
                            locations.Select(loc => loc.PrimaryKey));*/
        IEnumerator DoRefreshGrid()
        {
            IList<IResourceLocation> filteredLocations;

            // 1) decide which filters to apply
            bool hasPrim = primLabels != null && primLabels.Count > 0;
            bool hasSec = secLabels != null && secLabels.Count > 0;

            if (!hasPrim && !hasSec)
            {
                // no filters ? show all cards (assumes you've tagged every card asset with a "Card" label)
                var allHandle = Addressables.LoadResourceLocationsAsync(
                                    new[] { "Card" },
                                    Addressables.MergeMode.Union);
                yield return allHandle;
                filteredLocations = allHandle.Result;
                Addressables.Release(allHandle);
            }
            else
            {
                IList<IResourceLocation> primLocs = null, secLocs = null;

                // primary filter: union of all primLabels
                if (hasPrim)
                {
                    var primHandle = Addressables.LoadResourceLocationsAsync(
                                        primLabels.ToArray(),
                                        Addressables.MergeMode.Union);
                    yield return primHandle;
                    primLocs = primHandle.Result;
                    Addressables.Release(primHandle);
                }

                // secondary filter: union of all secLabels
                if (hasSec)
                {
                    var secHandle = Addressables.LoadResourceLocationsAsync(
                                        secLabels.ToArray(),
                                        Addressables.MergeMode.Union);
                    yield return secHandle;
                    secLocs = secHandle.Result;
                    Addressables.Release(secHandle);
                }

                // combine
                if (hasPrim && hasSec)
                {
                    // intersect primLocs ? secLocs ? at least one prim **and** one sec
                    filteredLocations = primLocs
                        .Intersect(secLocs, new ResourceLocationComparer())
                        .ToList();
                }
                else
                {
                    // only one filter active
                    filteredLocations = primLocs ?? secLocs;
                }
            }

            // 2) pull out your keys
            var newKeys = new HashSet<string>(
                            filteredLocations.Select(loc => loc.PrimaryKey));

            // 2) destroy any grid cards no longer in newKeys
            foreach (var key in loadedAssets.Keys.Except(newKeys).ToList())
            {
                if (gridItems.TryGetValue(key, out var go)
                    && go.transform.IsChildOf(cardGridTf))
                {
                    Destroy(go);
                    gridItems.Remove(key);
                    Addressables.Release(loadedAssets[key]);
                    loadedAssets.Remove(key);
                }
            }

            // … inside your DoRefreshGrid() …

            // 3) kick off loads for brand?new keys
            foreach (var key in newKeys.Except(loadedAssets.Keys))
            {
                // capture it so the lambda “remembers” the right string
                var capturedKey = key;

                var handle = Addressables.LoadAssetAsync<CardDef>(capturedKey);
                loadedAssets[capturedKey] = handle;
                handle.Completed += op =>
                {
                    if (op.Status != AsyncOperationStatus.Succeeded)
                        return;

                    var cd = op.Result;

                    // if it’s already in one of your lists, bail
                    if (listAssigned.Contains(capturedKey))
                        return;

                    // spawn into the grid
                    var go = Instantiate(cardPrefab, cardGridTf);
                    var c = go.GetComponent<Card>();
                    c.addressableKey = capturedKey;   // stash the key you captured
                    c.Apply(cd);
                    c.SetFacing(true);
                    go.GetComponent<Gobject>().draggable = false;

                    gridItems[capturedKey] = go;
                    ReorderGrid();
                };
            }


            // 4) if user just removed from a list, re?spawn here
            foreach (var key in loadedAssets.Keys)
            {
                var h = loadedAssets[key];
                if (h.IsDone
                    && h.Status == AsyncOperationStatus.Succeeded
                    && !listAssigned.Contains(key)
                    && !gridItems.ContainsKey(key))
                {
                    SpawnGridCard(key, h.Result);
                }
            }
        }

        void SpawnGridCard(string key, CardDef cd)
        {
            var go = Instantiate(cardPrefab, cardGridTf);
            var c = go.GetComponent<Card>();
            c.addressableKey = key;
            c.Apply(cd);
            c.SetFacing(true);
            go.GetComponent<Gobject>().draggable = false;

            gridItems[key] = go;
            ReorderGrid();
        }

        void ReorderGrid()
        {
            var sorted = loadedAssets
                .Where(kv => gridItems.ContainsKey(kv.Key)
                             && kv.Value.IsDone
                             && kv.Value.Status == AsyncOperationStatus.Succeeded)
                .Select(kv => new { Key = kv.Key, Name = kv.Value.Result.name })
                .OrderBy(x => x.Name)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
                gridItems[sorted[i].Key].transform.SetSiblingIndex(i);
        }

        // called by DeckSceneManager after you Destroy(...) a grid card
        public void RemoveGridCardMapping(string key)
        {
            gridItems.Remove(key);
        }

        // called by DeckSceneManager when a card is removed from a list
        public void AddCardToGrid(string key, CardDef cd)
        {
            if (listAssigned.Contains(key) || gridItems.ContainsKey(key))
                return;
            SpawnGridCard(key, cd);
        }
    }
}