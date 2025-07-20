using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LogosTcg
{
    public class CardTogAttachment : MonoBehaviour, IPointerClickHandler
    {
        public bool inList = false;

        public void OnPointerClick(PointerEventData eventData)
        {
            GameObject obj = null;

            if (eventData.pointerCurrentRaycast.gameObject.GetComponent<Card>() != null)
                obj = eventData.pointerCurrentRaycast.gameObject;
            else if (eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<CardLine>() != null)
                obj = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<CardLine>().gameObject;


            // This fires for any click that hits a Graphic under your raycaster
            Debug.Log("UI element clicked: " + eventData.pointerCurrentRaycast.gameObject.name);
            Debug.Log("UI element clicked: " + obj.name);
            if (obj.GetComponent<CardLine>() != null)
            {
                DeckSceneManager.instance.RemoveFromList(obj);
                Debug.Log("remove from list");
            }
            else if (GetComponent<Card>() != null)
            {
                Debug.Log("add to list");
                DeckSceneManager.instance.AddToList(obj);
            }
            }
    }
}
