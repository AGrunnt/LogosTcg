using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Netcode;
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
        PopulateOnlineList pol;

        void Start()
        {
            lm = ListManager.instance;
            dsm = DeckSceneManager.instance;
            cl = CardLoader.instance;
            flm = FaithfulListsManager.instance;
            gm = GridManager.instance;
            pol = GetComponent<PopulateOnlineList>();

            // fire off the population tasks 
            //AutoPopulateAll();
        }

        public void AutoPopulateAll()
        {
            if (NetworkManager.Singleton == null)
            {
                //_ = AutoPopulateAllFaithful();
                StartCoroutine(AutoPopulateAllFaithful());
                PopulateEncounterList();
                PopulateLocationList();
            } else if (NetworkManager.Singleton.IsHost)
            {
                pol.AutoPopulateOnlineAll();
            }
        }

        //private async Task AutoPopulateAllFaithful()
        IEnumerator AutoPopulateAllFaithful()
        {
            int old = dsm.currPlayer;
            for (int i = 0; i < dsm.faithfulListTf.Count; i++)
            {
                dsm.currPlayer = i;
                //await PopulateFaithfulList();
                yield return PopulateFaithfulList();

            }
            dsm.currPlayer = old;
            dsm.UpdateFaithfulStats();

        }

        private async Task PopulateFaithfulList()
        {
            var tf = dsm.faithfulListTf[dsm.currPlayer];

            var targets = new Dictionary<string, int>
            {
                ["Rare"] = flm.rareTot,
                ["Uncommon"] = flm.uncomTot,
                ["Common"] = flm.comTot
            };

            
            foreach (var kv in targets)
            {
                if (tf.childCount >= flm.maxTot)
                {
                    Debug.LogWarning($"Cannot add more than {flm.maxTot} Faithful cards. ");
                    continue;
                }

                string rarity = kv.Key;
                int desired = kv.Value;

                int current = tf.GetComponentsInChildren<CardLine>()
                                .Count(l => l.cardDef.Rarity == rarity);

                int needed = desired - current;
                if (needed <= 0) continue;

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

                    
                    lm.AddToList(go.GetComponent<Card>().addressableKey);
                }
            }

            
        }

        private void PopulateEncounterList()
        {
            const int totalEventSlots = 23;

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
                }
            }

            var abilityTargets = new Dictionary<string, int>
            {
                ["Cost"] = 13,
                ["RefreshSearchDraw"] = 6,
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
                             : (a == "Refresh" || a == "Search" || a == "DrawFaithful");
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
                                : (c._definition.AbilityType == "Refresh"
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
                    lm.AddToList(go.GetComponent<Card>().addressableKey);
                    
                }
            }

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
                     && string.IsNullOrEmpty(c._definition.AbilityType))
                    .Select(c => c.gameObject)
                    .ToList();

                for (int i = 0; i < slotsLeft && noAbilityCandidates.Count > 0; i++)
                {
                    int idx = Random.Range(0, noAbilityCandidates.Count);
                    var go = noAbilityCandidates[idx];
                    noAbilityCandidates.RemoveAt(idx);
                    lm.AddToList(go.GetComponent<Card>().addressableKey);
                }
            }
        }

        private void PopulateLocationList()
        {
            const int maxLocations = 10;
            int needed = maxLocations - dsm.locationListTf.childCount;
            if (needed <= 0) return;

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
            }
        }
    }
}
