using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace LogosTcg
{
    public class PopulateLists : MonoBehaviour
    {
        ListManager lm;
        DeckSceneManager dsm;

        private void Start()
        {
            lm = GetComponent<ListManager>();
            dsm = DeckSceneManager.instance;
        }

        // call this when you want to finish?populate everything:
        public void AutoPopulateAll()
        {
            AutoPopulateAllFaithful();
            PopulateEncounterList();
            PopulateLocationList();
        }

        public void AutoPopulateAllFaithful()
        {
            int old = dsm.currPlayer;
            for (int i = 0; i < dsm.faithfulListTf.Count; i++)
            {
                dsm.currPlayer = i;
                PopulateFaithfulList();
            }
            dsm.currPlayer = old;
        }

        void PopulateFaithfulList()
        {
            var tf = dsm.faithfulListTf[dsm.currPlayer];
            // your per?rarity targets:
            var targets = new Dictionary<string, int>
            {
                ["Rare"] = lm.rareTot,
                ["Uncommon"] = lm.uncomTot,
                ["Common"] = lm.comTot
            };

            foreach (var kv in targets)
            {
                string rarity = kv.Key;
                int desired = kv.Value;

                // count what’s already in *this* player’s list
                int current = tf.GetComponentsInChildren<CardLine>()
                                .Count(l => l.cardDef.Rarity == rarity);

                int needed = desired - current;
                if (needed <= 0) continue;

                // pick from the grid all Faithful cards of that rarity
                var candidates = LoadingCards.instance.cardGridTf
                    .GetComponentsInChildren<Card>()
                    .Where(c => c._definition.Type.Contains("Faithful")
                             && c._definition.Rarity == rarity)
                    .Select(c => c.gameObject)
                    .ToList();

                for (int i = 0; i < needed && candidates.Count > 0; i++)
                {
                    int idx = Random.Range(0, candidates.Count);
                    var go = candidates[idx];
                    candidates.RemoveAt(idx);

                    // this will now deposit into faithfulListTf[currPlayer]
                    lm.AddToList(go);
                }
            }

            dsm.UpdateFaithfulStats();
        }


        void PopulateEncounterList()
        {
            const int totalEventSlots = 23;

            // 1) top up non?Event categories
            var typeTargets = new Dictionary<string, int>
            {
                ["Faithless"] = 15,
                ["Neutral"] = 4,
                ["Support"] = 5,
                ["Trap"] = 7
            };

            foreach (var kv in typeTargets)
            {
                string typeKey = kv.Key;
                int desired = kv.Value;

                int current = dsm.encounterListTf
                    .GetComponentsInChildren<CardLine>()
                    .Count(l => l.cardDef.Type.Contains(typeKey));

                int needed = desired - current;
                if (needed <= 0) continue;

                var candidates = LoadingCards.instance.cardGridTf
                    .GetComponentsInChildren<Card>()
                    .Where(c => c._definition.Type.Contains(typeKey))
                    .Select(c => c.gameObject)
                    .ToList();

                for (int i = 0; i < needed && candidates.Count > 0; i++)
                {
                    int idx = Random.Range(0, candidates.Count);
                    var go = candidates[idx];
                    candidates.RemoveAt(idx);
                    lm.AddToList(go);
                }
            }

            // 2) fill the 23 Event cards by AbilityType distribution
            var abilityTargets = new Dictionary<string, int>
            {
                ["Cost"] = 13,
                ["RefreshSearchDraw"] = 6,   // combined group: Refresh, Search, DrawFaithful
                ["DrawLocation"] = 2,
                ["Instant"] = 2
            };

            foreach (var kv in abilityTargets)
            {
                string category = kv.Key;
                int desired = kv.Value;

                int current = dsm.encounterListTf
                    .GetComponentsInChildren<CardLine>()
                    .Count(l =>
                    {
                        var a = l.cardDef.AbilityType;
                        return category == "Cost" ? a == "Cost"
                             : category == "Instant" ? a == "Instant"
                             : category == "DrawLocation" ? a == "DrawLocation"
                             : /* RefreshSearchDraw */      (a == "Refresh"
                                                             || a == "Search"
                                                             || a == "DrawFaithful");
                    });

                int needed = desired - current;
                if (needed <= 0) continue;

                var candidates = LoadingCards.instance.cardGridTf
                    .GetComponentsInChildren<Card>()
                    .Where(c => c._definition.Type.Contains("Event")
                             && (
                                  category == "Cost" ? c._definition.AbilityType == "Cost"
                                : category == "Instant" ? c._definition.AbilityType == "Instant"
                                : category == "DrawLocation" ? c._definition.AbilityType == "DrawLocation"
                                : /* RefreshSearchDraw */      (c._definition.AbilityType == "Refresh"
                                                                  || c._definition.AbilityType == "Search"
                                                                  || c._definition.AbilityType == "DrawFaithful")
                               ))
                    .Select(c => c.gameObject)
                    .ToList();

                for (int i = 0; i < needed && candidates.Count > 0; i++)
                {
                    int idx = Random.Range(0, candidates.Count);
                    var go = candidates[idx];
                    candidates.RemoveAt(idx);
                    lm.AddToList(go);
                }
            }

            // 3) now handle Events with NO AbilityType
            //    fill any remaining slots up to totalEventSlots
            int currentEvents = dsm.encounterListTf
                .GetComponentsInChildren<CardLine>()
                .Count(l => l.cardDef.Type.Contains("Event"));

            int slotsLeft = totalEventSlots - currentEvents;
            if (slotsLeft > 0)
            {
                var noAbilityCandidates = LoadingCards.instance.cardGridTf
                    .GetComponentsInChildren<Card>()
                    .Where(c =>
                        c._definition.Type.Contains("Event")
                     && string.IsNullOrEmpty(c._definition.AbilityType)
                    )
                    .Select(c => c.gameObject)
                    .ToList();

                for (int i = 0; i < slotsLeft && noAbilityCandidates.Count > 0; i++)
                {
                    int idx = Random.Range(0, noAbilityCandidates.Count);
                    var go = noAbilityCandidates[idx];
                    noAbilityCandidates.RemoveAt(idx);
                    lm.AddToList(go);
                }
            }
        }


        void PopulateLocationList()
        {
            // assume max 10 locations; if yours varies, pull from a variable
            const int maxLocations = 10;
            int needed = maxLocations - dsm.locationListTf.childCount;
            if (needed <= 0) return;

            var candidates = LoadingCards.instance.cardGridTf
                .GetComponentsInChildren<Card>()
                .Where(c => c._definition.Type.Contains("Location"))
                .Select(c => c.gameObject)
                .ToList();

            for (int i = 0; i < needed && candidates.Count > 0; i++)
            {
                int idx = Random.Range(0, candidates.Count);
                var go = candidates[idx];
                candidates.RemoveAt(idx);
                lm.AddToList(go);
            }
        }
    }
}
