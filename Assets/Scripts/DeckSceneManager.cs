using System.Linq;
using UnityEngine;
using UnityEngine.UI;        // <-- for Text
using LogosTcg;
using TMPro;

namespace LogosTcg
{
    public class DeckSceneManager : MonoBehaviour
    {
        public static DeckSceneManager instance;
        void Awake() => instance = this;

        [Header("hook these up in Inspector")]
        public Transform faithfulListTf;
        public Transform encounterListTf;
        public Transform locationListTf;
        public GameObject cardLinePrefab;

        [Header("Faithful?list stats UI")]
        public TextMeshProUGUI faithfulStatsText;    // drag in your Text component here

        int maxTot;
        int rareTot;
        int uncomTot;
        int comTot;

        void Start()
        {
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
                if (faithfulListTf.childCount >= maxTot)
                {
                    Debug.LogWarning($"Cannot add more than {maxTot} Faithful cards.");
                    return;
                }

                // rarity cap
                int currRare = faithfulListTf.GetComponentsInChildren<CardLine>()
                                  .Count(l => l.cardDef.Rarity == "Rare");
                int currUncom = faithfulListTf.GetComponentsInChildren<CardLine>()
                                  .Count(l => l.cardDef.Rarity == "Uncommon");
                int currCom = faithfulListTf.GetComponentsInChildren<CardLine>()
                                  .Count(l => l.cardDef.Rarity == "Common");

                if (cd.Rarity == "Rare" && currRare >= rareTot) { Debug.LogWarning($"Max {rareTot} Rares."); return; }
                if (cd.Rarity == "Uncommon" && currUncom >= uncomTot) { Debug.LogWarning($"Max {uncomTot} Uncommons."); return; }
                if (cd.Rarity == "Common" && currCom >= comTot) { Debug.LogWarning($"Max {comTot} Commons."); return; }
            }
            // ?????????????????????????????????????????????????????????????

            // 2) mark it assigned so loader never spawns it
            LoadingCards.instance.listAssigned.Add(key);

            // 3) spawn a CardLine in the right list
            var parent = isFaithful ? faithfulListTf
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

        // ---------------------------------------------------
        void UpdateFaithfulStats()
        {
            // count current
            var lines = faithfulListTf.GetComponentsInChildren<CardLine>();
            int currRare = lines.Count(l => l.cardDef.Rarity == "Rare");
            int currUncom = lines.Count(l => l.cardDef.Rarity == "Uncommon");
            int currCom = lines.Count(l => l.cardDef.Rarity == "Common");

            // build text: “1 / 2 Rares, 3 / 5 Commons, …”
            faithfulStatsText.text =
                $"{currRare} / {rareTot} Rares,  " +
                $"{currUncom} / {uncomTot} Uncommons,  " +
                $"{currCom} / {comTot} Commons";
        }
    }
}
