using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Unity.VisualScripting;
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

        public IEnumerator LoadAndPartitionBaseSet()
        {
            // 1) Faithful: BaseSet ? Faithful
            var faithfulHandle = Addressables.LoadAssetsAsync<CardDef>(
                new object[] { BaseSetLabel, FaithfulLabel }, null, Addressables.MergeMode.Intersection);
            yield return faithfulHandle;
            var allFaithfulCards = faithfulHandle.Result.OrderBy(cd => cd.name).ToList();

            // Partition Faithful among players
            int currentIndex = 0;
            int chunkSize = 12;
            int playerCount = GetComponent<StartGameSequence>().testPlayerCount;

            for (int i = 0; i < playerCount; i++)
            {
                int takeCount = Mathf.Min(chunkSize, allFaithfulCards.Count - currentIndex);
                if (takeCount <= 0) break;

                deckFaithful[i].CardCollection = allFaithfulCards.Skip(currentIndex).Take(takeCount).ToList();
                currentIndex += takeCount;
            }

            // 2) Location: BaseSet ? Location
            var locationHandle = Addressables.LoadAssetsAsync<CardDef>(
                new object[] { BaseSetLabel, LocationLabel }, null, Addressables.MergeMode.Intersection);
            yield return locationHandle;

            if (locationHandle.Status == AsyncOperationStatus.Succeeded)
                deckLocation.CardCollection = locationHandle.Result.OrderBy(cd => cd.name).ToList();
            else
                Debug.LogError($"Location load failed: {locationHandle.OperationException}");

            // 3) Encounter = BaseSet ? (ALL Faithful ? ALL Location)
            var allHandle = Addressables.LoadAssetsAsync<CardDef>(BaseSetLabel, null);
            yield return allHandle;
            if (allHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"BaseSet load failed: {allHandle.OperationException}");
                yield break;
            }

            var allCards = allHandle.Result.ToList();

            // Prefer a stable key (name or your CardDef ID) to avoid ref-equality pitfalls.
            HashSet<string> faithfulKeys = allFaithfulCards.Select(c => c.name).ToHashSet();
            HashSet<string> locationKeys = deckLocation.CardCollection.Select(c => c.name).ToHashSet();

            deckEncounter.CardCollection = allCards
                .Where(c => !faithfulKeys.Contains(c.name) && !locationKeys.Contains(c.name))
                .OrderBy(c => c.name)
                .ToList();

            // Optional sanity check
            var leaked = deckEncounter.CardCollection.Where(c => faithfulKeys.Contains(c.name)).Select(c => c.name).ToList();
            if (leaked.Count > 0) Debug.LogError("Faithful leaked into Encounter: " + string.Join(", ", leaked));

            // 4) Clean up
            Addressables.Release(faithfulHandle);
            Addressables.Release(locationHandle);
            Addressables.Release(allHandle);
        }

    }
}

