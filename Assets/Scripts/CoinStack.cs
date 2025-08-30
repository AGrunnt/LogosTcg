using LogoTcg;
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
        [SerializeField] private Transform coinSlotAll;
        [SerializeField] private Transform coin1;
        [SerializeField] private Transform coin5;
        [SerializeField] private Transform coin10;
        [SerializeField] private Transform coinAll;
        [SerializeField] private GameObject coinVis1;
        [SerializeField] private GameObject coinVis5;
        [SerializeField] private GameObject coinVis10;
        [SerializeField] private GameObject coinVisAll;
        [SerializeField] Card card;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            card = GetComponent<Card>();
            //Need to pause for layout groups to have a second to adjust
            //StartCoroutine(SetUpCoinStacks());            
        }

        [ContextMenu("Run/SetUp")]
        public void SetUp()
        {
            StartCoroutine(DelayedSetUp());
        }

        IEnumerator DelayedSetUp()
        {
            yield return new WaitForEndOfFrame();
            
            coinVis1 = coin1.GetComponent<Gobject>().gobjectVisual.gameObject;
            coinVis5 = coin5.GetComponent<Gobject>().gobjectVisual.gameObject;
            coinVis10 = coin10.GetComponent<Gobject>().gobjectVisual.gameObject;
            coinVisAll = coinAll.GetComponent<Gobject>().gobjectVisual.gameObject;

            coinVis1.SetActive(false);
            coinVis5.SetActive(false);
            coinVis10.SetActive(false);
            coinVisAll.SetActive(false);

            coin1.gameObject.SetActive(false);
            coin5.gameObject.SetActive(false);
            coin10.gameObject.SetActive(false);
            coinAll.gameObject.SetActive(false);
        }

        [ContextMenu("Run/Visible")]
        public void ToggleVisible()
        {
            if(visible)
            {
                Debug.Log("make false");
                visible = false;
                coin1.gameObject.SetActive(false);
                coin5.gameObject.SetActive(false);
                coin10.gameObject.SetActive(false);
                coinAll.gameObject.SetActive(false);
                coinVis1.gameObject.SetActive(false);
                coinVis5.gameObject.SetActive(false);
                coinVis10.gameObject.SetActive(false);
                coinVisAll.gameObject.SetActive(false);
            }
            else
            {
                visible = true;
                if (card.currValue <= 0) return;
                coinAll.gameObject.SetActive(true);
                coinVisAll.gameObject.SetActive(true);
                coinAll.GetComponent<Coin>().SetValueTmp(card.currValue);

                coin1.gameObject.SetActive(true);
                coinVis1.gameObject.SetActive(true);
                if (card.currValue <= 4) return;
                coin5.gameObject.SetActive(true);
                coinVis5.gameObject.SetActive(true);
                if (card.currValue <= 9) return;
                coin10.gameObject.SetActive(true);
                coinVis10.gameObject.SetActive(true);
            }
        }


        public void ReVisible()
        {
            if (!visible) return;
                
            coin1.gameObject.SetActive(false);
            coin5.gameObject.SetActive(false);
            coin10.gameObject.SetActive(false);
            coinAll.gameObject.SetActive(false);

            coinVis1.gameObject.SetActive(false);
            coinVis5.gameObject.SetActive(false);
            coinVis10.gameObject.SetActive(false);
            coinVisAll.gameObject.SetActive(false);

            if (card.currValue < 1) return;
            coinAll.gameObject.SetActive(true);
            coinVisAll.gameObject.SetActive(true);
            Debug.Log($"curr val faithful card {card.currValue}");
            coinAll.GetComponent<Coin>().SetValueTmp(card.currValue);

            coin1.gameObject.SetActive(true);
            coinVis1.gameObject.SetActive(true);
            if (card.currValue < 5) return;
            coin5.gameObject.SetActive(true);
            coinVis5.gameObject.SetActive(true);
            if (card.currValue < 10) return;
            coin10.gameObject.SetActive(true);
            coinVis10.gameObject.SetActive(true);
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
