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
        [SerializeField] private Transform rightLocTf;

        private void OnTransformChildrenChanged()
        {
            //if (GetComponentsInChildren<Card>().Count() == 0 && rightLocTf != null && rightLocTf.GetComponentsInChildren<Card>().Count() != 0)
               // shiftLeft();
        }

        public void shiftLeft()
        {
            /*
            int colIndex = transform.parent.GetSiblingIndex();
            Transform gridTf = transform.parent.parent;
            
                var locSlotScripts = gridTf
                    .GetChild(colIndex)
                    .GetComponentsInChildren<SlotScript>()
                    .Where(ss => ss.slotType == "LocSlot");
                var vertSlotScripts = gridTf
                    .GetChild(colIndex)
                    .GetComponentsInChildren<SlotScript>()
                    .Where(ss => ss.slotType == "VertSlot");
                var rightLocSlotScripts = gridTf
                    .GetChild(colIndex + 1)
                    .GetComponentsInChildren<SlotScript>()
                    .Where(ss => ss.slotType == "LocSlot");
                var rightVertSlotScripts = gridTf
                    .GetChild(colIndex + 1)
                    .GetComponentsInChildren<SlotScript>()
                    .Where(ss => ss.slotType == "VertSlot");

                if (rightLocSlotScripts.First().GetComponentInChildren<Card>() == null) return;

                rightLocSlotScripts.First().GetComponentInChildren<Card>().transform.SetParent(locSlotScripts.First().transform, false);
                locSlotScripts.First().GetComponent<SlotScript>().SetLastCardSettings();
                rightLocSlotScripts.First().GetComponent<SlotScript>().SetLastCardSettings();

                if (rightVertSlotScripts.First().GetComponentInChildren<Card>() == null) return;

                foreach (var card in rightVertSlotScripts.First().GetComponentsInChildren<Card>())
                {
                    card.transform.SetParent(vertSlotScripts.First().transform, false);
                }
                vertSlotScripts.First().GetComponent<SlotScript>().SetLastCardSettings();
                rightVertSlotScripts.First().GetComponent<SlotScript>().SetLastCardSettings();
            */
            
            int colIndex = transform.parent.GetSiblingIndex();
            Transform gridTf = transform.parent.parent;
            for (int i = colIndex; i < 4; i++)
            {
                var locSlotScripts = gridTf
                    .GetChild(i)
                    .GetComponentsInChildren<SlotScript>()
                    .Where(ss => ss.slotType == "LocSlot");
                var vertSlotScripts = gridTf
                    .GetChild(i)
                    .GetComponentsInChildren<SlotScript>()
                    .Where(ss => ss.slotType == "VertSlot");
                var rightLocSlotScripts = gridTf
                    .GetChild(i+1)
                    .GetComponentsInChildren<SlotScript>()
                    .Where(ss => ss.slotType == "LocSlot");
                var rightVertSlotScripts = gridTf
                    .GetChild(i+1)
                    .GetComponentsInChildren<SlotScript>()
                    .Where(ss => ss.slotType == "VertSlot");

                if (rightLocSlotScripts.First().GetComponentInChildren<Card>() == null) return;

                rightLocSlotScripts.First().GetComponentInChildren<Card>().transform.SetParent(locSlotScripts.First().transform, false);
                locSlotScripts.First().GetComponent<SlotScript>().SetLastCardSettings();
                rightLocSlotScripts.First().GetComponent<SlotScript>().SetLastCardSettings();

                if (rightVertSlotScripts.First().GetComponentInChildren<Card>() == null) continue;

                foreach(var card in rightVertSlotScripts.First().GetComponentsInChildren<Card>())
                {
                    card.transform.SetParent(vertSlotScripts.First().transform, false);
                }
                vertSlotScripts.First().GetComponent<SlotScript>().SetLastCardSettings();
                rightVertSlotScripts.First().GetComponent<SlotScript>().SetLastCardSettings();
            }
        }
    }
}
