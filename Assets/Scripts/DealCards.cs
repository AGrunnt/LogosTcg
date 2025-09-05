using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using LogoTcg;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;

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

            //var orgSpeed = top.GetComponent<Gobject>().gobjectVisual.followSpeed;
            //top.GetComponent<Gobject>().gobjectVisual.followSpeed = 7f;
            //GobjectVisual gv = top.GetComponent<Gobject>().gobjectVisual;
            //gv.SetFollowSpeedMan();
            //gv.SetFollowSpeed(3f);

            //Debug.Log("dc fl");
            if (dest.GetComponent<LayoutGroup>() == null)
            {
                top.SetParent(dest, true);
                top.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);
            }
            else
            {
                var orgSpeed = top.GetComponent<Gobject>().gobjectVisual.followSpeed;
                top.GetComponent<Gobject>().gobjectVisual.followSpeed = 7f;

                top.SetParent(dest, false);

                StartCoroutine(DelayedSetFollowSpeed(top, orgSpeed, 0.5f));
            }

            //top.GetComponent<Gobject>().gobjectVisual.followSpeed = orgSpeed;
            //top.SetParent(dest, true);

            return top;
        }

        IEnumerator DelayedSetFollowSpeed(Transform card, float speed, float time)
        {
            yield return new WaitForSeconds(time);

            card.GetComponent<Gobject>().gobjectVisual.followSpeed = speed;
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
