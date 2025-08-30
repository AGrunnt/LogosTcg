using JetBrains.Annotations;
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


            if (obj.GetComponent<CardLine>() != null )
            {
                Debug.Log("has cardLine toggle");
                ListManager.instance.RemoveFromList(obj.GetComponent<CardLine>().addressableKey);
                RefreshGrid();
            }
            else if (GetComponent<Card>() != null )
            {
                Debug.Log("has card only toggle");
                ListManager.instance.AddToList(obj.GetComponent<Card>().addressableKey);
                RefreshGrid();
            }


            /*
            async void RefreshGrid()
            {
                await GridManager.instance.RefreshGridAsync();
            }
            */
        }

        async void RefreshGrid()
        {
            await GridManager.instance.RefreshGridAsync();
        }

        [ContextMenu("Refresh Grid")]
        public void RefreshGridContext()
        {
            Debug.Log("refreshed");
            RefreshGrid();
        }
    }
}
