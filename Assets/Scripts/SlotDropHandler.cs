using UnityEngine;
using UnityEngine.EventSystems;

namespace LogosTcg
{
    public class SlotDropHandler : MonoBehaviour, IDropHandler, IPointerEnterHandler
    {

        
    public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("Pointer entered slot!");
        }


        // This will be called when something is dropped on this UI element
        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("ran");
            var dropped = eventData.pointerDrag;
            if (dropped == null) return;

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