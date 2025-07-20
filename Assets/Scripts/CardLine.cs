using TMPro;
using UnityEngine;

namespace LogosTcg
{
    public class CardLine : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI rarity;
        public CardDef cardDef;
        public string addressableKey;

        public void Apply()
        {
            title.text = cardDef.name;
            rarity.text = cardDef.Rarity;
        }

    }
}
