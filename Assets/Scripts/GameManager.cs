using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace LogosTcg
{
    public class GameManager : MonoBehaviour
    {
        public bool setUpFinished = false;
        public List<Transform> columns;
        public static GameManager Instance;
        public List<Transform> faithfulDecks;

        private void Awake()
        {
            Instance = this;
        }
    }
}
