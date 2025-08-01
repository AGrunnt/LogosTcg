using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

namespace LogosTcg
{
    public class PopulateOnlineList : NetworkBehaviour
    {
        ListManager lm;
        DeckSceneManager dsm;
        CardLoader cl;
        FaithfulListsManager flm;
        GridManager gm;

        public List<string> keyList = new List<string>();
        public List<string> playerList = new List<string>();
        public List<string> listTypeList = new List<string>();

        private HashSet<string> usedKeys = new();


        void Start()
        {
            lm = ListManager.instance;
            dsm = DeckSceneManager.instance;
            cl = CardLoader.instance;
            flm = FaithfulListsManager.instance;
            gm = GridManager.instance;

            // fire off the population tasks 
        }

        public void AutoPopulateOnlineAll()
        {
            playerList.Clear();
            keyList.Clear();
            listTypeList.Clear();
            AutoPopulateAllFaithful();
            PopulateEncounterList();
            PopulateLocationList();
            PopulateOnlineListServerRpc(string.Join(";", keyList), string.Join(";", playerList), string.Join(";", listTypeList));
        }


        private void AutoPopulateAllFaithful()
        {
            for (int i = 0; i < dsm.faithfulListTf.Count; i++)
            {
                PopulateFaithfulList(i);
            }
        }

        private void PopulateFaithfulList(int playerNum)
        {

            var tf = dsm.faithfulListTf[playerNum];

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
                    Debug.LogWarning($"Cannot add more than {flm.maxTot} Faithful cards.");
                    return;
                }

                string rarity = kv.Key;
                int desired = kv.Value;

                int current = tf.GetComponentsInChildren<CardLine>()
                                .Count(l => l.cardDef.Rarity == rarity);

                int needed = desired - current;
                if (needed <= 0) continue;

                /*
                var candidates = gm.cardGridTf
                    .GetComponentsInChildren<Card>()
                    .Where(c => c._definition.Type.Contains("Faithful")
                             && c._definition.Rarity == rarity)
                    .Select(c => c.gameObject)
                    .ToList();
                */
                var candidates = gm.cardGridTf
                    .GetComponentsInChildren<Card>()
                    .Where(c => c._definition.Type.Contains("Faithful")
                             && c._definition.Rarity == rarity
                             && !usedKeys.Contains(c.addressableKey)) // ? filter out used keys
                    .Select(c => c.gameObject)
                    .ToList();

                for (int i = 0; i < needed && candidates.Count > 0; i++)
                {
                    int idx = Random.Range(0, candidates.Count);
                    var go = candidates[idx];
                    candidates.RemoveAt(idx);
                    playerList.Add(playerNum.ToString());
                    keyList.Add(go.GetComponent<Card>().addressableKey);
                    listTypeList.Add("0");
                    usedKeys.Add(go.GetComponent<Card>().addressableKey);
                    //GetComponent<ListOnlineManager>().RemoveCardFromGridIfPresent(go.GetComponent<Card>().addressableKey); //might be unnecissary because it removes all
                    //lm.AddToList(go.GetComponent<Card>().addressableKey);
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

                /*
                var candidates = gm.cardGridTf
                    .GetComponentsInChildren<Card>()
                    .Where(c => c._definition.Type.Contains(typeKey))
                    .Select(c => c.gameObject)
                    .ToList();
                */
                
                var candidates = gm.cardGridTf
                    .GetComponentsInChildren<Card>()
                    .Where(c => c._definition.Type.Contains(typeKey) && !usedKeys.Contains(c.addressableKey))
                    .Select(c => c.gameObject)
                    .ToList();

                for (int i = 0; i < needed && candidates.Count > 0; i++)
                {
                    int idx = Random.Range(0, candidates.Count);
                    var go = candidates[idx];
                    candidates.RemoveAt(idx);
                    keyList.Add(go.GetComponent<Card>().addressableKey);
                    playerList.Add("0");
                    listTypeList.Add("2");
                    usedKeys.Add(go.GetComponent<Card>().addressableKey);
                    //lm.AddToList(go.GetComponent<Card>().addressableKey);
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
                            && !usedKeys.Contains(c.addressableKey)
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
                    keyList.Add(go.GetComponent<Card>().addressableKey);
                    playerList.Add("0");
                    listTypeList.Add("2");
                    usedKeys.Add(go.GetComponent<Card>().addressableKey);
                    //lm.AddToList(go.GetComponent<Card>().addressableKey);

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

                    && !usedKeys.Contains(c.addressableKey)
                     && string.IsNullOrEmpty(c._definition.AbilityType))
                    .Select(c => c.gameObject)
                    .ToList();

                for (int i = 0; i < slotsLeft && noAbilityCandidates.Count > 0; i++)
                {
                    int idx = Random.Range(0, noAbilityCandidates.Count);
                    var go = noAbilityCandidates[idx];
                    noAbilityCandidates.RemoveAt(idx);
                    keyList.Add(go.GetComponent<Card>().addressableKey);
                    playerList.Add("0");
                    listTypeList.Add("2");
                    usedKeys.Add(go.GetComponent<Card>().addressableKey);
                    //lm.AddToList(go.GetComponent<Card>().addressableKey);
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
                .Where(c => c._definition.Type.Contains("Location") && !usedKeys.Contains(c.addressableKey))
                .Select(c => c.gameObject)
                .ToList();

            for (int i = 0; i < needed && candidates.Count > 0; i++)
            {
                int idx = Random.Range(0, candidates.Count);
                var go = candidates[idx];
                candidates.RemoveAt(idx);
                keyList.Add(go.GetComponent<Card>().addressableKey);
                playerList.Add("0");
                listTypeList.Add("1");
                usedKeys.Add(go.GetComponent<Card>().addressableKey);
                //lm.AddToList(go.GetComponent<Card>().addressableKey);
            }
        }

        [ServerRpc]
        void PopulateOnlineListServerRpc(string keyList, string playerList, string listTypeList)
        {
            PopulateOnlineListClientRpc(keyList, playerList, listTypeList);
        }

        [ClientRpc]
        void PopulateOnlineListClientRpc(string keyList, string playerList, string listTypeList)
        {
            //Debug.Log(keyList.Count());
            for(int i = 0; i < keyList.Split(";").Count() - 1; i++)
            {
                AddToOnlineList(keyList.Split(";")[i], int.Parse(playerList.Split(";")[i]), int.Parse(listTypeList.Split(";")[i]));
                //AddToOnlineListServerRpc
            }
            // 4) update UI stats if it was a faithful
            UpdateFaithfulOnlineStats();
        }

        async Task UpdateFaithfulOnlineStats()
        {
            await Task.Delay(1000);
            dsm.UpdateFaithfulStats();
        }

        public void AddToOnlineList(string key, int player, int listType)
        {
            

            // 2) mark it assigned so loader never spawns it
            //lm.listItems.add(key);

            Transform parent = listType == 0
                ? dsm.faithfulListTf[player]
                : listType == 1
                    ? dsm.locationListTf
                    : dsm.encounterListTf;

            var lineGO = Instantiate(lm.cardLinePrefab, parent);
            lm.listItems.Add(key, lineGO);
            var line = lineGO.GetComponent<CardLine>();
            line.addressableKey = key;

            // 3) load or reuse the CardDef handle safely
            if (!cl.loadedAssets.TryGetValue(key, out var handle))
            {
                // client never loaded this key, so start loading now
                handle = Addressables.LoadAssetAsync<CardDef>(key);
                cl.loadedAssets[key] = handle;
            }

            // 4) when the handle completes (or is already done), apply the definition
            if (handle.IsDone && handle.Status == AsyncOperationStatus.Succeeded)
            {
                line.cardDef = handle.Result;
                line.Apply();
            }
            else
            {
                handle.Completed += op =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        line.cardDef = op.Result;
                        line.Apply();
                    }
                    else
                    {
                        Debug.LogError($"Failed to load CardDef for key {key}");
                    }
                };
            }
           
        }
    }
}
