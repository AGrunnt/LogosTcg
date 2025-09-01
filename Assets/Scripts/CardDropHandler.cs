using DG.Tweening;
using LogoTcg;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LogosTcg
{
    public class CardDropHandler : MonoBehaviour, IDropHandler
    {
        Card card;
        GameNetworkManager gnm;
        void Start()
        {
            gnm = GameNetworkManager.Instance;
            card = GetComponent<Card>();
        }

        // This will be called when something is dropped on this UI element
        public void OnDrop(PointerEventData eventData)
        {
            var dropped = eventData.pointerDrag;

            if (dropped.tag != "Coin" || !transform.parent.GetComponent<SlotScript>().faceup) return;

            if (NetworkManager.Singleton != null)
            {
                gnm.CoinDropServerRpc(dropped.transform.parent.parent.name, card.name,dropped.GetComponent<Coin>().value);
            }
            else
            {
                int overkill = card.SetValue(dropped.GetComponent<Coin>().value);
                Card orgCard = dropped.transform.parent.parent.GetComponent<Card>();
                orgCard.SetValue(dropped.GetComponent<Coin>().value - overkill);
                Debug.Log($"orgCard Val {dropped.GetComponent<Coin>().value - overkill}");
                orgCard.GetComponent<CoinStack>().ReVisible();
            }


            /*
            int overkill = card.SetValue(val);
            Card orgCard = dropped.transform.parent.parent.GetComponent<Card>();
            orgCard.SetValue(dropped.GetComponent<Coin>().value - overkill);
            Debug.Log($"orgCard Val {dropped.GetComponent<Coin>().value - overkill}");
            orgCard.GetComponent<CoinStack>().ReVisible();
            */
        }



        
    }
}