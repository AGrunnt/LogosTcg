using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using System.Linq;
using Unity.Netcode;

namespace LogosTcg
{
    public class ListManager : MonoBehaviour
    {

        public static ListManager instance;
        ListOnlineManager lom;

        void Awake()
        {
            instance = this;
            lom = GetComponent<ListOnlineManager>();
        }
        
        /*
        public List<Transform> faithfulListTf;
        public List<Transform> playerListTf;
        public List<Transform> faithfulBtnTf;
        public Transform encounterListTf;
        public Transform locationListTf;
        */
        DeckSceneManager dsm;
        public GameObject cardLinePrefab;

        public int maxTot = 0;
        public int rareTot = 0;
        public int uncomTot = 0;
        public int comTot = 0;

        void Start()
        {
            dsm = DeckSceneManager.instance;

            int playerCount;
            if (NetworkManager.Singleton != null)
                playerCount = 1;
            else
                playerCount = StaticData.playerNums;

            for (int i = 3; i > StaticData.playerNums - 1; i--)
            {
                dsm.faithfulListTf.RemoveAt(i);
                dsm.playerListTf.RemoveAt(i);
                dsm.faithfulBtnTf[i].gameObject.SetActive(false);
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

            dsm.UpdateFaithfulStats();
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
            if (isEncounter && dsm.encounterListTf.childCount >= 54)
            {
                Debug.LogWarning("Cannot add more than 54 Encounter cards.");
                return;
            }

            if (isLocation && dsm.locationListTf.childCount >= 10)
            {
                Debug.LogWarning("Cannot add more than 10 Location cards.");
                return;
            }

            if (isFaithful)
            {
                // total cap
                if (dsm.faithfulListTf[GetComponent<DeckSceneManager>().currPlayer].childCount >= maxTot)
                {
                    Debug.LogWarning($"Cannot add more than {maxTot} Faithful cards.");
                    return;
                }

                // rarity cap
                int currRare = dsm.faithfulListTf[GetComponent<DeckSceneManager>().currPlayer].GetComponentsInChildren<CardLine>()
                                  .Count(l => l.cardDef.Rarity == "Rare");
                int currUncom = dsm.faithfulListTf[GetComponent<DeckSceneManager>().currPlayer].GetComponentsInChildren<CardLine>()
                                  .Count(l => l.cardDef.Rarity == "Uncommon");
                int currCom = dsm.faithfulListTf[GetComponent<DeckSceneManager>().currPlayer].GetComponentsInChildren<CardLine>()
                                  .Count(l => l.cardDef.Rarity == "Common");

                if (cd.Rarity == "Rare" && currRare >= rareTot) { Debug.LogWarning($"Max {rareTot} Rares."); return; }
                if (cd.Rarity == "Uncommon" && currUncom >= uncomTot) { Debug.LogWarning($"Max {uncomTot} Uncommons."); return; }
                if (cd.Rarity == "Common" && currCom >= comTot) { Debug.LogWarning($"Max {comTot} Commons."); return; }
            }
            // ?????????????????????????????????????????????????????????????

            int listType = c._definition.Type.Contains("Faithful") ? 0
                         : c._definition.Type.Contains("Location") ? 1
                         : 2;

            if (NetworkManager.Singleton != null)
            {
                lom.AddToOnlineListServerRpc(key, dsm.currPlayer, listType);
                return;
            }

            // 2) mark it assigned so loader never spawns it
            LoadingCards.instance.listAssigned.Add(key);

            // 3) spawn a CardLine in the right list
            var parent = isFaithful ? dsm.faithfulListTf[GetComponent<DeckSceneManager>().currPlayer]
                       : isLocation ? dsm.locationListTf
                                     : dsm.encounterListTf;

            var line = Instantiate(cardLinePrefab, parent)
                       .GetComponent<CardLine>();

            line.cardDef = cd;
            line.Apply();
            line.addressableKey = key;

            // 4) destroy the grid object + clear our internal map
            Destroy(gridCardObj);
            LoadingCards.instance.RemoveGridCardMapping(key);

            // 5) update our stats display
            if (isFaithful) GetComponent<DeckSceneManager>().UpdateFaithfulStats();
        }

        // ---------------------------------------------------
        // Take a CardLine out of a list & pop it back into the grid:
        public void RemoveFromList(GameObject cardLineObj)
        {
            var line = cardLineObj.GetComponent<CardLine>();
            var cd = line.cardDef;
            var key = line.addressableKey;
            int listType = line.cardDef.Type.Contains("Faithful") ? 0
                         : line.cardDef.Type.Contains("Location") ? 1
                         : 2;

            if (NetworkManager.Singleton != null)
            {
                lom.RemoveFromOnlineListServerRpc(key, listType, dsm.currPlayer);
                return;
            }

            // 1) un?mark so loader can spawn it again
            LoadingCards.instance.listAssigned.Remove(key);

            // 2) destroy the line UI
            Destroy(cardLineObj);

            // 3) respawn into grid (if it still matches)
            LoadingCards.instance.AddCardToGrid(key, cd);

            // 4) if it was a Faithful line, update stats
            if (cd.Type.Contains("Faithful"))
                GetComponent<DeckSceneManager>().UpdateFaithfulStats();
        }
    }
}
