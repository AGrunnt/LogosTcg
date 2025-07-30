using System.Linq;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;

namespace LogosTcg
{
    public class FaithfulListsManager : MonoBehaviour
    {
        DeckSceneManager dsm;

        public static FaithfulListsManager instance;


        public int maxTot = 0;
        public int rareTot = 0;
        public int uncomTot = 0;
        public int comTot = 0;

        private void Awake()
        {
            instance = this;
        }

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

        public bool ValidFaithful(CardDef cd)
        {
            // total cap
            if (dsm.faithfulListTf[GetComponent<DeckSceneManager>().currPlayer].childCount >= maxTot)
            {
                Debug.LogWarning($"Cannot add more than {maxTot} Faithful cards.");
                return false;
            }

            // rarity cap
            int currRare = dsm.faithfulListTf[GetComponent<DeckSceneManager>().currPlayer].GetComponentsInChildren<CardLine>()
                              .Count(l => l.cardDef.Rarity == "Rare");
            int currUncom = dsm.faithfulListTf[GetComponent<DeckSceneManager>().currPlayer].GetComponentsInChildren<CardLine>()
                              .Count(l => l.cardDef.Rarity == "Uncommon");
            int currCom = dsm.faithfulListTf[GetComponent<DeckSceneManager>().currPlayer].GetComponentsInChildren<CardLine>()
                              .Count(l => l.cardDef.Rarity == "Common");

            if (cd.Rarity == "Rare" && currRare >= rareTot) { Debug.LogWarning($"Max {rareTot} Rares."); return false; }
            if (cd.Rarity == "Uncommon" && currUncom >= uncomTot) { Debug.LogWarning($"Max {uncomTot} Uncommons."); return false; }
            if (cd.Rarity == "Common" && currCom >= comTot) { Debug.LogWarning($"Max {comTot} Commons."); return false; }

            return true;
        }
    }
}
