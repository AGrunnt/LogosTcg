// Assets/Scripts/PopulateDecks.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LogosTcg
{
    public class PopulateDecks : MonoBehaviour
    {
        [Header("Assign your DeckDefinition ScriptableObjects here")]
        [SerializeField] public DeckDefinition deckFaithful;
        [SerializeField] public DeckDefinition deckLocation;
        [SerializeField] public DeckDefinition deckEncounter;

        private const string BaseSetLabel = "BaseSet";
        private const string FaithfulLabel = "Faithful";
        private const string LocationLabel = "Location";

        public IEnumerator LoadAndPartitionBaseSet()
        {
            // 1) Faithful = BaseSet ? Faithful, then sort by name
            var faithfulHandle = Addressables.LoadAssetsAsync<CardDef>(
                new object[] { BaseSetLabel, FaithfulLabel },
                callback: null,
                mode: Addressables.MergeMode.Intersection
            );
            yield return faithfulHandle;
            if (faithfulHandle.Status == AsyncOperationStatus.Succeeded)
                deckFaithful.CardCollection = faithfulHandle
                    .Result
                    .OrderBy(cd => cd.name)
                    .ToList();
            else
                Debug.LogError($"Faithful load failed: {faithfulHandle.OperationException}");

            // 2) Location = BaseSet ? Location, then sort by name
            var locationHandle = Addressables.LoadAssetsAsync<CardDef>(
                new object[] { BaseSetLabel, LocationLabel },
                callback: null,
                mode: Addressables.MergeMode.Intersection
            );
            yield return locationHandle;
            if (locationHandle.Status == AsyncOperationStatus.Succeeded)
                deckLocation.CardCollection = locationHandle
                    .Result
                    .OrderBy(cd => cd.name)
                    .ToList();
            else
                Debug.LogError($"Location load failed: {locationHandle.OperationException}");

            // 3) Encounter = BaseSet minus those two sets, then sort by name
            var allHandle = Addressables.LoadAssetsAsync<CardDef>(
                BaseSetLabel,
                callback: null
            );
            yield return allHandle;

            if (allHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"BaseSet load failed: {allHandle.OperationException}");
                yield break;
            }

            // Build hash?sets of faithful & location for quick exclusion
            var allCards = allHandle.Result.ToList();
            var faithfulSet = new HashSet<CardDef>(deckFaithful.CardCollection);
            var locationSet = new HashSet<CardDef>(deckLocation.CardCollection);

            deckEncounter.CardCollection = allCards
                .Where(cd => !faithfulSet.Contains(cd) && !locationSet.Contains(cd))
                .OrderBy(cd => cd.name)
                .ToList();

            // 4) Clean up
            Addressables.Release(faithfulHandle);
            Addressables.Release(locationHandle);
            Addressables.Release(allHandle);
        }
    }
}
