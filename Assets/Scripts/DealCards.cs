using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace LogosTcg
{
    public class DealCards : MonoBehaviour
    {
        public Transform locDeck;
        public Transform FaithfulDeck;
        public List<GameObject> hands;
        public List<Transform> locSlots;


        public void SetHands()
        {
            hands = GameObject
                .FindGameObjectsWithTag("Hand")
                .OrderBy(go => go.name)      // sort alphabetically by name
                .ToList();
        }

        public void SendTopTo(Transform src, Transform dest)
        {
            var top = src.GetChild(src.childCount - 1);
            top.transform.SetParent(dest, false);
        }

        public void StartingDeal()
        {
            int modifier = 0;
            if (StaticData.playerNums > 1) { modifier = 1; }

            for (int i = 0; i < 3 + modifier; i++)
            {
                SendTopTo(locDeck, locSlots[i]);
            }
            for (int i = 0; i < StaticData.playerNums; i++)
            {
                for (int j = 0; j < 3 - modifier; j++)
                {
                    SendTopTo(FaithfulDeck, hands[i].transform);
                }
            }

        }
    }
}
