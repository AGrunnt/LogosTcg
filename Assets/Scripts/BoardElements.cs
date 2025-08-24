using System.Collections.Generic;
using UnityEngine;

namespace LogosTcg
{
    public class BoardElements : MonoBehaviour
    {
        public static BoardElements instance;
        void Awake() => instance = this;

        public List<Transform> inPlayUsable;
        public List<Transform> inPlayNotUsable;
        public List<Transform> playerBoards;
        public List<Transform> columns;
        public List<Transform> faithfulDecks;
        public Transform locDeck;
        public List<Transform> hands;
        public List<Transform> locSlots;
        public List<Transform> stackSlots;
        public Transform encountersDeck;
        public Transform commonBoard;
        public Transform mainCamera;

    }
}
