// Assets/Scripts/PopulateDecks.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
            // 1) Faithful = BaseSet ? Faithful, then sort by name
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

            for (int i = 0; i < GetComponent<StartGameSequence>().testPlayerCount; i++)
            {
                if (currentIndex >= allFaithfulCards.Count)
                    break; // No more cards to assign

                int cardsLeft = allFaithfulCards.Count - currentIndex;
                int takeCount = Mathf.Min(chunkSize, cardsLeft); // Handle cases where < 12 cards are left

                deckFaithful[i].CardCollection = allFaithfulCards
                    .Skip(currentIndex)
                    .Take(takeCount)
                    .ToList();

                currentIndex += takeCount;
            }





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
            //var faithfulSet = new HashSet<CardDef>(deckFaithful.CardCollection);
            HashSet<CardDef> faithfulSet = new HashSet<CardDef>();

            for (int i = 0; i < GetComponent<StartGameSequence>().testPlayerCount; i++)
            {
                faithfulSet.UnionWith(deckFaithful[i].CardCollection);
            }


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

/*
 encounter.CardCollection =
                    GetComponent<DeckSceneManager>()
                      .encounterListTf
                      .GetComponentsInChildren<CardLine>()
                      .Select(l => l.cardDef)
                      .ToList();

                location.CardCollection =
                    GetComponent<DeckSceneManager>()
                      .locationListTf
                      .GetComponentsInChildren<CardLine>()
                      .Select(l => l.cardDef)
                      .ToList();

                for (int i = 0; i < StaticData.playerNums; i++)
                {
                    Debug.Log($"list item {i}");
                    faithfulList[i].CardCollection =
                        GetComponent<DeckSceneManager>()
                          .faithfulListTf[i]
                          .GetComponentsInChildren<CardLine>()
                          .Select(l => l.cardDef)
                          .ToList();
                } 
 
 */
