using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;
using System.Collections;

namespace LogosTcg
{
    public class AutoPopulateLists : MonoBehaviour
    {
        ListManager lm;
        DeckSceneManager dsm;
        CardLoader cl;
        FaithfulListsManager flm;
        GridManager gm;

        void Start()
        {
            lm = ListManager.instance;
            dsm = DeckSceneManager.instance;
            cl = CardLoader.instance;
            flm = FaithfulListsManager.instance;
            gm = GridManager.instance;
        }

        // call this when you want to finish?populate everything:
        public void AutoPopulateAll()
        {
            AutoPopulateAllFaithful();
            StartCoroutine(PopulateEncounterList());
            StartCoroutine(PopulateLocationList());
        }

        public void AutoPopulateAllFaithful()
        {
            int old = dsm.currPlayer;
            for (int i = 0; i < dsm.faithfulListTf.Count; i++)
            {
                dsm.currPlayer = i;
                StartCoroutine(PopulateFaithfulList());
            }
            dsm.currPlayer = old;
        }

        IEnumerator PopulateFaithfulList()
        {
            var tf = dsm.faithfulListTf[dsm.currPlayer];
            // your per?rarity targets:
            var targets = new Dictionary<string, int>
            {
                ["Rare"] = flm.rareTot,
                ["Uncommon"] = flm.uncomTot,
                ["Common"] = flm.comTot
            };

            foreach (var kv in targets)
            {

                if (dsm.faithfulListTf[GetComponent<DeckSceneManager>().currPlayer].childCount >= flm.maxTot)
                {
                    Debug.LogWarning($"Cannot add more than {flm.maxTot} Faithful cards.");
                    yield break;
                }

                yield return new WaitForSeconds(0.85f);
                string rarity = kv.Key;
                int desired = kv.Value;

                // count what’s already in *this* player’s list
                int current = tf.GetComponentsInChildren<CardLine>()
                                .Count(l => l.cardDef.Rarity == rarity);

                int needed = desired - current;
                if (needed <= 0) continue;

                // pick from the grid all Faithful cards of that rarity
                var candidates = gm.cardGridTf
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


                    yield return new WaitForSeconds(1.85f);
                    //yield return new WaitForSeconds(0.1f);
                    // this will now deposit into faithfulListTf[currPlayer]
                    lm.AddToList(go.GetComponent<Card>().addressableKey);
                }
            }

            dsm.UpdateFaithfulStats();
        }


        IEnumerator PopulateEncounterList()
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

                var candidates = gm.cardGridTf
                    .GetComponentsInChildren<Card>()
                    .Where(c => c._definition.Type.Contains(typeKey))
                    .Select(c => c.gameObject)
                    .ToList();

                for (int i = 0; i < needed && candidates.Count > 0; i++)
                {
                    int idx = Random.Range(0, candidates.Count);
                    var go = candidates[idx];
                    candidates.RemoveAt(idx);
                    lm.AddToList(go.GetComponent<Card>().addressableKey);
                    yield return new WaitForSeconds(0.5f);
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

                var candidates = gm.cardGridTf
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
                    yield return new WaitForSeconds(0.5f);
                    lm.AddToList(go.GetComponent<Card>().addressableKey);
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
                var noAbilityCandidates = gm.cardGridTf
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
                    yield return new WaitForSeconds(0.5f);
                    lm.AddToList(go.GetComponent<Card>().addressableKey);
                }
            }
        }


        IEnumerator PopulateLocationList()
        {
            // assume max 10 locations; if yours varies, pull from a variable
            const int maxLocations = 10;
            int needed = maxLocations - dsm.locationListTf.childCount;
            if (needed <= 0) yield break;

            var candidates = gm.cardGridTf
                .GetComponentsInChildren<Card>()
                .Where(c => c._definition.Type.Contains("Location"))
                .Select(c => c.gameObject)
                .ToList();

            for (int i = 0; i < needed && candidates.Count > 0; i++)
            {
                int idx = Random.Range(0, candidates.Count);
                var go = candidates[idx];
                candidates.RemoveAt(idx);
                lm.AddToList(go.GetComponent<Card>().addressableKey);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
