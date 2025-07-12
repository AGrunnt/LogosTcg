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
            if(NetworkManager.Singleton != null)
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
                GameNetworkManager.Instance.MountServerRpc(obj.gameObject.name, obj.transform.parent.name);
            }
        }

    }
}
