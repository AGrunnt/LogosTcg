using DG.Tweening;
using LogoTcg;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LogosTcg
{
    public class SlotDropHandler : MonoBehaviour, IDropHandler
    {

        SlotScript slotScript;

        void Start()
        {
            slotScript = GetComponent<SlotScript>();
        }

        // This will be called when something is dropped on this UI element
        public void OnDrop(PointerEventData eventData)
        {
            var dropped = eventData.pointerDrag;
            Gobject obj = dropped.GetComponent<Gobject>();

            if (dropped == null || !obj.draggable || transform.GetComponentsInChildren<Gobject>().Length >= GetComponent<SlotScript>().maxChildrenCards || !GetComponent<SlotScript>().active) return;


            Transform lastParent = dropped.transform.parent;
             // Reparent the card under this slot
            dropped.transform.SetParent(transform, worldPositionStays: false);

            // Zero out its local position so it sits perfectly in the slot
            var rt = dropped.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = Vector2.zero;
            else
                dropped.transform.localPosition = Vector3.zero;

            slotScript.SetFacing(dropped.transform);

        }




    }
}