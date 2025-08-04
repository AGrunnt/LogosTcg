using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

namespace LogosTcg
{
    public class TurnManager : NetworkBehaviour
    {
        BoardElements be;
        public int index = 0;

        private void Start()
        {
            be = GetComponent<BoardElements>();
        }
        public void NextPhase()
        {
            index = (index + 1) % StaticData.playerNums;
            be.mainCamera.transform.DOLocalMoveX(19 * index, 1);
            Transform temptf = be.playerBoards[index];
            be.commonBoard.SetParent(be.playerBoards[index].transform);
        }
        
    }
}
