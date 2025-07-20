using System.Linq;
using UnityEngine;
using UnityEngine.UI;        // <-- for Text
using LogosTcg;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;

namespace LogosTcg
{
    public class DeckSceneManager : MonoBehaviour
    {
        public static DeckSceneManager instance;
        void Awake() => instance = this;

        [Header("hook these up in Inspector")]
        public List<Transform> faithfulListTf;
        public List<Transform> playerListTf;
        public List<Transform> faithfulBtnTf;
        public Transform encounterListTf;
        public Transform locationListTf;
        public GameObject cardLinePrefab;

        [Header("Faithful?list stats UI")]
        public TextMeshProUGUI faithfulStatsText;    // drag in your Text component here

        public int currPlayer = 0;

        int maxTot;
        int rareTot;
        int uncomTot;
        int comTot;

        void Start()
        {
            int playerCount;
            if (NetworkManager.Singleton != null)
                playerCount = 1;
            else
                playerCount = StaticData.playerNums;

            for (int i = 3; i > StaticData.playerNums - 1; i--)
            {
                faithfulListTf.RemoveAt(i);
                playerListTf.RemoveAt(i);
                faithfulBtnTf[i].gameObject.SetActive(false);
            }

                switch (StaticData.playerNums)
            {
                case 3:
                    maxTot = 10; rareTot = 2; uncomTot = 3; comTot = 5;
                    break;
                case 4:
                    maxTot = 8; rareTot = 1; uncomTot = 3; comTot = 4;
                    break;
                default:
                    maxTot = 12; rareTot = 2; uncomTot = 4; comTot = 6;
                    break;
            }

            UpdateFaithfulStats();
        }

        // ---------------------------------------------------
        // Move a grid?card into one of the three lists:
        public void AddToList(GameObject gridCardObj)
        {
            var c = gridCardObj.GetComponent<Card>();
            var cd = c._definition;
            var key = c.addressableKey;

            bool isFaithful = cd.Type.Contains("Faithful");
            bool isLocation = cd.Type.Contains("Location");
            bool isEncounter = !isFaithful && !isLocation;

            // ?? enforce per?list & per?rarity caps ???????????????????????
            if (isEncounter && encounterListTf.childCount >= 54)
            {
                Debug.LogWarning("Cannot add more than 54 Encounter cards.");
                return;
            }

            if (isLocation && locationListTf.childCount >= 10)
            {
                Debug.LogWarning("Cannot add more than 10 Location cards.");
                return;
            }

            if (isFaithful)
            {
                // total cap
                if (faithfulListTf[currPlayer].childCount >= maxTot)
                {
                    Debug.LogWarning($"Cannot add more than {maxTot} Faithful cards.");
                    return;
                }

                // rarity cap
                int currRare = faithfulListTf[currPlayer].GetComponentsInChildren<CardLine>()
                                  .Count(l => l.cardDef.Rarity == "Rare");
                int currUncom = faithfulListTf[currPlayer].GetComponentsInChildren<CardLine>()
                                  .Count(l => l.cardDef.Rarity == "Uncommon");
                int currCom = faithfulListTf[currPlayer].GetComponentsInChildren<CardLine>()
                                  .Count(l => l.cardDef.Rarity == "Common");

                if (cd.Rarity == "Rare" && currRare >= rareTot) { Debug.LogWarning($"Max {rareTot} Rares."); return; }
                if (cd.Rarity == "Uncommon" && currUncom >= uncomTot) { Debug.LogWarning($"Max {uncomTot} Uncommons."); return; }
                if (cd.Rarity == "Common" && currCom >= comTot) { Debug.LogWarning($"Max {comTot} Commons."); return; }
            }
            // ?????????????????????????????????????????????????????????????

            // 2) mark it assigned so loader never spawns it
            LoadingCards.instance.listAssigned.Add(key);

            // 3) spawn a CardLine in the right list
            var parent = isFaithful ? faithfulListTf[currPlayer]
                       : isLocation ? locationListTf
                                     : encounterListTf;

            var line = Instantiate(cardLinePrefab, parent)
                       .GetComponent<CardLine>();

            line.cardDef = cd;
            line.Apply();
            line.addressableKey = key;

            // 4) destroy the grid object + clear our internal map
            Destroy(gridCardObj);
            LoadingCards.instance.RemoveGridCardMapping(key);

            // 5) update our stats display
            if (isFaithful) UpdateFaithfulStats();
        }

        // ---------------------------------------------------
        // Take a CardLine out of a list & pop it back into the grid:
        public void RemoveFromList(GameObject cardLineObj)
        {
            var line = cardLineObj.GetComponent<CardLine>();
            var cd = line.cardDef;
            var key = line.addressableKey;

            // 1) un?mark so loader can spawn it again
            LoadingCards.instance.listAssigned.Remove(key);

            // 2) destroy the line UI
            Destroy(cardLineObj);

            // 3) respawn into grid (if it still matches)
            LoadingCards.instance.AddCardToGrid(key, cd);

            // 4) if it was a Faithful line, update stats
            if (cd.Type.Contains("Faithful"))
                UpdateFaithfulStats();
        }

        public void SetPlayer(int player)
        {
            currPlayer = player;
            UpdateFaithfulStats();
            foreach(var playerTf in playerListTf)
            {
                playerTf.gameObject.SetActive(false);
            }
            playerListTf[currPlayer].gameObject.SetActive(true);
        }

        // ---------------------------------------------------
        void UpdateFaithfulStats()
        {
            // count current
            var lines = faithfulListTf[currPlayer].GetComponentsInChildren<CardLine>();
            int currRare = lines.Count(l => l.cardDef.Rarity == "Rare");
            int currUncom = lines.Count(l => l.cardDef.Rarity == "Uncommon");
            int currCom = lines.Count(l => l.cardDef.Rarity == "Common");

            // build text: “1 / 2 Rares, 3 / 5 Commons, …”
            faithfulStatsText.text =
                $"{currRare} / {rareTot} Rares,  " +
                $"{currUncom} / {uncomTot} Uncommons,  " +
                $"{currCom} / {comTot} Commons";
        }

        // ???????????????????????????????????????????????????????????????
        // your existing AddToList / RemoveFromList go here unchanged…
        // (omitted for brevity)
        // ???????????????????????????????????????????????????????????????


        // call this when you want to finish?populate everything:
        public void AutoPopulateAll()
        {
            AutoPopulateAllFaithful();
            PopulateEncounterList();
            PopulateLocationList();
        }

        public void AutoPopulateAllFaithful()
        {
            int old = currPlayer;
            for (int i = 0; i < faithfulListTf.Count; i++)
            {
                currPlayer = i;
                PopulateFaithfulList();
            }
            currPlayer = old;
        }

        void PopulateFaithfulList()
        {
            var tf = faithfulListTf[currPlayer];
            // your per?rarity targets:
            var targets = new Dictionary<string, int>
            {
                ["Rare"] = rareTot,
                ["Uncommon"] = uncomTot,
                ["Common"] = comTot
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
                    AddToList(go);
                }
            }

            UpdateFaithfulStats();
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

                int current = encounterListTf
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
                    AddToList(go);
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

                int current = encounterListTf
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
                    AddToList(go);
                }
            }

            // 3) now handle Events with NO AbilityType
            //    fill any remaining slots up to totalEventSlots
            int currentEvents = encounterListTf
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
                    AddToList(go);
                }
            }
        }


        void PopulateLocationList()
        {
            // assume max 10 locations; if yours varies, pull from a variable
            const int maxLocations = 10;
            int needed = maxLocations - locationListTf.childCount;
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
                AddToList(go);
            }
        }
    }
}
