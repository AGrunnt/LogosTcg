// CardLoader.cs  (replace your LoadingCards.cs contents with this)
// Loads CardDef assets via Addressables. NO Instantiate, NO Apply here.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;               // for Task / await
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LogosTcg
{
    public class CardLoader : MonoBehaviour
    {
        public static CardLoader instance;

        // All successfully requested loads live here
        public readonly Dictionary<string, AsyncOperationHandle<CardDef>> loadedAssets
            = new Dictionary<string, AsyncOperationHandle<CardDef>>();

        // Prevent duplicate loads when multiple callers request the same key
        private readonly Dictionary<string, Task<CardDef>> _inflightLoads
            = new Dictionary<string, Task<CardDef>>();

        private void Awake() => instance = this;

        /// <summary>
        /// Ensure all keys in newKeys are loaded. This ONLY loads; it does not Instantiate or Apply.
        /// Safe to call repeatedly; already-loaded keys are skipped and in-flight loads are reused.
        /// </summary>
        public async Task LoadNewKeys(ICollection<string> newKeys)
        {
            if (newKeys == null || newKeys.Count == 0) return;

            var tasks = new List<Task>();
            foreach (var key in newKeys)
            {
                if (string.IsNullOrEmpty(key)) continue;
                if (loadedAssets.ContainsKey(key)) continue;

                // If someone else is already loading it, reuse their task
                if (_inflightLoads.TryGetValue(key, out var existing))
                {
                    tasks.Add(existing);
                    continue;
                }

                // Kick off a new load
                var t = LoadCardDefAsync(key);
                tasks.Add(t);
            }

            if (tasks.Count > 0)
                await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Load one CardDef by key. Stores the Addressables handle in loadedAssets.
        /// Returns null if the load fails.
        /// </summary>
        public async Task<CardDef> LoadCardDefAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            // If already loaded, return the existing asset
            if (loadedAssets.TryGetValue(key, out var existingHandle))
            {
                if (existingHandle.IsDone && existingHandle.Status == AsyncOperationStatus.Succeeded)
                    return existingHandle.Result;
                // fall through: if it's an unfinished/failed handle, try (re)loading below
            }

            // If another request is already loading this key, await it
            if (_inflightLoads.TryGetValue(key, out var inflight))
                return await inflight;

            // Start a new load and register it in the inflight map
            var task = InternalLoad(key);
            _inflightLoads[key] = task;
            try
            {
                return await task;
            }
            finally
            {
                _inflightLoads.Remove(key);
            }
        }

        private async Task<CardDef> InternalLoad(string key)
        {
            var handle = Addressables.LoadAssetAsync<CardDef>(key);

            // Put the handle into the dictionary immediately so Remove can release it even if we fail later
            loadedAssets[key] = handle;

            try
            {
                var result = await handle.Task;
                if (handle.Status != AsyncOperationStatus.Succeeded || result == null)
                {
                    // Failed load — clean up our entry
                    loadedAssets.Remove(key);
                    return null;
                }
                return result;
            }
            catch
            {
                // Exception during load — remove the handle entry so we don’t hold a bad handle
                loadedAssets.Remove(key);
                throw;
            }
        }

        /// <summary>
        /// Remove object(s) with this key from list/grid and optionally release the Addressables handle.
        /// This mirrors your previous behavior so callers like GridManager.DestroyItems keep working.
        /// </summary>
        public void RemoveCardMapping(string key, bool unloadAddressable)
        {
            if (string.IsNullOrEmpty(key)) return;

            // Remove any instantiated objects that reference this key
            var lm = ListManager.instance;
            if (lm != null && lm.listItems.TryGetValue(key, out var listGo) && listGo != null)
            {
                Destroy(listGo);
                lm.listItems.Remove(key);
            }

            var gm = GridManager.instance;
            if (gm != null && gm.gridItems.TryGetValue(key, out var gridGo) && gridGo != null)
            {
                Destroy(gridGo);
                gm.gridItems.Remove(key);
            }

            // Release Addressables handle if requested
            if (unloadAddressable && loadedAssets.TryGetValue(key, out var handle))
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
                loadedAssets.Remove(key);
            }
        }

        /// <summary>
        /// Convenience helper to clear everything. Usually only for scene teardown.
        /// </summary>
        public void ClearAll(bool unloadAddressables = true)
        {
            // Copy keys to avoid modifying collection during iteration
            var keys = loadedAssets.Keys.ToList();
            foreach (var key in keys)
                RemoveCardMapping(key, unloadAddressables);
        }
    }
}
