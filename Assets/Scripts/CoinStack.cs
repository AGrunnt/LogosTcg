using System.Collections;
using System.Drawing;
using UnityEngine;

namespace LogosTcg
{
    public class CoinStack : MonoBehaviour
    {
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private int value;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //Need to pause for layout groups to have a second to adjust
            StartCoroutine(SetUpCoinStacks());
        }

        private IEnumerator SetUpCoinStacks()
        {
            yield return new WaitForSeconds(0.5f);

            var coin1 = Instantiate(coinPrefab, transform, false);
            var coin2 = Instantiate(coinPrefab, transform, false);
            coin1.GetComponent<Coin>().value = value;
            coin2.GetComponent<Coin>().value = value;
        }

        private void OnTransformChildrenChanged()
        {
            if (transform.childCount < 3)
            {
                var coin = Instantiate(coinPrefab, transform, false);
                coin.GetComponent<Coin>().value = value;
                coin.transform.localPosition = Vector3.zero;
            }
        }
    }
}
