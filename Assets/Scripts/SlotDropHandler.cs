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

            if (dropped.GetComponent<Card>() == null || dropped == null || !obj.draggable || transform.GetComponentsInChildren<Gobject>().Length >= GetComponent<SlotScript>().maxChildrenCards || !GetComponent<SlotScript>().active) return;


            Transform lastParent = dropped.transform.parent;
             // Reparent the card under this slot
            dropped.transform.SetParent(transform, worldPositionStays: true); //worldPositStay throws the card around

            slotScript.SetFacing(dropped.transform);

        }
    }
}