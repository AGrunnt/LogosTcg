using UnityEngine;
using LogosTcg;

namespace LogosTcg
{
    public class DeckSceneManager : MonoBehaviour
    {
        public static DeckSceneManager instance;
        void Awake() => instance = this;

        [Header("hook these up in Inspector")]
        public Transform faithfulListTf;
        public Transform encounterListTf;
        public Transform locationListTf;
        public GameObject cardLinePrefab;

        // ---------------------------------------------------
        // Move a grid?card into one of the three lists:
        public void AddToList(GameObject gridCardObj)
        {
            var c = gridCardObj.GetComponent<Card>();
            var cd = c._definition;
            var key = c.addressableKey;

            // 1) mark it assigned so loader never spawns it
            LoadingCards.instance.listAssigned.Add(key);

            // 2) spawn a CardLine in the right list
            var parent =
                cd.Type.Contains("Faithful") ? faithfulListTf :
                cd.Type.Contains("Location") ? locationListTf :
                                               encounterListTf;
            var line = Instantiate(cardLinePrefab, parent)
                       .GetComponent<CardLine>();

            line.cardDef = cd;
            line.Apply();
            line.addressableKey = key;

            // 3) destroy the grid object + clear our internal map
            Destroy(gridCardObj);
            LoadingCards.instance.RemoveGridCardMapping(key);
        }

        // ---------------------------------------------------
        // Take a CardLine out of a list & pop it back into the grid:
        public void RemoveFromList(GameObject cardLineObj)
        {
            var line = cardLineObj.GetComponent<CardLine>();
            var cd = line.cardDef;
            var key = line.addressableKey;

            // 1) un?mark so loader can spawn it again
            LoadingCards.instance.listAssigned.Remove(key);

            // 2) destroy the line.
            Destroy(cardLineObj);

            // 3) immediately respawn into grid (if it still matches filters)
            LoadingCards.instance.AddCardToGrid(key, cd);
        }
    }
}
