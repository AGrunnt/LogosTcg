
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
        public string addressableKey;

        public void Apply(CardDef data)
        {
            gameObject.name = data.name;
            _definition = data;
            image.sprite = data.Artwork;
        }

        public void SetFacing(bool faceup)
        {
            Transform front = FindDescendantByName(GetComponent<Gobject>().gobjectVisual.transform, "Front");
            Transform back = FindDescendantByName(GetComponent<Gobject>().gobjectVisual.transform, "Back");

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