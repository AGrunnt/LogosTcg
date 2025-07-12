using DG.Tweening.Core.Easing;
using UnityEngine;

namespace LogosTcg
{
    public class State : MonoBehaviour
    {
        public static State Instance;

        public bool globalDragging = false;
        

        private void Awake()
        {
            Instance = this;
        }
    }
}
