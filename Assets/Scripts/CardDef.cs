using LogosTcg;
using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UI;

namespace LogosTcg
{

    /// <summary>
    /// Serializable wrapper for a list of strings to allow nested lists in the Unity Inspector.
    /// </summary>
    [Serializable]
    public class StringListWrapper
    {
        [Tooltip("A single group of suits or categories.")]
        public List<string> innerList = new List<string>();
    }

    /// <summary>
    /// Represents a single scoring ability with explicit parameters for each ability type.
    /// </summary>
    [Serializable]
    public class Ability
    {
        

        [Tooltip("Type of scoring ability.")]
        public List<string> AbilityType; // e.g., FOREACH, WITH, etc.

        public List<string> Target;

        public List<string> Tag;


    }

  

    /// <summary>
    /// ScriptableObject definition for a Realm card with multiple suits and flexible abilities.
    /// </summary>
    [CreateAssetMenu(menuName = "Logos/CardDef")]
    public class CardDef : ScriptableObject
    {
        [Tooltip("Unique identifier for this card")]
        public string Id;
        public Sprite Artwork;

        [Tooltip("Display title of the card")]
        public string Title;
        public string Rarity;
        public List<string> Type;
        public int Value;
        public string AbilityText;
        public string Verse;
        public string VerseText;
        public string AbilityType;

        [Header("Scoring Abilities")]
        [Tooltip("All abilities this card grants or triggers at end-game scoring")]
        public List<Ability> Abilities = new List<Ability>();
    }
}
