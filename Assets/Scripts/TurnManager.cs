using DG.Tweening;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Rendering.CoreUtils;
using NUnit.Framework.Internal;
using System;

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
        void Awake() => instance = this;
        //public textmesh pro

        private void Start()
        {
            be = GetComponent<BoardElements>();
            dc = GetComponent<DealCards>();

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
                Debug.Log("online");
                NavPhaseServerRpc();
            } else
            {
                Debug.Log("offline");
                OfflineNavPhase();
            }

        }

        [ServerRpc(RequireOwnership = false)]
        public void NavPhaseServerRpc()
        {
            Debug.Log("server");
            NavPhaseClientRpc();
        }

        [ClientRpc]
        public void NavPhaseClientRpc()
        {
            Debug.Log("client");
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
                    EndTurn();
                    playCount = 0;
                    break;
                case "DrawEnc": //only runs at the start of the game, then is auto done with endturn
                    currPhase = "Play";
                    DrawEncounters0();
                    break;
            }
        }

        public void StartTurn0()
        {
            currPlayer = (currPlayer + 1) % StaticData.playerNums;
            be.mainCamera.transform.DOLocalMoveX(19 * currPlayer, 1);
            Transform temptf = be.playerBoards[currPlayer];
            be.commonBoard.SetParent(be.playerBoards[currPlayer].transform);

            //be.commonBoard.localPosition = Vector3.zero;
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

            DrawEncounters0();
        }
        
        public void DrawEncounters0()
        {
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

                Transform topCard = dc.SendTopTo(be.encountersDeck, slot);
                tfList.Add(topCard);
                slot.GetComponent<SlotScript>().InitializeSlots();
                //break;
            }

            foreach(Transform tf in tfList)
            {
                string type0 = tf.GetComponent<Card>()._definition.Type[0];

                if (new[] { "Support", "Neutral"}.Contains(type0) || (type0 == "Event" && tf.GetComponent<Card>()._definition.Value == 0))
                {
                    tf.SetParent(be.hands[currPlayer], true);
                    tf.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);
                    be.hands[currPlayer].GetComponent<SlotScript>().InitializeSlots();
                    //top.SetParent(dest, false);
                }

                if (type0 == "Trap")
                {
                    tf.SetParent(be.discard, true);
                    tf.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);
                    be.discard.GetComponent<SlotScript>().InitializeSlots();
                    //top.SetParent(dest, false);
                }

            }
        }

        public void EndTurn()
        {
            dc.SendTopTo(be.faithfulDecks[currPlayer], be.hands[currPlayer]);
            be.hands[currPlayer].GetComponent<SlotScript>().InitializeSlots();
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


    }
}
