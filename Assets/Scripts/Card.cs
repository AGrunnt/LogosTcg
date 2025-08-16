
using LogoTcg;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
        public Image IconImg;
        public Image IconSqrBckImg;
        public Image IconDimBckImg;
        public Image ValBckImg;
        public TextMeshProUGUI valTmp;
        public TextMeshProUGUI abilityTmp;
        public Image setImg;
        public TextMeshProUGUI cardId;
        public Image backgroundImg;
        public Image rarityImg;
        ImageCollection ic;

        void Start()
        {
            ic = ImageCollection.instance;
        }

        public void Apply(CardDef data)
        {
            gameObject.name = data.name;
            _definition = data;
            image.sprite = data.Artwork;
            cardId.text = data.Id;
            valTmp.text = data.Value.ToString();

            switch(data.Rarity) //do same for set
            {
                case "Common":
                    rarityImg.sprite = ic.rarityCommon;
                    break;
                case "Uncommon":
                    rarityImg.sprite = ic.rarityUncommon;
                    break;
                case "Rare":
                    rarityImg.sprite = ic.rarityRare;
                    break;
            }

            if (data.Type[0] == "Location")
            {
                switch (data.Tag[0])
                {
                    case "Leaf":
                        IconSqrBckImg.sprite = ic.boxBckLocLeaf;
                        IconDimBckImg.sprite = ic.dimBckLocLeaf;
                        IconImg.sprite = ic.IconLeaf;
                        break;
                    case "Mountain":
                        IconSqrBckImg.sprite = ic.boxBckLocMnt;
                        IconDimBckImg.sprite = ic.dimBckLocMnt;
                        IconImg.sprite = ic.IconMnt;
                        break;
                    case "Wave":
                        IconSqrBckImg.sprite = ic.boxBckLocWave;
                        IconDimBckImg.sprite = ic.dimBckLocWave;
                        IconImg.sprite = ic.IconWave;
                        break;
                }
                //set, 
            }
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