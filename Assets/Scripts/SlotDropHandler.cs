using DG.Tweening;
using LogoTcg;
using System.Linq;
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


            Transform lastParent = dropped.transform.parent;
             // Reparent the card under this slot
            dropped.transform.SetParent(transform, worldPositionStays: false);

            // Zero out its local position so it sits perfectly in the slot
            var rt = dropped.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = Vector2.zero;
            else
                dropped.transform.localPosition = Vector3.zero;

            GetComponent<ParentActions>().SetLastChildActive();
            lastParent.GetComponent<ParentActions>().SetLastChildActive();

            Transform front = FindDescendantByName(dropped.transform, "Front");
            Transform back = FindDescendantByName(dropped.transform, "Back");

            if(faceup)
            {
                front.gameObject.SetActive(true);
                back.gameObject.SetActive(false);
            } else
            {
                front.gameObject.SetActive(false);
                back.gameObject.SetActive(true);
            }

        }

        public Transform FindDescendantByName(Transform tf, string childName)
        {
            // includes this.transform too; skip if you only want strict children
            return tf
                .GetComponentsInChildren<Transform>(includeInactive: true)
                .FirstOrDefault(t => t.name == childName);
        }
    }
}