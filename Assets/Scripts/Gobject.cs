using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem;               // ? new
using LogosTcg;
using NUnit.Framework;
using System.Collections.Generic;
using DG.Tweening;

namespace LogoTcg
{
    public class Gobject : MonoBehaviour,
        IDragHandler, IBeginDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerUpHandler, IPointerDownHandler
    {
        public Canvas canvas;
        //public Canvas canvasChild;
        private Image imageComponent;
        [SerializeField] private bool instantiateVisual = true;
        private VisualHandler visualHandler;
        private Vector3 offset;
        [SerializeField] private bool center = true;
        public string objType;

        [Header("Movement")]
        [SerializeField] private float moveSpeedLimit = 50;

        [Header("Selection")]
        public bool selected;
        public float selectionOffset = 50;
        private float pointerDownTime;
        private float pointerUpTime;

        [Header("Visual")]
        [SerializeField] private GameObject gobjectVisualPrefab;
        [SerializeField] public GobjectVisual gobjectVisual; //visible
        [SerializeField] public Transform Shadow;

        [Header("States")]
        public bool isHovering = false;
        public bool isDragging = false;
        [HideInInspector] public bool wasDragged;
        public bool draggable = false;
        public bool hoverable = false;
        public bool selectable = false;

        [Header("Events")]
        [HideInInspector] public UnityEvent<Gobject> PointerEnterEvent;
        [HideInInspector] public UnityEvent<Gobject> PointerExitEvent;
        [HideInInspector] public UnityEvent<Gobject, bool> PointerUpEvent;
        [HideInInspector] public UnityEvent<Gobject> PointerDownEvent;
        [HideInInspector] public UnityEvent<Gobject> BeginDragEvent;
        [HideInInspector] public UnityEvent<Gobject> DragEvent;
        [HideInInspector] public UnityEvent<Gobject> EndDragEvent;
        [HideInInspector] public UnityEvent<Gobject, bool> SelectEvent;

        void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            
            imageComponent = GetComponent<Image>(); //was getting the image comp of the card. now want to assign it

            if (!instantiateVisual)
                return;

            var directChildren = new List<Transform>();
            foreach (Transform child in transform)
                directChildren.Add(child);

            visualHandler = FindFirstObjectByType<VisualHandler>();
            gobjectVisual = Instantiate(
                gobjectVisualPrefab,
                visualHandler ? visualHandler.transform : this.transform
            ).GetComponent<GobjectVisual>();
            gobjectVisual.Initialize(this);

            foreach (Transform child in directChildren)
                child.SetParent(gobjectVisual.holder);

            //canvasChild = GetComponentInChildren<Canvas>();
        }

        void Update()
        {
            ClampPosition();

            if (isDragging)
            {
                // ? use new Input System
                Vector2 pointerScreenPos = Mouse.current.position.ReadValue();
                Vector2 targetPosition = Camera.main.ScreenToWorldPoint(pointerScreenPos) - offset;
                Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
                Vector2 velocity = direction * Mathf.Min(
                    moveSpeedLimit,
                    Vector2.Distance(transform.position, targetPosition) / Time.deltaTime
                );
                transform.Translate(velocity * Time.deltaTime);
            }
        }

        void ClampPosition()
        {
            Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(
                Screen.width, Screen.height, Camera.main.transform.position.z
            ));
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
            transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!draggable)
                return;

            BeginDragEvent.Invoke(this);
            //Debug.Log($"begin drag {transform.name}");

            if(State.Instance != null)
                State.Instance.globalDragging = true;

            // ? use new Input System
            Vector2 pointerScreenPos = Mouse.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(pointerScreenPos);
            offset = worldPos - (Vector2)transform.position;

            isDragging = true;
            imageComponent.raycastTarget = false;
            wasDragged = true;
        }

        public void OnDrag(PointerEventData eventData) 
        { 
            DragEvent.Invoke(this);
            //Debug.Log($"drag event {transform.name}");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if(!draggable) return; //fix: may not need

            if(State.Instance != null)
                State.Instance.globalDragging = false;

            EndDragEvent.Invoke(this);
            //Debug.Log($"end drag {transform.name}");

            isDragging = false;
            imageComponent.raycastTarget = true;

            StartCoroutine(FrameWait());

            IEnumerator FrameWait()
            {
                yield return new WaitForEndOfFrame();
                wasDragged = false;
            }

            if(center && transform.parent.GetComponent<HorizontalLayoutGroup>() == null && transform.parent.GetComponent<GridLayoutGroup>() == null)
                transform.DOLocalMove(Vector3.zero, .15f).SetEase(Ease.OutBack); //fix

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!hoverable || State.Instance == null || State.Instance.globalDragging) return;

            PointerEnterEvent.Invoke(this);
            //Debug.Log($"pointer enter {transform.name}");
            isHovering = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!hoverable) return;

            PointerExitEvent.Invoke(this);
            //Debug.Log($"pointer exit {transform.name}");
            isHovering = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(!selectable) return;

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            PointerDownEvent.Invoke(this);
            //Debug.Log($"pointer down {transform.name}");
            pointerDownTime = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!selectable) return;

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            pointerUpTime = Time.time;
            bool longPress = (pointerUpTime - pointerDownTime) > .2f;
            PointerUpEvent.Invoke(this, longPress);
            //Debug.Log($"pointer up {transform.name}");

            if (longPress || wasDragged) return;

            selected = !selected;
            SelectEvent.Invoke(this, selected);

            if (selected)
                transform.localPosition += (gobjectVisual.transform.up * selectionOffset);
            else
                transform.localPosition -= (gobjectVisual.transform.up * selectionOffset);
        }

        public void Deselect()
        {
            Debug.Log("deselected");
            if (!selected) return;
            selected = false;
            transform.localPosition = Vector3.zero;
        }

        public int SiblingAmount()
        {
            return transform.parent.CompareTag("Slot")
                ? transform.parent.parent.childCount - 1
                : 0;
        }

        public int ParentIndex()
        {
            return transform.parent.CompareTag("Slot")
                ? transform.parent.GetSiblingIndex()
                : 0;
        }

        public float NormalizedPosition()
        {
            if (!transform.parent.CompareTag("Slot")) return 0;
            int slotCount = transform.parent.parent.childCount - 1;
            return ExtensionMethods.Remap(
                (float)ParentIndex(), 0, (float)slotCount, 0, 1
            );
        }

        private void OnDestroy()
        {
            if (gobjectVisual != null)
                Destroy(gobjectVisual.gameObject);
        }
    }
}
