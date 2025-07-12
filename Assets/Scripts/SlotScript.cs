using LogoTcg;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace LogosTcg
{
    public class SlotScript : MonoBehaviour
    {
        public bool faceup = true;
        public bool active = true;
        public bool onlyTop = true;
        public int maxChildrenCards = 1;
        
        public string slotType;

        [HideInInspector] public UnityEvent<SlotScript> SlotChg;

        void OnTransformChildrenChanged()
        {
            if(GameManager.Instance.slotChangeActionsActive)
                SlotChg.Invoke(this);
            
            InitializeSlots();
        }
        //SetFacing

        public void SetLastCardSettings()
        {
            Gobject lastObj = GetLastDirectChildGobjLinq(transform);
            if (lastObj == null) return;

            lastObj.gobjectVisual.GetComponent<Canvas>().sortingOrder = lastObj.transform.GetSiblingIndex();

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
            foreach (Card card in transform.GetComponentsInChildren<Card>())
            {
                Gobject childCard = card.GetComponent<Gobject>();
                if (onlyTop)
                {
                    childCard.draggable = false;
                    childCard.hoverable = false;
                    childCard.selectable = false;
                } else
                {
                    childCard.draggable = true;
                    childCard.hoverable = true;
                    childCard.selectable = true;
                }
                SetFacing(childCard.transform);
                childCard.gobjectVisual.GetComponentInChildren<Canvas>().sortingOrder = childCard.transform.GetSiblingIndex();
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
            Transform front = FindDescendantByName(tf.GetComponent<Gobject>().gobjectVisual.transform, "Front");
            Transform back = FindDescendantByName(tf.GetComponent<Gobject>().gobjectVisual.transform, "Back");

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
