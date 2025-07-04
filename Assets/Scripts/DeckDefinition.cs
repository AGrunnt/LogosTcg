using System.Collections.Generic;
using UnityEngine;

namespace LogosTcg
{
    [CreateAssetMenu(menuName = "Logos/Deck Definition")]
    public class DeckDefinition : ScriptableObject
    {
        public Sprite CardBackArt;
        public List<CardDef> CardCollection;



        
    }
}
