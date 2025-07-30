using LogoTcg;
using UnityEngine;

namespace LogosTcg
{
    public class CardInstantiator : MonoBehaviour
    {

        public static CardInstantiator instance;
        void Awake() => instance = this;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }


    }
}
