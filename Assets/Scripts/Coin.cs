using LogoTcg;
using TMPro;
using UnityEngine;

namespace LogosTcg
{
    public class Coin : MonoBehaviour
    {
        public int value = 5;
        [SerializeField] TextMeshProUGUI valueTmp;

        /*
        public void SetUp()
        {
            Debug.Log("coin setup");
            //this.transform.parent.parent.GetComponent<CoinStack>().SetUp();
            //this.transform.parent.parent.GetComponent<CoinStack>().SetUp();
            GetComponent<Gobject>().gobjectVisual.gameObject.SetActive(false);
            
        }
        */

        public void SetValueTmp(int val)
        {
            Debug.Log($"setvaltmp {val}");
            value = val;
            valueTmp.text = value.ToString();
        }

        private void Start()
        {
            //transform.localPosition = Vector3.zero;
        }

    }
}
