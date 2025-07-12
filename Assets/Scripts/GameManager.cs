using UnityEngine;

namespace LogosTcg
{
    public class GameManager : MonoBehaviour
    {
        public bool slotChangeActionsActive = false;

        public static GameManager Instance;

        private void Awake()
        {
            Instance = this;
        }
    }
}
