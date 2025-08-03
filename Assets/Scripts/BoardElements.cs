using System.Collections.Generic;
using UnityEngine;

namespace LogosTcg
{
    public class BoardElements : MonoBehaviour
    {
        public static BoardElements instance;
        void Awake() => instance = this;

        public List<Transform> playerBoards;
        public List<Transform> columns;
        public List<Transform> faithfulDecks;
        public Transform locDeck;
        public List<Transform> hands;
        public List<Transform> locSlots;
        public Transform encountersDeck;

    }
}
