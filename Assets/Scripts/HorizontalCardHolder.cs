using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using LogoTcg;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace LogosTcg
{
    public class HorizontalGobjectHolder : MonoBehaviour
    {
        // drag events come in on the Gobject itself
        public List<Gobject> gobjects = new List<Gobject>();
        public GraphicRaycaster raycaster;
        public EventSystem eventSystem;
        [SerializeField] bool swap = false;
        RectTransform rect;
        Gobject selectedObj;
        //bool isDragging = false;

        //[SerializeField] float tweenDuration = 0.15f;

        void Awake()
        {
            // auto-find if you forgot to assign
            if (raycaster == null) raycaster = GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>();
            if (eventSystem == null) eventSystem = EventSystem.current;
            rect = GetComponent<RectTransform>();
        }


        void OnTransformChildrenChanged()
        {
            //List<Gobject> prevGobjects = gobjects;
            List<Gobject> prevGobjects = new List<Gobject>(gobjects);

            gobjects.Clear();
            foreach (Transform child in this.transform)
                if (child.TryGetComponent<Gobject>(out var comp))
                    gobjects.Add(comp);
            
            // if you ever reparent cards under this holder, resync:
            //gobjects = transform.GetComponentsInChildren<Gobject>().ToList<Gobject>();

            if (gobjects == null) return;

            if(gobjects.Count > 5 && GetComponent<HorizontalLayoutGroup>() != null)
                rect.sizeDelta = new Vector2(gobjects.Count * gobjects[0].GetComponent<RectTransform>().sizeDelta.x * 1.1f, rect.sizeDelta.y);

            // everything in list2 that isn’t in list1
            List<Gobject> missingObjs = prevGobjects
                .Except(gobjects)
                .ToList();

            // everything in list1 that isn’t in list2
            List<Gobject> newObjs = gobjects
                .Except(prevGobjects)
                .ToList();

            foreach (Gobject missingObj in missingObjs)
            {
                missingObj.BeginDragEvent.RemoveListener(OnBeginDrag);
                missingObj.DragEvent.RemoveListener(OnDrag);      // now matches UnityEvent<Gobject>
                missingObj.EndDragEvent.RemoveListener(OnEndDrag);
            }

            foreach (Gobject newObj in newObjs)
            {
                //Debug.Log(newObj.name);
                newObj.BeginDragEvent.AddListener(OnBeginDrag);
                newObj.DragEvent.AddListener(OnDrag);      // now matches UnityEvent<Gobject>
                newObj.EndDragEvent.AddListener(OnEndDrag);
            }
        }

        void OnBeginDrag(Gobject obj)
        {
        }

        // ? this signature now matches UnityEvent<Gobject>
        void OnDrag(Gobject dragged)
        {
            Debug.Log("try dragging");
            if (!swap) return;

            Vector2 screenPos = Mouse.current.position.ReadValue();

            var pointer = new PointerEventData(eventSystem)
            {
                position = screenPos
            };
            var results = new List<RaycastResult>();
            raycaster.Raycast(pointer, results);

            // only deal with the one we began dragging
            //if (dragged != selectedObj) return;

            // (A) Move your card with the mouse
            //dragged.transform.position = Input.mousePosition;

            // (B) Build a pointer event at the current mouse position

            /*
            var pointer = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            Vector2 screenPos = Mouse.current.position.ReadValue();

            // (C) Raycast into all UI elements under the mouse
            var results = new List<RaycastResult>();
            raycaster.Raycast(pointer, results);
            */

            // (D) Find the first other Gobject under the cursor
            var hit = results
                .Select(r => r.gameObject.GetComponentInParent<Gobject>())
                .FirstOrDefault(g => g != null && g != selectedObj);
            //Debug.Log("hit maybe");
            // (E) If we hit one, swap their sibling indices
            if (hit != null)
            {
                
                var selT = dragged.transform;
                var hitT = hit.transform;

                int selIndex = selT.GetSiblingIndex();
                int hitIndex = hitT.GetSiblingIndex();

                if (selIndex != hitIndex)
                {
                    selT.SetSiblingIndex(hitIndex);
                    hitT.SetSiblingIndex(selIndex);
                }
            }
        }


        void OnEndDrag(Gobject obj) //fix: might run when anything is dragged
        {
            //GetComponent<HorizontalLayoutGroup>().CalculateLayoutInputHorizontal();
            if (GetComponent<LayoutGroup>() != null)
            {
                GetComponent<LayoutGroup>().SetLayoutHorizontal();
                GetComponent<LayoutGroup>().SetLayoutVertical();
            }

        }



    }
}
