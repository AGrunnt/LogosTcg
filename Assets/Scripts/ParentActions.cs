using LogoTcg;
using System.Linq;
using UnityEngine;

namespace LogosTcg
{
    public class ParentActions : MonoBehaviour
    {
        void OnTransformChildrenChanged()
        {

            SetLastChildActive();
        }

        public void SetLastChildActive()
        {
            foreach (Gobject childCard in transform.GetComponentsInChildren<Gobject>())
                {
                    childCard.draggable = false;
                    childCard.hoverable = false;
                    childCard.selectable = false;
                }

            //Gobject lastObj = transform.GetChild(transform.childCount - 1).GetComponent<Gobject>();
            Gobject lastObj = GetLastDirectChildGobjLinq(transform);

            if (lastObj == null) return;

            lastObj.draggable = true;
            lastObj.hoverable = true;
            lastObj.selectable = true;
        }

        Gobject GetLastDirectChildGobjLinq(Transform parent)
        {
            return parent
                .Cast<Transform>()                  // IEnumerable<Transform> of direct children
                .Select(t => t.GetComponent<Gobject>())// pull Card or null
                .Where(c => c != null)              // filter out non?Cards
                .LastOrDefault();                   // take the last one (or null)
        }

    }
}
