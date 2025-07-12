using DG.Tweening;
using LogoTcg;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LogosTcg
{
    public class CardDropHandler : MonoBehaviour, IDropHandler
    {

        // This will be called when something is dropped on this UI element
        public void OnDrop(PointerEventData eventData)
        {
            var dropped = eventData.pointerDrag;

            if (dropped.tag != "Coin") return;

            dropped.transform.SetParent(transform, worldPositionStays: true); //worldPositStay throws the card around
        }
    }
}