using DG.Tweening;
using LogoTcg;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LogosTcg
{
    public class SlotDropHandler : MonoBehaviour//, IDropHandler
    {
        /*delscript
        SlotScript slotScript;
        GridSlotActions slotActions;

        void Start()
        {
            slotScript = GetComponent<SlotScript>();
            slotActions = GetComponent<GridSlotActions>();
        }

        // This will be called when something is dropped on this UI element
        public void OnDrop(PointerEventData eventData)
        {

            var dropped = eventData.pointerDrag;
            Gobject obj = dropped.GetComponent<Gobject>();
            Transform oldParent = dropped.transform.parent;
            Transform newParent = transform;

            if (dropped.GetComponent<Card>() == null || dropped == null || !obj.draggable || transform.GetComponentsInChildren<Gobject>().Length >= GetComponent<SlotScript>().maxChildrenCards || !GetComponent<SlotScript>().active) return;

            if (NetworkManager.Singleton == null)
            {
                dropped.transform.SetParent(newParent, worldPositionStays: true); //worldPositStay throws the card around
            }
            else
                GetComponent<SlotNetwork>().SetCardParentNetwork(dropped.transform, newParent);

            GridSlotActions oldSlotActs = oldParent.GetComponent<GridSlotActions>();
            if(oldSlotActs != null)
                oldParent.GetComponent<GridSlotActions>().SlotLosing();
            
            if(slotActions != null)
                slotActions.SlotGaining(true);

            oldParent.GetComponent<SlotScript>().SetLastCardSettings();
            GetComponent<SlotScript>().SetLastCardSettings();
        }
        */
    }
}