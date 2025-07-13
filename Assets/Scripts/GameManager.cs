using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace LogosTcg
{
    public class GameManager : MonoBehaviour
    {
        public bool slotChangeActionsActive = false;
        public List<Transform> columns;
        public static GameManager Instance;

        private void Awake()
        {
            Instance = this;
        }
    }
}
