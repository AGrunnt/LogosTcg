using UnityEngine;
using UnityEngine.EventSystems;

namespace LogosTcg
{
    public class CardTogAttachment : MonoBehaviour, IPointerClickHandler
    {
        public bool inList = false;
        public void OnPointerClick(PointerEventData eventData)
        {
            // This fires for any click that hits a Graphic under your raycaster
            Debug.Log("UI element clicked: " + eventData.pointerCurrentRaycast.gameObject.name);
        }
    }
}
