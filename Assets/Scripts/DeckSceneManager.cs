using LogoTcg;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LogosTcg
{
    public class DeckSceneManager : MonoBehaviour
    {

        public EventSystem eventSystem;
        public GraphicRaycaster raycaster;
        public static DeckSceneManager instance;

        List<CardDef> locCardDefs;
        List<CardDef> EncounterCardDefs;
        List<CardDef> FaithfulCardDefs;

        public Transform faithfulListTf;
        public Transform encounterListTf;
        public Transform locationListTf;

        public GameObject cardLinePrefab;

        void Start()
        {
            instance = this;
        }

        public void AddToList(GameObject gridCardObj)
        {
            CardDef cardDef = gridCardObj.GetComponent<Card>()._definition;
            GameObject cardLineObj;
            if(cardDef.Type.Contains("Faithful"))
            {
                cardLineObj = Instantiate(cardLinePrefab, faithfulListTf);
            } else if (cardDef.Type.Contains("Location"))
            {
                cardLineObj = Instantiate(cardLinePrefab, locationListTf);
            } else
                cardLineObj = Instantiate(cardLinePrefab, encounterListTf);

            cardLineObj.GetComponent<CardLine>().cardDef = cardDef;
            Destroy(gridCardObj);
        }

        public void RemoveFromList(GameObject cardLineObj)
        {
            CardDef cardDef = cardLineObj.GetComponent<CardLine>().cardDef;
            GameObject gridCardObj;
            gridCardObj= Instantiate(cardLinePrefab, faithfulListTf);

            gridCardObj.GetComponent<CardLine>().cardDef = cardDef;
            Destroy(cardLineObj);
        }


    }
}

