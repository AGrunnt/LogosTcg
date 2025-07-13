using LogoTcg;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LogosTcg
{
    public class GridSlotActions : MonoBehaviour
    {

        [SerializeField] private Transform pullVertTransform;
        [SerializeField] private Transform pushVertTransform;
        [SerializeField] private Transform pullHorzTransform;
        [SerializeField] private Transform pushHorzTransform;
        [SerializeField] public bool isLocSlot = false;
        private bool pauseVert = false;

        void Start()
        {
            GetComponent<SlotScript>().SlotChg.AddListener(pullOnVertEmpty);
            GetComponent<SlotScript>().SlotChg.AddListener(pushVert);
            GetComponent<SlotScript>().SlotChg.AddListener(pullOnHorzEmpty);
            GetComponent<SlotScript>().SlotChg.AddListener(pushHorz);
        }

        public void pullOnVertEmpty(SlotScript slot)
        {
            //if (!pullOnEmptyBool) return;

            if(pauseVert == false && isLocSlot == false && pullVertTransform != null && transform.GetComponentsInChildren<Card>().Count() == 0 && pullVertTransform.GetComponentsInChildren<Card>().Count() != 0)
            {
                var card = pullVertTransform.GetComponentsInChildren<Card>()[0];
                card.GetComponent<Gobject>().runOnline = false;
                card.transform.SetParent(this.transform, false);
            }
        }

        public void pushVert(SlotScript slot)
        {
            if(pauseVert == false && isLocSlot == false && pushVertTransform != null && pushVertTransform.GetComponentsInChildren<Card>().Count() == 0 && transform.GetComponentsInChildren<Card>().Count() != 0)
            {
                var card = this.GetComponentsInChildren<Card>()[0];
                card.GetComponent<Gobject>().runOnline = false;
                card.transform.SetParent(pushVertTransform, false);
            }
        }

        public void pullOnHorzEmpty(SlotScript slot)
        {
            if (isLocSlot == true && pullHorzTransform != null && transform.GetComponentsInChildren<Card>().Count() == 0 && pullHorzTransform.GetComponentsInChildren<Card>().Count() != 0)
            {
                foreach(GridSlotActions slotAction in transform.parent.parent.GetComponentsInChildren<GridSlotActions>())
                {
                    slotAction.pauseVert = true;
                }

                int colIndex = transform.parent.GetSiblingIndex();
                Transform gridTf = transform.parent.parent;
                for (int i = colIndex; i < 5; i++)
                {
                    foreach(GridSlotActions slotAction in gridTf.GetChild(i).GetComponentsInChildren<GridSlotActions>())
                    {
                        if (slotAction.pullHorzTransform.GetComponentsInChildren<Card>().Count() == 0) continue;

                        var card = slotAction.pullHorzTransform.GetComponentsInChildren<Card>()[0];
                        card.GetComponent<Gobject>().runOnline = false;
                        // reparent back to this slot’s transform
                        card.transform.SetParent(slotAction.transform, false);
                    }
                }


                foreach (GridSlotActions slotAction in transform.parent.parent.GetComponentsInChildren<GridSlotActions>())
                {
                    slotAction.pauseVert = false;
                }
            }
        }

        public void pushHorz(SlotScript slot)
        {
            if (isLocSlot == true && pushHorzTransform != null && pushHorzTransform.GetComponentsInChildren<Card>().Count() == 0 && transform.GetComponentsInChildren<Card>().Count() != 0)
            {
                //pauseVert = true;
                //pushHorzTransform.GetComponent<GridSlotActions>().pauseVert = true;
                
                    var card = this.GetComponentsInChildren<Card>()[0];
                    card.GetComponent<Gobject>().runOnline = false;
                    card.transform.SetParent(pushHorzTransform, false);
                
                /*
                // Loop from last ? first
                for (int i = slotActs.Length - 1; i >= 0; i--)
                {
                    var slotAct = slotActs[i];
                    if (slotAct.GetComponentInChildren<Card>() == null)
                        continue;

                    var card = slotAct.GetComponentsInChildren<Card>()[0];
                    card.GetComponent<Gobject>().runOnline = false;
                    card.transform.SetParent(slotAct.pushHorzTransform, false);
                }
                */
                //pauseVert = false;
                //pushHorzTransform.GetComponent<GridSlotActions>().pauseVert = false;

            }
        }

    }
}
