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
        ListManager lm;

        void Awake()
        {
            lm = ListManager.instance;
            instance = this;
        }
        

        [Header("hook these up in Inspector")]
        public List<Transform> faithfulListTf;
        public List<Transform> playerListTf;
        public List<Transform> faithfulBtnTf;
        public Transform encounterListTf;
        public Transform locationListTf;
        //public GameObject cardLinePrefab;

        [Header("Faithful?list stats UI")]
        public TextMeshProUGUI faithfulStatsText;    // drag in your Text component here

        public int currPlayer = 0;


        private void Start()
        {
            lm = GetComponent<ListManager>();
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
        public void UpdateFaithfulStats()
        {
            // count current
            var lines = faithfulListTf[currPlayer].GetComponentsInChildren<CardLine>();
            int currRare = lines.Count(l => l.cardDef.Rarity == "Rare");
            int currUncom = lines.Count(l => l.cardDef.Rarity == "Uncommon");
            int currCom = lines.Count(l => l.cardDef.Rarity == "Common");

            Debug.Log($"{currRare} {currUncom} {currCom}");
            // build text: “1 / 2 Rares, 3 / 5 Commons, …”
            faithfulStatsText.text =
                $"{currRare} / {lm.rareTot} Rares,  " +
                $"{currUncom} / {lm.uncomTot} Uncommons,  " +
                $"{currCom} / {lm.comTot} Commons";
        }

        // ???????????????????????????????????????????????????????????????
        // your existing AddToList / RemoveFromList go here unchanged…
        // (omitted for brevity)
        // ???????????????????????????????????????????????????????????????


        
    }
}
