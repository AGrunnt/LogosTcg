using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;                 // Stopwatch
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task; // avoid UnityEditor.Task ambiguity
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using LogoTcg;

namespace LogosTcg
{
    public class GridManager : MonoBehaviour
    {
        // ---------- Scene refs ----------
        [Header("Parents / UI")]
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

        [Header("Layout")]
        public LayoutGroup lg = null;
        public Mask mask;

        [Header("ScrollRect / Content")]
        [SerializeField] private ScrollRect scrollRect;              // CollectionView's ScrollRect
        [SerializeField] private RectTransform contentRect;          // Content RectTransform
        [SerializeField] private GridLayoutGroup contentGrid;        // Content GridLayoutGroup
        [SerializeField] private ContentSizeFitter contentFitter;    // Content ContentSizeFitter

        [Header("Performance")]
        [Tooltip("Max milliseconds to spend spawning per frame.")]
        [SerializeField] private int instantiateBudgetMsPerFrame = 6;
        [Tooltip("Disable LayoutGroup while spawning; enable after finalize.")]
        [SerializeField] private bool suppressLayoutDuringBuild = true;
        [Tooltip("Debounce delay for rapid filter changes (seconds).")]
        [SerializeField] private float filtersDebounceSeconds = 0.05f;

        [Header("Behavior")]
        [Tooltip("If ON, the grid is sorted once at the end of a refresh. If OFF, children keep insertion order.")]
        [SerializeField] private bool reorderAfterRefresh = true;

        [Header("ContentSizeFitter Throttling")]
        [Tooltip("Temporarily re-enable the fitter after this many spawned cards.")]
        [SerializeField] private int fitterFlushEveryN = 24;
        [Tooltip("How many frames to keep the fitter enabled during a flush.")]
        [SerializeField] private int fitterFlushFrames = 1;

        // ---------- State ----------
        public static GridManager instance;

        public Dictionary<string, GameObject> gridItems = new();
        public List<GameObject> gridItemsView = new(); // rebuilt only when content changes

        private CardLoader cl;
        private FilterLabels fl;
        private ListManager lm;

        private CancellationTokenSource _refreshCts;
        private CancellationTokenSource _lifetimeCts;   // cancels on scene change/disable/destroy
        private bool _bootstrapStarted;

        // filter-change debounce + stale work suppression
        private Coroutine _debounceRefreshCo;
        private int _refreshSerial = 0;

        // reorder
        private bool _reorderScheduled;
        public event Action OnReordered;

        // layout suppression
        private bool _layoutSuppressed;

        // allowed keys snapshot for quick checks
        private HashSet<string> _currentAllowedKeys = new();

        // --- fitter throttle internal state ---
        private int _spawnSinceFlush;
        private Coroutine _fitterFlushCo;
        private bool _fitterThrottleActive;

        // scene ownership
        private Scene _owningScene;
        private bool _isQuitting;

        void Awake()
        {
            instance = this;
            _owningScene = gameObject.scene;
            _lifetimeCts = new CancellationTokenSource();

            // Cancel on scene changes that affect us
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            Application.quitting += OnAppQuitting;
        }

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

            if (FilterLabels.instance != null)
                FilterLabels.instance.FiltersChanged += OnFiltersChanged;
        }

        void OnDisable()
        {
            // End throttles and ensure fitter is left enabled
            TryEndFitterThrottle();

            // cancel lifetime for any in-flight tasks
            _lifetimeCts?.Cancel();

            var nm = NetworkManager.Singleton;
            if (nm != null)
            {
                nm.OnServerStarted -= OnServerStarted;
                nm.OnClientConnectedCallback -= OnClientConnected;
                if (nm.SceneManager != null)
                    nm.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            }

            if (FilterLabels.instance != null)
                FilterLabels.instance.FiltersChanged -= OnFiltersChanged;
        }

        void OnDestroy()
        {
            TryEndFitterThrottle();

            _lifetimeCts?.Cancel();
            _lifetimeCts?.Dispose();
            _lifetimeCts = null;

            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            Application.quitting -= OnAppQuitting;
        }

        private void OnAppQuitting() => _isQuitting = true;

        private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            // If *our* scene is going away or we’re no longer in the active scene, stop everything.
            if (oldScene == _owningScene || gameObject == null || gameObject.scene != newScene)
                _lifetimeCts?.Cancel();
        }

        private void OnSceneUnloaded(Scene unloaded)
        {
            if (unloaded == _owningScene)
                _lifetimeCts?.Cancel();
        }

        async void Start()
        {
            cl = CardLoader.instance;
            fl = FilterLabels.instance;
            lm = ListManager.instance;

            await Task.Yield(); await Task.Yield(); // give transport a breath
            StartGridSafely();

            // late-subscribe if needed
            if (fl != null)
            {
                fl.FiltersChanged -= OnFiltersChanged;
                fl.FiltersChanged += OnFiltersChanged;
            }
        }

        void OnServerStarted() { if (NetworkManager.Singleton.IsHost) StartGridSafely(); }
        void OnClientConnected(ulong clientId)
        { if (clientId == NetworkManager.Singleton.LocalClientId) StartGridSafely(); }
        void OnLoadEventCompleted(string _, LoadSceneMode __, List<ulong> ok, List<ulong> ___)
        { if (ok.Contains(NetworkManager.Singleton.LocalClientId)) StartGridSafely(); }

        void StartGridSafely()
        {
            if (_bootstrapStarted) return;
            _bootstrapStarted = true;
            _ = SafeRun(async () => { await Task.Yield(); await Task.Yield(); await RefreshGridAsync(); });
        }

        // ----- Filters changed -----
        private void OnFiltersChanged()
        {
            _refreshCts?.Cancel(); // cancel in-flight
            if (_debounceRefreshCo != null) StopCoroutine(_debounceRefreshCo);
            _debounceRefreshCo = StartCoroutine(DebounceRefresh(filtersDebounceSeconds));
        }

        private IEnumerator DebounceRefresh(float delay)
        {
            yield return new WaitForSeconds(delay);
            _debounceRefreshCo = null;
            RefreshGridFireAndForget();
        }

        // ----- Safe runner -----
        async Task SafeRun(Func<Task> fn)
        {
            try { await fn(); }
            catch (OperationCanceledException) { /* normal when superseded or scene changed */ }
            catch (Exception ex) { if (!_isQuitting) UnityEngine.Debug.LogException(ex); }
        }

        public void RefreshGridFireAndForget() => _ = SafeRun(RefreshGridAsync);

        // ---------- Public API ----------
        [ContextMenu("Refresh Grid (async)")]
        private async void RefreshGrid_ContextMenu() => await SafeRun(RefreshGridAsync);

        public async Task RefreshGridAsync()
        {
            int mySerial = Interlocked.Increment(ref _refreshSerial);

            // link refresh CTS with lifetime CTS so scene changes kill this run
            _refreshCts?.Cancel();
            _refreshCts = new CancellationTokenSource();
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(_refreshCts.Token, _lifetimeCts.Token);
            var ct = linked.Token;

            try
            {
                // Bail immediately if our scene/parent is gone
                if (!IsAliveForSpawns()) return;

                // 1) read filters
                var filteredLocations = await fl.GetFilteredLocationsAsync();
                if (ct.IsCancellationRequested || mySerial != _refreshSerial || !IsAliveForSpawns()) return;

                // 2) compute allowed keys off-thread
                var newKeys = await Task.Run(() =>
                {
                    var set = new HashSet<string>(filteredLocations.Select(loc => loc.PrimaryKey));
                    foreach (var k in lm.listItems.Keys) set.Add(k);
                    return set;
                }, ct);
                if (ct.IsCancellationRequested || mySerial != _refreshSerial || !IsAliveForSpawns()) return;

                _currentAllowedKeys = new HashSet<string>(newKeys);

                // 3) remove stale
                var stale = cl.loadedAssets.Keys.Where(k => !newKeys.Contains(k)).ToList();
                DestroyItems(stale);
                if (ct.IsCancellationRequested || mySerial != _refreshSerial || !IsAliveForSpawns()) return;

                // 4) load (no instantiate here)
                await cl.LoadNewKeys(newKeys);
                if (ct.IsCancellationRequested || mySerial != _refreshSerial || !IsAliveForSpawns()) return;

                // 5) spawn budgeted + throttle the ContentSizeFitter
                BeginBulkLayout();
                BeginFitterThrottle();
                try
                {
                    await InstantiateMissingWithBudget(instantiateBudgetMsPerFrame, ct, mySerial);
                }
                finally
                {
                    // ensure fitter ends enabled even on cancel
                    if (_fitterThrottleActive) EndFitterThrottle(leaveEnabled: true);
                }

                if (ct.IsCancellationRequested || mySerial != _refreshSerial || !IsAliveForSpawns()) return;

                // 6) finalize
                if (reorderAfterRefresh)
                {
                    ReorderGrid(); // schedules EndBulkLayout + final fitter enable
                }
                else
                {
                    FinalizeWithoutReorder();
                }
            }
            catch (OperationCanceledException) { /* expected on supersede/scene change */ }
        }

        /// <summary>True if key allowed by current filter snapshot.</summary>
        public bool CanShowInGrid(string key) =>
            _currentAllowedKeys == null || _currentAllowedKeys.Count == 0 || _currentAllowedKeys.Contains(key);

        public bool AddCardToGrid(string key)
        {
            if (!CanShowInGrid(key)) return false;
            if (gridItems.ContainsKey(key)) return true;
            if (!IsAliveForSpawns()) return false;

            SpawnGridCard_NoReorder(key);

            // NOTE: per-item adds still reorder; the end-of-refresh toggle only affects auto sort
            ReorderGrid();
            return gridItems.ContainsKey(key);
        }

        // ---------- Reorder ----------
        [ContextMenu("Reorder Grid (deferred)")]
        public void ReorderGrid() => DeferReorderOnce();

        public void ReorderGrid(bool defer) { if (defer) DeferReorderOnce(); else ReorderGridImmediate(); }

        public void ReorderGridImmediate()
        {
            if (!IsAliveForSpawns())
            { // still finalize layout/fitter if we can
                TryEndFitterThrottle();
                EndBulkLayout();
                return;
            }

            var sorted = cl.loadedAssets
                .Where(kv => gridItems.ContainsKey(kv.Key) &&
                             kv.Value.IsDone &&
                             kv.Value.Status == AsyncOperationStatus.Succeeded)
                .Select(kv => new { kv.Key, Name = kv.Value.Result.name })
                .OrderBy(x => x.Name).ToList();

            for (int i = 0; i < sorted.Count; i++)
                gridItems[sorted[i].Key].transform.SetSiblingIndex(i);

            EndBulkLayout();

            // finalize fitter state (must end enabled)
            EndFitterThrottle(leaveEnabled: true);

            if (mask != null) { mask.enabled = false; mask.enabled = true; }

            gridItemsView = gridItems.Values.ToList();
            OnReordered?.Invoke();
        }

        private void FinalizeWithoutReorder()
        {
            EndBulkLayout();
            EndFitterThrottle(leaveEnabled: true);

            if (mask != null) { mask.enabled = false; mask.enabled = true; }

            gridItemsView = gridItems.Values.ToList();
        }

        private void DeferReorderOnce()
        {
            if (_reorderScheduled) return;
            _reorderScheduled = true;
            StartCoroutine(ReorderNextFrame());
        }

        private IEnumerator ReorderNextFrame()
        {
            yield return new WaitForEndOfFrame();
            _reorderScheduled = false;
            ReorderGridImmediate();
        }

        // ---------- Spawn / Destroy ----------
        private async Task InstantiateMissingWithBudget(int maxMsPerFrame, CancellationToken ct, int mySerial)
        {
            var candidates = cl.loadedAssets.Keys.ToList();
            var sw = Stopwatch.StartNew();

            foreach (var key in candidates)
            {
                if (ct.IsCancellationRequested || mySerial != _refreshSerial || !IsAliveForSpawns()) return;

                if (!CanShowInGrid(key)) continue;           // live filter recheck
                if (lm.listItems.ContainsKey(key)) continue;  // moved to list mid-refresh
                if (gridItems.ContainsKey(key)) continue;     // already spawned elsewhere

                if (!cl.loadedAssets.TryGetValue(key, out var handle)) continue;
                if (!handle.IsDone || handle.Status != AsyncOperationStatus.Succeeded) continue;

                SpawnGridCard_NoReorder(key);
                OnSpawnedOneForFitterThrottle();

                if (sw.ElapsedMilliseconds >= maxMsPerFrame)
                {
                    sw.Restart();
                    await Task.Yield(); // let networking/UI breathe
                }
            }
        }

        public void DestroyItems(List<string> keyList)
        {
            foreach (var key in keyList)
            {
                cl.RemoveCardMapping(key, true);
                gridItems.Remove(key);
            }
            gridItemsView = gridItems.Values.ToList();
        }

        private void SpawnGridCard_NoReorder(string key)
        {
            if (!IsAliveForSpawns()) return;
            if (!cl.loadedAssets.TryGetValue(key, out var handle)) return;
            if (!handle.IsDone || handle.Status != AsyncOperationStatus.Succeeded) return;

            var cd = handle.Result;

            // If our parent is gone (scene changed), skip
            if (cardGridTf == null) return;

            GameObject go = InstGo(cd);
            if (go == null) return;

            var c = go.GetComponent<Card>();
            c.addressableKey = key;
            c.Apply(cd); // if heavy, you can move to a later budgeted pass
            c.SetFacing(true);
            var g = go.GetComponent<Gobject>(); if (g) g.draggable = false;

            gridItems[key] = go;

            // Ensure spawned object stays in the same scene as this manager
            try { SceneManager.MoveGameObjectToScene(go, _owningScene); } catch { /* ignore if invalid */ }
        }

        public GameObject InstGo(CardDef cd)
        {
            if (cardGridTf == null) return null;

            if (!test)
                return Instantiate(gridCardPrefab, cardGridTf);

            GameObject obj;
            switch (cd.Type[0])
            {
                case "Location": obj = Instantiate(cardPrefabLocation, cardGridTf); break;
                case "Faithful": obj = Instantiate(cardPrefabFaithful, cardGridTf); break;
                case "Support": obj = Instantiate(cardPrefabSupport, cardGridTf); break;
                case "Trap": obj = Instantiate(cardPrefabTrap, cardGridTf); break;
                case "Neutral": obj = Instantiate(cardPrefabNeutral, cardGridTf); break;
                case "Faithless": obj = Instantiate(cardPrefabFaithless, cardGridTf); break;
                case "Event":
                    obj = (cd.Value == 0)
                        ? Instantiate(cardPrefabEventBase, cardGridTf)
                        : Instantiate(cardPrefabEventValue, cardGridTf);
                    break;
                default:
                    obj = Instantiate(cardPrefabFaithful, cardGridTf);
                    break;
            }
            return obj;
        }

        // ---------- Compatibility ----------
        public void InstantiateLoadedCardsNotInListOrGrid()
        {
            if (!IsAliveForSpawns()) return;

            BeginBulkLayout();
            BeginFitterThrottle();

            foreach (var key in cl.loadedAssets.Keys.ToList())
            {
                if (!IsAliveForSpawns()) break;
                if (!CanShowInGrid(key)) continue;
                if (lm.listItems.ContainsKey(key)) continue;
                if (gridItems.ContainsKey(key)) continue;

                SpawnGridCard_NoReorder(key);
                OnSpawnedOneForFitterThrottle();
            }

            if (reorderAfterRefresh) ReorderGrid();
            else FinalizeWithoutReorder();
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

        // ---------- ContentSizeFitter throttling ----------
        private IEnumerator FitterEnableWindow(int frames)
        {
            if (!contentFitter) yield break;

            contentFitter.enabled = true;
            for (int i = 0; i < Mathf.Max(1, frames); i++)
                yield return null;
            // disable again until we finish or next flush
            if (_fitterThrottleActive) contentFitter.enabled = false;
        }

        private void BeginFitterThrottle()
        {
            if (!contentFitter) return;
            _spawnSinceFlush = 0;
            _fitterThrottleActive = true;
            contentFitter.enabled = false; // start disabled
        }

        private void OnSpawnedOneForFitterThrottle()
        {
            if (!_fitterThrottleActive || !contentFitter) return;

            _spawnSinceFlush++;
            if (fitterFlushEveryN > 0 && _spawnSinceFlush >= fitterFlushEveryN)
            {
                _spawnSinceFlush = 0;
                if (_fitterFlushCo != null) StopCoroutine(_fitterFlushCo);
                _fitterFlushCo = StartCoroutine(FitterEnableWindow(fitterFlushFrames));
            }
        }

        private void EndFitterThrottle(bool leaveEnabled)
        {
            if (!contentFitter) return;

            if (_fitterFlushCo != null)
            {
                StopCoroutine(_fitterFlushCo);
                _fitterFlushCo = null;
            }

            contentFitter.enabled = leaveEnabled;

            // make sure size is up-to-date immediately
            if (contentRect)
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

            _fitterThrottleActive = false;
        }

        private void TryEndFitterThrottle()
        {
            if (_fitterThrottleActive)
                EndFitterThrottle(leaveEnabled: true);
        }

        // ---------- Life/scene guards ----------
        private bool IsAliveForSpawns()
        {
            // UnityEngine.Object equality handles destroyed objects -> null
            return this != null
                   && gameObject != null
                   && cardGridTf != null
                   && gameObject.scene == _owningScene
                   && gameObject.activeInHierarchy
                   && !_isQuitting;
        }
    }
}
