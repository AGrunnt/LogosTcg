using TMPro;
using UnityEngine;

namespace LogosTcg
{
    public class Coin : MonoBehaviour
    {
        public int value = 5;
        [SerializeField] TextMeshProUGUI valueTmp;

        public void SetValueTmp(int val)
        {
            value = val;
            valueTmp.text = value.ToString();
        }

        private void Start()
        {
            //transform.localPosition = Vector3.zero;
        }

    }
}
