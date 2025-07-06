using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using LogoTcg;

namespace LogosTcg
{
    public class HorizontalGobjectHolder : MonoBehaviour
    {
        // drag events come in on the Gobject itself
        public List<Gobject> gobjects = new List<Gobject>();

        RectTransform rect;
        public Transform tempHold;
        Gobject selectedObj;
        bool isDragging = false;

        [SerializeField] float tweenDuration = 0.15f;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        void Start()
        {
            // hook into their drag callbacks
            foreach (var obj in gobjects)
            {
                obj.BeginDragEvent.AddListener(OnBeginDrag);
                obj.DragEvent.AddListener(OnDrag);      // now matches UnityEvent<Gobject>
                obj.EndDragEvent.AddListener(OnEndDrag);
            }

            // initial spread
            LayoutCards();
        }

        void OnTransformChildrenChanged()
        {
            
            // if you ever reparent cards under this holder, resync:
            gobjects = transform.GetComponentsInChildren<Gobject>().ToList<Gobject>();

            if (gobjects == null) return;

            if(Gobject.Count > 5)
                rect.sizeDelta = new Vector2(gobjects.Count * gobjects[0].GetComponent<RectTransform>().sizeDelta.x * 1.1f, rect.sizeDelta.y);
            
            LayoutCards();
        }

        void OnBeginDrag(Gobject obj)
        {
            selectedObj = obj;
            isDragging = true;

            // remove from the layout list so it can move freely
            gobjects.Remove(obj);
        }

        // ? this signature now matches UnityEvent<Gobject>
        void OnDrag(Gobject dragged)
        {
            // only care about the currently selected card
            if (selectedObj == null || dragged != selectedObj)
                return;

            // convert screen mouse pos to local-x in our RectTransform
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect,
                Input.mousePosition,
                Camera.main,            // or null if your canvas is Overlay
                out Vector2 localPoint
            );

            // compute *where* in the hand it would land:
            int newIndex = CalculateInsertIndex(localPoint.x);

            // if it really moved, update the list & re-layout
            if (gobjects.IndexOf(selectedObj) != newIndex)
            {
                gobjects.Insert(newIndex, selectedObj);
                LayoutCards();
            }
        }

        void OnEndDrag(Gobject obj)
        {
            isDragging = false;

            // put it back into the layout fully
            if (!gobjects.Contains(obj))
                gobjects.Add(obj);

            LayoutCards();
            selectedObj = null;
        }

        int CalculateInsertIndex(float localX)
        {
            int count = gobjects.Count + 1; // +1 because selectedObj is out
            if (count <= 1) return 0;

            float width = rect.rect.width;
            // map [-width/2 .. +width/2] to [0 .. count-1]
            float normalized = Mathf.InverseLerp(-width / 2, width / 2, localX);
            int idx = Mathf.RoundToInt(normalized * (count - 1));
            return Mathf.Clamp(idx, 0, count - 1);
        }

        void LayoutCards()
        {
            int count = gobjects.Count;
            if (selectedObj != null && isDragging)
                count++; // include the dragged card as placeholder

            float width = rect.rect.width;
            float spacing = count > 1 ? width / (count - 1) : 0;
            float startX = -width / 2;

            // now tween each card to its target spot
            for (int i = 0; i < gobjects.Count; i++)
            {
                var obj = gobjects[i];

                // skip the one being dragged so it follows the pointer
                if (obj == selectedObj && isDragging)
                    continue;

                Vector2 target = new Vector2(
                    startX + spacing * i,
                    obj.selected ? obj.selectionOffset : 0
                );
                obj.transform.DOLocalMove(target, tweenDuration)
                             .SetEase(Ease.OutBack);
            }
        }
    }
}
