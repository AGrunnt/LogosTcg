using DG.Tweening;
using LogoTcg;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LogosTcg
{
    public class SlotDropHandler : MonoBehaviour, IDropHandler
    {
        public bool faceup = true;
        public bool active = true;
        public int maxChildrenCards = 1;


        // This will be called when something is dropped on this UI element
        public void OnDrop(PointerEventData eventData)
        {
            
            var dropped = eventData.pointerDrag;
            Gobject obj = dropped.GetComponent<Gobject>();

            if (dropped == null || !obj.draggable || transform.GetComponentsInChildren<Gobject>().Length >= maxChildrenCards || !active) return;


             // Reparent the card under this slot
            dropped.transform.SetParent(transform, worldPositionStays: false);

            // Zero out its local position so it sits perfectly in the slot
            var rt = dropped.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = Vector2.zero;
            else
                dropped.transform.localPosition = Vector3.zero;

        }
    }
}