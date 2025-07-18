
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
    }

}