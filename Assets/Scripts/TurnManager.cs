

using DG.Tweening;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Rendering.CoreUtils;
using NUnit.Framework.Internal;
using System;
using System.Collections;
using LogoTcg;
using Unity.VisualScripting;
using UnityEngine.InputSystem.HID;

namespace LogosTcg
{
    public class TurnManager : NetworkBehaviour
    {
        BoardElements be;
        DealCards dc;
        public int currPlayer = 0;
        public string currPhase = "DrawEnc";
        public static TurnManager instance;
        public int playCount = 0;
        public int playCountAvailable = 1;
        public GameObject nextPhaseBtn;
        public GameManager gm;
        public event Action OnEndTurn;
        void Awake() => instance = this;
        //public textmesh pro

        private void Start()
        {
            be = GetComponent<BoardElements>();
            dc = GetComponent<DealCards>();
            gm = GameManager.Instance;

            if (NetworkManager.Singleton != null)
            {
                if (NetworkManager.Singleton.LocalClientId != 0)
                    nextPhaseBtn.SetActive(false);
            }
        }

        public void IncPlayCount()
        {
            playCount++;
        }

        public void NavPhase()
        {
            if(NetworkManager.Singleton != null)
            {
                NavPhaseServerRpc();
            } else
            {
                OfflineNavPhase();
            }

        }

        [ServerRpc(RequireOwnership = false)]
        public void NavPhaseServerRpc()
        {
            NavPhaseClientRpc();
        }

        [ClientRpc]
        public void NavPhaseClientRpc()
        {
            OfflineNavPhase();

        }

        public void OfflineNavPhase()
        {
            switch (currPhase)
            {
                case "Play":
                    SetCoins();
                    currPhase = "Spend";
                    break;
                case "Spend":
                    SetCoins();
                    currPhase = "Play";
                    StartCoroutine(EndTurn());
                    playCount = 0;
                    break;
                case "DrawEnc": //only runs at the start of the game, then is auto done with endturn
                    currPhase = "Play";
                    StartCoroutine(DrawEncounters0());
                    break;
            }
        }

        public void StartTurn0()
        {
            currPlayer = (currPlayer + 1) % StaticData.playerNums;
            be.mainCamera.transform.DOLocalMoveX(19 * currPlayer, 1);
            Transform temptf = be.playerBoards[currPlayer];
            be.commonBoard.SetParent(be.playerBoards[currPlayer].transform);
            be.commonBoard.transform.DOLocalMoveX(0, 1.5f);

            if(NetworkManager.Singleton != null)
            {
                Debug.Log($"{currPlayer} and {(int)NetworkManager.Singleton.LocalClientId}");
                if(currPlayer == (int)NetworkManager.Singleton.LocalClientId)
                {
                    nextPhaseBtn.SetActive(true);
                } else
                {
                    nextPhaseBtn.SetActive(false);
                }
            }

            StartCoroutine(DrawEncounters0());
        }

        IEnumerator DrawEncounters0()   //public void DrawEncounters0()
        {
            yield return new WaitForSeconds(1.5f);

            List<Transform> tfList = new List<Transform>();
            for (int i = 0; i < be.locSlots.Count; i++)
            {
                if (be.locSlots[i].GetComponentInChildren<Card>() == null)
                    continue;

                Transform slot = null;

                for(int j = i; j < be.locSlots.Count; j++)
                {
                    if (be.stackSlots[j].GetComponentsInChildren<Card>().Count() >= 3)
                        continue;
                    slot = be.stackSlots[j];
                    break;

                }

                //want one to shoot out and half way through the next one will be coming out
                yield return new WaitForSeconds(0.2f);
                
                //Transform top = be.encountersDeck.GetChild(be.encountersDeck.childCount - 1).transform;
                //GobjectVisual gv = top.GetComponent<Gobject>().gobjectVisual;
                //float orgSpeed = gv.followSpeed;
                //gv.SetFollowSpeed(5f);


                Transform topCard = dc.SendTopTo(be.encountersDeck, slot);
                tfList.Add(topCard);
                topCard.GetComponent<Card>().InPlay = true;
                topCard.GetComponent<CardTurnEvents>().TrySubscribeOnEndTurn();
                gm.AddString(topCard.GetComponent<Card>()._definition.Title);
                //slot.GetComponent<SlotScript>().InitializeSlots();

                //gv.SetFollowSpeed(orgSpeed);
                
            }

            yield return new WaitForSeconds(0.5f);

            foreach (var slot in be.stackSlots)
            {
                slot.GetComponent<SlotScript>().InitializeSlots();
            }

            yield return new WaitForSeconds(1);
            //I want a light pause here for maybe 1 second
            foreach (Transform tf in tfList)
            {
                //want one to shoot out and when its most the way through the next one will be coming out
                yield return new WaitForSeconds(0.3f);
                string type0 = tf.GetComponent<Card>()._definition.Type[0];
                List<Ability> abilities = tf.GetComponent<Card>()._definition.Abilities;

                foreach(Ability ab in abilities)
                {
                    if (ab.AbilityType[0] == "Add" && gm.inString.Contains(ab.Target[0]))
                    {
                        tf.GetComponent<Card>().SetValue(int.Parse(ab.Tag[0]));
                    }

                    if (ab.AbilityType[0] == "Minus" && gm.inString.Contains(ab.Target[0]))
                    {
                        tf.GetComponent<Card>().SetValue(-int.Parse(ab.Tag[0]));
                    }
                }

                if (new[] { "Support", "Neutral"}.Contains(type0) || (type0 == "Event" && tf.GetComponent<Card>()._definition.Value == 0))
                {
                    //tf.SetParent(be.hands[currPlayer], false);
                    //tf.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);
                    

                    GobjectVisual gv = tf.GetComponent<Gobject>().gobjectVisual;
                    float orgSpeed = gv.followSpeed;
                    gv.SetFollowSpeed(5f);

                    tf.SetParent(be.hands[currPlayer], false);
                    be.hands[currPlayer].GetComponent<SlotScript>().InitializeSlots();

                    StartCoroutine(DelayedSetFollowSpeed(tf, orgSpeed, 0.5f));
                    //gv.SetFollowSpeed(orgSpeed);
                }

                if (type0 == "Trap")
                {
                    tf.SetParent(be.discard, true);
                    tf.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);
                    be.discard.GetComponent<SlotScript>().InitializeSlots();
                }

                if(type0 == "Faithless")
                {
                    if(!tf.parent.GetComponent<ColumnScript>().FaithlessAllowed) //occupy ability
                    {
                        int idx = UnityEngine.Random.Range(0, be.encountersDeck.childCount); // 0..childCount-1
                        tf.SetParent(be.encountersDeck, true);
                        tf.SetSiblingIndex(idx);
                        tf.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);
                        be.encountersDeck.GetComponent<SlotScript>().InitializeSlots();
                    } else
                    {
                        gm.AddString(tf.GetComponent<Card>()._definition.Title);
                        GetComponent<FaithlessAbilities>().RunAbilities(tf);
                    }
                }

            }
        }

        IEnumerator DelayedSetFollowSpeed(Transform card, float speed, float time)
        {
            yield return new WaitForSeconds(time);

            card.GetComponent<Gobject>().gobjectVisual.followSpeed = speed;
        }

        IEnumerator EndTurn()
        {
            DiscardZeroedLocs();
            dc.SendTopTo(be.faithfulDecks[currPlayer], be.hands[currPlayer]);
            be.hands[currPlayer].GetComponent<SlotScript>().InitializeSlots();
            OnEndTurn?.Invoke();
            yield return new WaitForSeconds(0.5f);
            StartTurn0();
        }

        public void SetCoins()
        {
            if (NetworkManager.Singleton != null && currPlayer != (int)NetworkManager.Singleton.LocalClientId)
                return;

            Transform currentUsable = be.inPlayUsable[currPlayer];
            foreach(var stack in currentUsable.GetComponentsInChildren<CoinStack>())
            {
                //Debug.Log("set coins");
                stack.ToggleVisible();
            }
        }

        public void DiscardZeroedLocs()
        {
            foreach(Transform tf in be.locSlots)
            {
                if (tf.GetComponentInChildren<CardDropHandler>() != null)
                {
                    tf.GetComponentInChildren<CardDropHandler>().DiscardZeroed();
                }
            }
        }
    }
}
