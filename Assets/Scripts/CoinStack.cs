using System.Collections;
using System.Drawing;
using UnityEngine;

namespace LogosTcg
{
    public class CoinStack : MonoBehaviour
    {
        [SerializeField] private GameObject coinPrefab;
        //[SerializeField] private int value = 1;
        [SerializeField] private bool visible = false;
        [SerializeField] private Transform coinSlot1;
        [SerializeField] private Transform coinSlot5;
        [SerializeField] private Transform coinSlot10;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //Need to pause for layout groups to have a second to adjust
            //StartCoroutine(SetUpCoinStacks());
        }

        [ContextMenu("Run/Visible")]
        public void ToggleVisible()
        {
            if(visible)
            {
                visible = false;
                coinSlot1.gameObject.SetActive(false);
                coinSlot5.gameObject.SetActive(false);
                coinSlot10.gameObject.SetActive(false);
            }
            else
            {
                visible = true;
                coinSlot1.gameObject.SetActive(true);
                coinSlot5.gameObject.SetActive(true);
                coinSlot10.gameObject.SetActive(true);
            }
        }


        [ContextMenu("Run/Draw One Card")]
        public void RunSetUp()
        {
            StartCoroutine(SetUpCoinStacks());
        }


        
        public IEnumerator SetUpCoinStacks()
        {
            yield return new WaitForSeconds(0.5f);

            var coin1 = Instantiate(coinPrefab, coinSlot1, false);
            var coin2 = Instantiate(coinPrefab, coinSlot1, false);
            coin1.GetComponent<Coin>().SetValueTmp(1);
            coin2.GetComponent<Coin>().SetValueTmp(1);

            var coin3 = Instantiate(coinPrefab, coinSlot5, false);
            var coin4 = Instantiate(coinPrefab, coinSlot5, false);
            coin3.GetComponent<Coin>().SetValueTmp(5);
            coin4.GetComponent<Coin>().SetValueTmp(5);

            var coin5 = Instantiate(coinPrefab, coinSlot10, false);
            var coin6 = Instantiate(coinPrefab, coinSlot10, false);
            coin5.GetComponent<Coin>().SetValueTmp(10);
            coin6.GetComponent<Coin>().SetValueTmp(10);
        }

        void OnTransformChildrenChanged()
        {
            /*
            if (transform.childCount < 2 && visible)
            {
                var coin = Instantiate(coinPrefab, transform, false);
                coin.GetComponent<Coin>().value = value;
                coin.transform.localPosition = Vector3.zero;
            }
            if (transform.childCount < 2 && visible)
            {
                var coin = Instantiate(coinPrefab, transform, false);
                coin.GetComponent<Coin>().value = value;
                coin.transform.localPosition = Vector3.zero;
            }
            if (transform.childCount < 2 && visible)
            {
                var coin = Instantiate(coinPrefab, transform, false);
                coin.GetComponent<Coin>().value = value;
                coin.transform.localPosition = Vector3.zero;
            }
            */
        }
    }
}
