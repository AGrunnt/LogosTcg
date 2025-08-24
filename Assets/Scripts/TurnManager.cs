using DG.Tweening;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.CoreUtils;

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
        void Awake() => instance = this;
        //public textmesh pro

        private void Start()
        {
            be = GetComponent<BoardElements>();
            dc = GetComponent<DealCards>();
        }

        public void IncPlayCount()
        {
            playCount++;
        }

        public void NavPhase()
        {
            switch(currPhase)
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

            DrawEncounters0();
        }
        
        public void DrawEncounters0()
        {
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

                dc.SendTopTo(be.encountersDeck, slot);
                slot.GetComponent<SlotScript>().InitializeSlots();
                //break;
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
            Transform currentUsable = be.inPlayUsable[currPlayer];
            foreach(var stack in currentUsable.GetComponentsInChildren<CoinStack>())
            {
                Debug.Log("set coins");
                stack.ToggleVisible();
            }
        }


    }
}
