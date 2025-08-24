using UnityEngine;

namespace LogosTcg
{
    public class CoinCardHandler : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<SlotScript>().OnCardDropped.AddListener(CoinCardDropped);
            // or: slot.OnCardDropped.AddListener(tf => CoinCardDropped(tf));

        }

        public void CoinCardDropped(Transform tf)
        {
            
            //Debug.Log($"ran coin card dropped {tf.name}");
            //tf.GetComponent<CoinStack>().ToggleVisible();
        }
    }
}
