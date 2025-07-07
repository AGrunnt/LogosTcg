using DG.Tweening;
using LogoTcg;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace LogosTcg
{
    public class SlotNetwork : NetworkBehaviour
    {

        public List<Gobject> gobjects = new List<Gobject>();

        private void Start()
        {
            GetComponent<SlotScript>().SlotChg.AddListener(ChildChg);
        }

        public void ChildChg(SlotScript slot)
        {
            List<Gobject> prevGobjects = gobjects;
            gobjects = transform.GetComponentsInChildren<Gobject>().ToList<Gobject>();

            if (gobjects == null) return;

            List<Gobject> newObjs = gobjects
                .Except(prevGobjects)
                .ToList();

            if(newObjs == null) return;

            foreach(Gobject obj in newObjs)
            {
                MountServerRpc(obj.gameObject.name, obj.transform.parent.name);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void MountServerRpc(string cardName, string newParent)
        {

            MountClientRpc(cardName, newParent);
        }

        [ClientRpc]
        public void MountClientRpc(string cardName, string newParent)
        {
            Transform card = GameObject.Find(cardName).transform;
            Transform slot = GameObject.Find(newParent).transform;

            card.SetParent(slot, worldPositionStays: true);

            if (slot.GetComponent<HorizontalLayoutGroup>() == null && slot.GetComponent<GridLayoutGroup>() == null)
                card.DOLocalMove(Vector3.zero, .15f).SetEase(Ease.OutBack);
        }


    }
}
