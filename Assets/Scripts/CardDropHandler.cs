using DG.Tweening;
using LogoTcg;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LogosTcg
{
    public class CardDropHandler : MonoBehaviour, IDropHandler
    {
        Card card;
        void Start()
        {
            card = GetComponent<Card>();
        }

        // This will be called when something is dropped on this UI element
        public void OnDrop(PointerEventData eventData)
        {
            var dropped = eventData.pointerDrag;

            if (dropped.tag != "Coin" || !transform.parent.GetComponent<SlotScript>().faceup) return;

            int overkill = card.SetValue(dropped.GetComponent<Coin>().value);
            Card orgCard = dropped.transform.parent.parent.GetComponent<Card>();
            orgCard.SetValue(dropped.GetComponent<Coin>().value - overkill);
            Debug.Log($"orgCard Val {dropped.GetComponent<Coin>().value - overkill}");
            orgCard.GetComponent<CoinStack>().ReVisible();

            
        }
    }
}