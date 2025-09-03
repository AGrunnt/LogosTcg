using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using LogoTcg;
using DG.Tweening;

namespace LogosTcg
{
    public class DealCards : MonoBehaviour
    {
        BoardElements be;

        private void Awake()
        {
            be = BoardElements.instance;
        }

        public Transform SendTopTo(Transform src, Transform dest)
        {
            Transform top = src.GetChild(src.childCount - 1).transform;

            var orgSpeed = top.GetComponent<Gobject>().gobjectVisual.followSpeed;
            top.GetComponent<Gobject>().gobjectVisual.followSpeed = 10;
            top.SetParent(dest, true);
            top.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);

            top.GetComponent<Gobject>().gobjectVisual.followSpeed = orgSpeed;
            //top.SetParent(dest, true);

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
