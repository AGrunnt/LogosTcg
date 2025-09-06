using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace LogosTcg
{
    public class GameManager : MonoBehaviour
    {
        public bool setUpFinished = false;
        public static GameManager Instance;
        public int playerCount;
        public List<string> inString;
        public List<string> GameplayStatuses;
        public int coinModifier = 0;

        public void AddString(string str)
        {
            inString.Add(str);
        }
        public void RmString(string str)
        {
            inString.Remove(str);
        }

        private void Awake()
        {
            Instance = this;
            playerCount = StaticData.playerNums;
        }
    }
}
