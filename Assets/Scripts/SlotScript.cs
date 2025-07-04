using LogoTcg;
using System.Linq;
using UnityEngine;

namespace LogosTcg
{
    public class SlotScript : MonoBehaviour
    {
        public bool faceup = true;
        public bool active = true;
        public int maxChildrenCards = 1;

        void OnTransformChildrenChanged()
        {
            SetLastCardSettings();
        }
        //SetFacing

        public void SetLastCardSettings()
        {
            Gobject lastObj = GetLastDirectChildGobjLinq(transform);
            if (lastObj == null) return;

            lastObj.GetComponentInChildren<Canvas>().sortingOrder = lastObj.transform.GetSiblingIndex();

            if (active)
            {
                lastObj.draggable = true;
                lastObj.hoverable = true;
                lastObj.selectable = true;
            }

            if(faceup)
                SetFacing(lastObj.transform);
        }

        public void InitializeSlots()
        {
            foreach (Gobject childCard in transform.GetComponentsInChildren<Gobject>())
            {
                childCard.draggable = false;
                childCard.hoverable = false;
                childCard.selectable = false;
                SetFacing(childCard.transform);
                childCard.GetComponentInChildren<Canvas>().sortingOrder = childCard.transform.GetSiblingIndex();
            }

            if (active) SetLastCardSettings();

        }

        Gobject GetLastDirectChildGobjLinq(Transform parent)
        {
            return parent
                .Cast<Transform>()                  // IEnumerable<Transform> of direct children
                .Select(t => t.GetComponent<Gobject>())// pull Card or null
                .Where(c => c != null)              // filter out non?Cards
                .LastOrDefault();                   // take the last one (or null)
        }

        public void SetFacing(Transform tf)
        {
            Transform front = FindDescendantByName(tf, "Front");
            Transform back = FindDescendantByName(tf, "Back");

            if (faceup)
            {
                front.gameObject.SetActive(true);
                back.gameObject.SetActive(false);
            }
            else
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
