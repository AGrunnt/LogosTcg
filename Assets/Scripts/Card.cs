
using LogoTcg;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LogosTcg
{

    /// <summary>
    /// Component that applies a RealmsCardDef to a visible card in-game.
    /// </summary>
    public class Card : MonoBehaviour
    {
  
        public CardDef _definition;
        public Image image;    //me was Image
        public Image BackImage;
        public List<Gobject> gobjects = new List<Gobject>();

        public void Apply(CardDef data)
        {
            gameObject.name = data.name;
            _definition = data;
            image.sprite = data.Artwork;
        }

        void OnTransformChildrenChanged()
        {
            if (GameManager.Instance.slotChangeActionsActive && NetworkManager.Singleton != null)
                ChildChgNetwork();
        }

        public void ChildChgNetwork()
        {
            List<Gobject> prevGobjects = gobjects;
            gobjects = transform.GetComponentsInChildren<Gobject>().ToList<Gobject>();

            if (gobjects == null) return;

            List<Gobject> newObjs = gobjects
                .Except(prevGobjects)
                .ToList();

            if (newObjs == null) return;

            foreach (Gobject obj in newObjs)
            {
                GameNetworkManager.Instance.MountServerRpc(obj.gameObject.name, obj.transform.parent.name);
            }
        }
    }

}