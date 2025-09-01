
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using LogoTcg;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Diagnostics; // for Stopwatch
using System;
using LogosTcg;
using NUnit.Framework.Internal;
//using UnityEditor.VersionControl;

namespace LogosTcg
{
    public class GridManager : MonoBehaviour
    {
        // ---------- Inspector ----------
        public Transform cardGridTf;

        [Header("Prefabs")]
        [SerializeField] private bool test = false;
        public GameObject gridCardPrefab;
        [SerializeField] private GameObject cardPrefabNeutral;
        [SerializeField] private GameObject cardPrefabSupport;
        [SerializeField] private GameObject cardPrefabFaithful;
        [SerializeField] private GameObject cardPrefabFaithless;
        [SerializeField] private GameObject cardPrefabLocation;
        [SerializeField] private GameObject cardPrefabTrap;
        [SerializeField] private GameObject cardPrefabEventBase;
        [SerializeField] private GameObject cardPrefabEventValue;

        [Header("Layout Nudge")]
        public LayoutGroup lg = null;
        public Mask mask;

        [Header("Performance Tuning")]
        [Tooltip("Hard cap on spawn work per frame to keep networking responsive (ms).")]
        [SerializeField] private int instantiateBudgetMsPerFrame = 350;
        [Tooltip("Temporarily disable LayoutGroup while building to avoid thrash.")]
        [SerializeField] private bool suppressLayoutDuringBuild = false;

        // ---------- State ----------
        public static GridManager instance;
        public Dictionary<string, GameObject> gridItems = new();
        public List<GameObject> gridItemsView = new(); // rebuilt only when content changes 

        private CardLoader cl;
        private FilterLabels fl;
        private ListManager lm;

        private CancellationTokenSource _refreshCts;
        private bool _bootstrapStarted;

        // Reorder scheduling & event
        private bool _reorderScheduled;
        public event Action OnReordered;

        // Layout suppression tracking
        private bool _layoutSuppressed;

        // ---------- Lifecycle ----------
        void Awake() => instance = this;

        void OnEnable()
        {
            var nm = NetworkManager.Singleton;
            if (nm != null)
            {
                nm.OnServerStarted += OnServerStarted;
                nm.OnClientConnectedCallback += OnClientConnected;
                if (nm.SceneManager != null)
                    nm.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            }
        }

        void OnDisable()
        {
            var nm = NetworkManager.Singleton;
            if (nm != null)
            {
                nm.OnServerStarted -= OnServerStarted;
                nm.OnClientConnectedCallback -= OnClientConnected;
                if (nm.SceneManager != null)
                    nm.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            }
        }

        async void Start()
        {
            cl = CardLoader.instance;
            fl = FilterLabels.instance;
            lm = ListManager.instance;

            // Fallback if the connection was already created in the previous scene.
            await Task.Yield();
            await Task.Yield();
            StartGridSafely();
        }

        void OnServerStarted()
        {
            if (NetworkManager.Singleton.IsHost)
                StartGridSafely();
        }

        void OnClientConnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
                StartGridSafely();
        }

        void OnLoadEventCompleted(string sceneName, LoadSceneMode mode,
                                  List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (clientsCompleted.Contains(NetworkManager.Singleton.LocalClientId))
                StartGridSafely();
        }

        void StartGridSafely()
        {
            if (_bootstrapStarted) return;
            _bootstrapStarted = true;

            _ = SafeRun(async () =>
            {
                // Give UTP/Relay a couple frames to breathe before doing any heavy work.
                await Task.Yield();
                await Task.Yield();
                await RefreshGridAsync();
            });
        }

        async Task SafeRun(Func<Task> fn)
        {
            try { await fn(); }
            catch (Exception ex) { }
            //Debug.LogException(ex); }
        }

        // ---------- Public API ----------
        [ContextMenu("Refresh Grid (async)")]
        private async void RefreshGrid_ContextMenu()
        {
            await SafeRun(RefreshGridAsync);
        }

        public async Task RefreshGridAsync()
        {
            // Cancel any previous refresh to avoid overlapping work.
            _refreshCts?.Cancel();
            _refreshCts = new CancellationTokenSource();
            var ct = _refreshCts.Token;

            // 1) Get filtered locations (async, non-blocking)
            var filteredLocations = await fl.GetFilteredLocationsAsync();
            ct.ThrowIfCancellationRequested();

            // 2) Build newKeys OFF the main thread (pure C#)
            var newKeys = await Task.Run(() =>
            {
                var set = new HashSet<string>(filteredLocations.Select(loc => loc.PrimaryKey));
                foreach (var k in lm.listItems.Keys) set.Add(k);
                return set;
            }, ct);
            ct.ThrowIfCancellationRequested();

            // 3) Remove stale items (main thread)
            var stale = cl.loadedAssets.Keys.Where(k => !newKeys.Contains(k)).ToList();
            DestroyItems(stale);
            ct.ThrowIfCancellationRequested();

            // 4) Load new keys asynchronously (ensure CardLoader uses awaits, not WaitForCompletion)
            await cl.LoadNewKeys(newKeys);
            ct.ThrowIfCancellationRequested();

            // 5) Instantiate missing items with a strict per-frame time budget
            BeginBulkLayout();
            await InstantiateMissingWithBudget(instantiateBudgetMsPerFrame, ct);
            ct.ThrowIfCancellationRequested();

            // 6) Reorder once at end-of-frame (also ends bulk layout + nudges mask)
            ReorderGrid(); // schedules
        }

        public bool AddCardToGrid(string key)
        {
            if (lm.listItems.ContainsKey(key) || gridItems.ContainsKey(key))
            {
                SpawnGridCard_NoReorder(key);
                ReorderGrid(); // schedule a single reorder at frame end
                return true;
            }
            return false;
        }

        // ---------- Reorder API (backward compatible) ----------

        // Old callers can keep calling this. It now SCHEDULES a single reorder for end-of-frame.
        [ContextMenu("Reorder Grid (deferred)")]
        public void ReorderGrid() => DeferReorderOnce();

        // Optional convenience overload
        public void ReorderGrid(bool defer)
        {
            if (defer) DeferReorderOnce();
            else ReorderGridImmediate();
        }

        // Use sparingly if a caller truly needs immediate results in the same frame.
        public void ReorderGridImmediate()
        {
            var sorted = cl.loadedAssets
                .Where(kv => gridItems.ContainsKey(kv.Key) &&
                             kv.Value.IsDone &&
                             kv.Value.Status == AsyncOperationStatus.Succeeded)
                .Select(kv => new { Key = kv.Key, Name = kv.Value.Result.name })
                .OrderBy(x => x.Name)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
                gridItems[sorted[i].Key].transform.SetSiblingIndex(i);

            // Re-enable layout if we suppressed it during build
            EndBulkLayout();

            // Nudge layout once (no per-card thrash)
            if (mask != null)
            {
                mask.enabled = false;
                mask.enabled = true;
            }

            // Keep the view list in sync only when content actually changes
            gridItemsView = gridItems.Values.ToList();

            OnReordered?.Invoke();
        }

        private void DeferReorderOnce()
        {
            if (_reorderScheduled) return;
            _reorderScheduled = true;
            StartCoroutine(ReorderNextFrame());
        }

        private IEnumerator ReorderNextFrame()
        {
            // Wait until all layout/instantiation for this frame is done
            yield return new WaitForEndOfFrame();
            _reorderScheduled = false;
            ReorderGridImmediate();
        }

        // ---------- Instantiation / Destruction ----------

        private async Task InstantiateMissingWithBudget(int maxMsPerFrame, CancellationToken ct)
        {
            var pending = cl.loadedAssets
                .Where(kv =>
                    kv.Value.IsDone &&
                    kv.Value.Status == AsyncOperationStatus.Succeeded &&
                    !lm.listItems.ContainsKey(kv.Key) &&
                    !gridItems.ContainsKey(kv.Key))
                .Select(kv => kv.Key)
                .ToList();

            var sw = Stopwatch.StartNew();

            foreach (var key in pending)
            {
                ct.ThrowIfCancellationRequested();

                // Spawn one card (main-thread only)
                SpawnGridCard_NoReorder(key);

                // If we've used our budget this frame, yield so networking can breathe
                if (sw.ElapsedMilliseconds >= maxMsPerFrame)
                {
                    sw.Restart();
                    await Task.Yield(); // lets EarlyUpdate/transport run
                }
            }
        }

        public void DestroyItems(List<string> keyList)
        {
            foreach (var key in keyList)
            {
                cl.RemoveCardMapping(key, true);
                if (gridItems.TryGetValue(key, out var go) && go != null)
                {
                    Destroy(go);
                    gridItems.Remove(key);
                }
            }

            gridItemsView = gridItems.Values.ToList();
        }

        private void SpawnGridCard_NoReorder(string key)
        {
            var handle = cl.loadedAssets[key];
            if (!handle.IsDone || handle.Status != AsyncOperationStatus.Succeeded) return;

            CardDef cd = handle.Result;
            GameObject go = InstGo(cd);

            var c = go.GetComponent<Card>();
            c.addressableKey = key;
            c.Apply(cd);
            c.SetFacing(true);
            go.GetComponent<Gobject>().draggable = false;

            gridItems[key] = go;
            // do NOT reorder here (batched at end)
        }

        public GameObject InstGo(CardDef cd)
        {
            if (!test)
                return Instantiate(gridCardPrefab, cardGridTf);

            GameObject returnObj;
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
                    returnObj = (cd.Value == 0)
                        ? Instantiate(cardPrefabEventBase, cardGridTf)
                        : Instantiate(cardPrefabEventValue, cardGridTf);
                    break;
                default:
                    returnObj = Instantiate(cardPrefabFaithful, cardGridTf);
                    break;
            }
            return returnObj;
        }

        // Kept for compatibility if called elsewhere (now defers a single reorder)
        public void InstantiateLoadedCardsNotInListOrGrid()
        {
            BeginBulkLayout();

            foreach (var kvp in cl.loadedAssets)
            {
                var key = kvp.Key;
                var handle = kvp.Value;

                if (handle.IsDone &&
                    handle.Status == AsyncOperationStatus.Succeeded &&
                    !lm.listItems.ContainsKey(key) &&
                    !gridItems.ContainsKey(key))
                {
                    SpawnGridCard_NoReorder(key);
                }
            }
            ReorderGrid(); // schedule once (will EndBulkLayout inside)
        }

        // ---------- Layout helpers ----------
        private void BeginBulkLayout()
        {
            if (!_layoutSuppressed && suppressLayoutDuringBuild && lg != null && lg.enabled)
            {
                lg.enabled = false;
                _layoutSuppressed = true;
            }
        }

        private void EndBulkLayout()
        {
            if (_layoutSuppressed && lg != null)
            {
                lg.enabled = true;
                _layoutSuppressed = false;
            }
        }
    }
}

/*
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
*/