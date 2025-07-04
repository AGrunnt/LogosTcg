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

        private void Start()
        {
            //StartCoroutine(LoadAndPartitionBaseSet());
        }

        public IEnumerator LoadAndPartitionBaseSet()
        {
            // 1) Faithful = BaseSet ? Faithful
            var faithfulHandle = Addressables.LoadAssetsAsync<CardDef>(
                new object[] { BaseSetLabel, FaithfulLabel },
                callback: null,
                mode: Addressables.MergeMode.Intersection
            );
            yield return faithfulHandle;
            if (faithfulHandle.Status == AsyncOperationStatus.Succeeded)
                deckFaithful.CardCollection = faithfulHandle.Result.ToList();
            else
                Debug.LogError($"Faithful load failed: {faithfulHandle.OperationException}");

            // 2) Location = BaseSet ? Location
            var locationHandle = Addressables.LoadAssetsAsync<CardDef>(
                new object[] { BaseSetLabel, LocationLabel },
                callback: null,
                mode: Addressables.MergeMode.Intersection
            );
            yield return locationHandle;
            if (locationHandle.Status == AsyncOperationStatus.Succeeded)
                deckLocation.CardCollection = locationHandle.Result.ToList();
            else
                Debug.LogError($"Location load failed: {locationHandle.OperationException}");

            // 3) Encounter = everything in BaseSet minus those two sets
            var allHandle = Addressables.LoadAssetsAsync<CardDef>(
                BaseSetLabel,    // all BaseSet assets
                callback: null
            );
            yield return allHandle;

            if (allHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"BaseSet load failed: {allHandle.OperationException}");
                yield break;
            }

            var allCards = allHandle.Result.ToList();
            var faithfulSet = new HashSet<CardDef>(deckFaithful.CardCollection);
            var locationSet = new HashSet<CardDef>(deckLocation.CardCollection);

            deckEncounter.CardCollection = allCards
                .Where(cd => !faithfulSet.Contains(cd) && !locationSet.Contains(cd))
                .ToList();

            // 4) Clean up
            Addressables.Release(faithfulHandle);
            Addressables.Release(locationHandle);
            Addressables.Release(allHandle);
        }
    }
}
