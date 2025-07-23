using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
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


            if (obj.GetComponent<CardLine>() != null)
            {
                ListManager.instance.RemoveFromList(obj);
                /*
                if(NetworkManager.Singleton == null)
                    DeckSceneManager.instance.RemoveFromList(obj);
                else
                    DeckSceneManager.instance.RemoveFromOnlineListServerRpc(obj.GetComponent<Card>().addressableKey, DeckSceneManager.instance.currPlayer);
                */
                Debug.Log("remove from list");
            }
            else if (GetComponent<Card>() != null)
            {
                ListManager.instance.AddToList(obj);
                /*
                if (NetworkManager.Singleton == null)
                    DeckSceneManager.instance.AddToList(obj);
                else
                    DeckSceneManager.instance.AddToOnlineList(obj);

                */
            }
            }
    }
}
