using LogoTcg;
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

        void Start()
        {
            if (raycaster == null) raycaster = GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>();
            if (eventSystem == null) eventSystem = EventSystem.current;
        }

        void OnSelect(Gobject obj)
            {
                Debug.Log("selected");
            }

    }
}

