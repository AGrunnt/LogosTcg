using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace LogosTcg
{
    public class DealCards : MonoBehaviour
    {
        BoardElements be;

        private void Start()
        {
            be = BoardElements.instance;
        }

        /*
        public void SetHands()
        {
            hands = GameObject
                .FindGameObjectsWithTag("Hand")
                .OrderBy(go => go.name)      // sort alphabetically by name
                .ToList();
        }
        */
        public Transform SendTopTo(Transform src, Transform dest)
        {
            Transform top = src.GetChild(src.childCount - 1).transform;
            top.SetParent(dest, false);
            return top;
        }

        public void StartingDeal()
        {
            int modifier = 0;
            if (StaticData.playerNums > 1) { modifier = 1; }
            //Debug.Log($"player nums {StaticData.playerNums}");
            for (int i = 0; i < 3 + modifier; i++)
            {
                SendTopTo(be.locDeck, be.locSlots[i]);
            }
            for (int i = 0; i < StaticData.playerNums; i++)
            {
                for (int j = 0; j < 3 - modifier; j++)
                {
                    SendTopTo(be.faithfulDecks[i], be.hands[i]);
                }
            }

        }
    }
}
