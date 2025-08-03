using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace LogosTcg
{
    public class GameManager : MonoBehaviour
    {
        public bool setUpFinished = false;
        public static GameManager Instance;


        private void Awake()
        {
            Instance = this;
        }
    }
}
