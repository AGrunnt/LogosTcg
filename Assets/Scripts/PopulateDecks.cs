using System.Collections;
using System.Collections.Generic;
using System.Linq;
// using Unity.VisualScripting; // keep this out to avoid LINQ extension clashes
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LogosTcg
{
    public class PopulateDecks : MonoBehaviour
    {
        [Header("Assign your DeckDefinition ScriptableObjects here")]
        [SerializeField] public List<DeckDefinition> deckFaithful;
        [SerializeField] public DeckDefinition deckLocation;
        [SerializeField] public DeckDefinition deckEncounter;

        private const string BaseSetLabel = "BaseSet";
        private const string FaithfulLabel = "Faithful";
        private const string LocationLabel = "Location";

        // Encounter categories (any one of these, but always within BaseSet)
        private const string FaithlessLabel = "Faithless";
        private const string SupportLabel = "Support";
        private const string TrapLabel = "Trap";
        private const string NeutralLabel = "Neutral";

        public IEnumerator LoadAndPartitionBaseSet()
        {
            // -------- 1) Faithful = BaseSet ? Faithful --------
            var faithfulHandle = Addressables.LoadAssetsAsync<CardDef>(
                new object[] { BaseSetLabel, FaithfulLabel },
                callback: null,
                mode: Addressables.MergeMode.Intersection
            );
            yield return faithfulHandle;

            var allFaithfulCards = faithfulHandle.Result
                .OrderBy(cd => cd.name)
                .ToList();

            int currentIndex = 0;
            int chunkSize = 12;
            int playerCount = GetComponent<StartGameSequence>().testPlayerCount;
            int maxDecks = Mathf.Min(playerCount, deckFaithful.Count);

            for (int i = 0; i < maxDecks; i++)
            {
                if (currentIndex >= allFaithfulCards.Count)
                    break;

                int cardsLeft = allFaithfulCards.Count - currentIndex;
                int takeCount = Mathf.Min(chunkSize, cardsLeft);

                deckFaithful[i].CardCollection = allFaithfulCards
                    .Skip(currentIndex)
                    .Take(takeCount)
                    .ToList();

                currentIndex += takeCount;
            }

            // -------- 2) Location = BaseSet ? Location --------
            var locationHandle = Addressables.LoadAssetsAsync<CardDef>(
                new object[] { BaseSetLabel, LocationLabel },
                callback: null,
                mode: Addressables.MergeMode.Intersection
            );
            yield return locationHandle;

            if (locationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                deckLocation.CardCollection = locationHandle
                    .Result
                    .OrderBy(cd => cd.name)
                    .ToList();
            }
            else
            {
                Debug.LogError($"Location load failed: {locationHandle.OperationException}");
            }

            // -------- 3) Encounter = (BaseSet ? Faithless) ? (BaseSet ? Support) ? (BaseSet ? Trap) ? (BaseSet ? Neutral) --------
            var encFaithlessHandle = Addressables.LoadAssetsAsync<CardDef>(
                new object[] { BaseSetLabel, FaithlessLabel }, null, Addressables.MergeMode.Intersection);
            yield return encFaithlessHandle;

            var encSupportHandle = Addressables.LoadAssetsAsync<CardDef>(
                new object[] { BaseSetLabel, SupportLabel }, null, Addressables.MergeMode.Intersection);
            yield return encSupportHandle;

            var encTrapHandle = Addressables.LoadAssetsAsync<CardDef>(
                new object[] { BaseSetLabel, TrapLabel }, null, Addressables.MergeMode.Intersection);
            yield return encTrapHandle;

            var encNeutralHandle = Addressables.LoadAssetsAsync<CardDef>(
                new object[] { BaseSetLabel, NeutralLabel }, null, Addressables.MergeMode.Intersection);
            yield return encNeutralHandle;

            // Union the results, de-duping by a stable key (name here; swap to your CardDef ID if available)
            var byName = new Dictionary<string, CardDef>();
            void AddRangeIfSucceeded(AsyncOperationHandle<IList<CardDef>> h, string label)
            {
                if (h.Status == AsyncOperationStatus.Succeeded && h.Result != null)
                {
                    foreach (var cd in h.Result) byName[cd.name] = cd;
                }
                else
                {
                    Debug.LogError($"Encounter ({label}) load failed: {h.OperationException}");
                }
            }

            AddRangeIfSucceeded(encFaithlessHandle, FaithlessLabel);
            AddRangeIfSucceeded(encSupportHandle, SupportLabel);
            AddRangeIfSucceeded(encTrapHandle, TrapLabel);
            AddRangeIfSucceeded(encNeutralHandle, NeutralLabel);

            deckEncounter.CardCollection = byName.Values
                .OrderBy(cd => cd.name)
                .ToList();

            // -------- 4) Clean up --------
            Addressables.Release(faithfulHandle);
            Addressables.Release(locationHandle);
            Addressables.Release(encFaithlessHandle);
            Addressables.Release(encSupportHandle);
            Addressables.Release(encTrapHandle);
            Addressables.Release(encNeutralHandle);
        }
    }
}
