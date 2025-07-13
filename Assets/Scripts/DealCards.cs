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
        public List<Transform> col1;
        public List<Transform> col2;
        public List<Transform> col3;
        public List<Transform> col4;

        public void SetHands()
        {
            hands = GameObject.FindGameObjectsWithTag("Hand").ToList();
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
            Debug.Log($"start deal. player num {StaticData.playerNums}");

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
