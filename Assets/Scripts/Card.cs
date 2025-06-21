using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem;               // ? new
using LogosTcg;

namespace LogoTcg
{
    public class Card : MonoBehaviour,
        IDragHandler, IBeginDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerUpHandler, IPointerDownHandler
    {
        private Canvas canvas;
        private Image imageComponent;
        [SerializeField] private bool instantiateVisual = true;
        private VisualCardsHandler visualHandler;
        private Vector3 offset;

        [Header("Movement")]
        [SerializeField] private float moveSpeedLimit = 50;

        [Header("Selection")]
        public bool selected;
        public float selectionOffset = 50;
        private float pointerDownTime;
        private float pointerUpTime;

        [Header("Visual")]
        [SerializeField] private GameObject cardVisualPrefab;
        [HideInInspector] public CardVisual cardVisual;

        [Header("States")]
        public bool isHovering;
        public bool isDragging;
        [HideInInspector] public bool wasDragged;

        [Header("Events")]
        [HideInInspector] public UnityEvent<Card> PointerEnterEvent;
        [HideInInspector] public UnityEvent<Card> PointerExitEvent;
        [HideInInspector] public UnityEvent<Card, bool> PointerUpEvent;
        [HideInInspector] public UnityEvent<Card> PointerDownEvent;
        [HideInInspector] public UnityEvent<Card> BeginDragEvent;
        [HideInInspector] public UnityEvent<Card> EndDragEvent;
        [HideInInspector] public UnityEvent<Card, bool> SelectEvent;

        void Start()
        {
            canvas = GetComponentInParent<Canvas>();
            imageComponent = GetComponent<Image>();

            if (!instantiateVisual)
                return;

            visualHandler = FindFirstObjectByType<VisualCardsHandler>();
            cardVisual = Instantiate(
                cardVisualPrefab,
                visualHandler ? visualHandler.transform : this.transform
            ).GetComponent<CardVisual>();
            cardVisual.Initialize(this);
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
            BeginDragEvent.Invoke(this);

            // ? use new Input System
            Vector2 pointerScreenPos = Mouse.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(pointerScreenPos);
            offset = worldPos - (Vector2)transform.position;

            isDragging = true;
            canvas.GetComponent<GraphicRaycaster>().enabled = false;
            imageComponent.raycastTarget = false;
            wasDragged = true;
        }

        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData)
        {
            EndDragEvent.Invoke(this);
            isDragging = false;
            canvas.GetComponent<GraphicRaycaster>().enabled = true;
            imageComponent.raycastTarget = true;

            StartCoroutine(FrameWait());

            IEnumerator FrameWait()
            {
                yield return new WaitForEndOfFrame();
                wasDragged = false;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnterEvent.Invoke(this);
            isHovering = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerExitEvent.Invoke(this);
            isHovering = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            PointerDownEvent.Invoke(this);
            pointerDownTime = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            pointerUpTime = Time.time;
            bool longPress = (pointerUpTime - pointerDownTime) > .2f;
            PointerUpEvent.Invoke(this, longPress);
            if (longPress || wasDragged) return;

            selected = !selected;
            SelectEvent.Invoke(this, selected);

            if (selected)
                transform.localPosition += (cardVisual.transform.up * selectionOffset);
            else
                transform.localPosition -= (cardVisual.transform.up * selectionOffset);
            //transform.localPosition = Vector3.zero;
        }

        public void Deselect()
        {
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
            if (cardVisual != null)
                Destroy(cardVisual.gameObject);
        }
    }
}
