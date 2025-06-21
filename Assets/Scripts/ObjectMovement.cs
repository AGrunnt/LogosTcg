using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DG.Tweening;

namespace LogosTcg
{
    public class ObjectMovement : MonoBehaviour,
        IDragHandler, IBeginDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerUpHandler, IPointerDownHandler
    {
        // Core components
        private Canvas canvas;
        private Image imageComponent;
        private GraphicRaycaster canvasRaycaster;

        // Movement & Drag
        [Header("Movement & Drag")]
        [SerializeField] private float moveSpeedLimit = 50f;
        private Vector3 dragOffset;
        private bool isDragging;
        private bool wasDragged;

        // Selection & Hover
        [Header("Selection & Hover")]
        [SerializeField] private float selectionOffset = 50f;
        [SerializeField] private float longPressThreshold = 0.2f;
        private float pointerDownTime;
        private bool isHovering;
        public bool selected;

        // Curve-based card spread (optional grouping)
        [Header("Curve Settings")]
        public AnimationCurve positioningCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float positioningInfluence = 0.1f;
        public AnimationCurve rotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float rotationInfluence = 10f;

        // Animation settings
        [Header("Animation Settings")]
        [SerializeField] private float followSpeed = 30f;
        [SerializeField] private float rotationSpeed = 20f;
        [SerializeField] private float rotationAmount = 20f;
        [SerializeField] private float autoTiltAmount = 30f;
        [SerializeField] private float manualTiltAmount = 20f;
        [SerializeField] private float tiltSpeed = 20f;
        [SerializeField] private bool scaleAnimations = true;
        [SerializeField] private float scaleOnHover = 1.15f;
        [SerializeField] private float scaleOnSelect = 1.25f;
        [SerializeField] private float scaleTransition = 0.15f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;
        [SerializeField] private float hoverPunchAngle = 5f;
        [SerializeField] private float hoverTransition = 0.15f;
        [SerializeField] private bool swapAnimations = true;
        [SerializeField] private float swapPunchAngle = 20f;
        [SerializeField] private float swapTransition = 0.15f;
        [SerializeField] private int swapVibrato = 5;

        // Internal smoothing state
        private Vector3 movementDelta;
        private Vector3 rotationDelta;
        private int savedIndex;

        void Awake()
        {
            imageComponent = GetComponent<Image>();
            canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                canvasRaycaster = canvas.GetComponent<GraphicRaycaster>();
        }

        void Update()
        {
            ClampPosition();
            if (isDragging) HandleDragging();
            UpdateVisual();
        }

        private void ClampPosition()
        {
            if (Camera.main == null) return;
            Vector3 bounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.nearClipPlane));
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, -bounds.x, bounds.x);
            pos.y = Mathf.Clamp(pos.y, -bounds.y, bounds.y);
            transform.position = pos;
        }

        private void HandleDragging()
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            Vector2 target = worldPos - (Vector2)dragOffset;
            Vector2 dir = (target - (Vector2)transform.position).normalized;
            Vector2 vel = dir * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, target) / Time.deltaTime);
            transform.Translate(vel * Time.deltaTime);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            wasDragged = true;
            if (canvasRaycaster != null) canvasRaycaster.enabled = false;
            imageComponent.raycastTarget = false;
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            dragOffset = worldPos - (Vector2)transform.position;
            Visual_BeginDrag();
        }

        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            if (canvasRaycaster != null) canvasRaycaster.enabled = true;
            imageComponent.raycastTarget = true;
            StartCoroutine(ResetWasDragged());
            Visual_EndDrag();
        }

        private IEnumerator ResetWasDragged()
        {
            yield return new WaitForEndOfFrame();
            wasDragged = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
            Visual_PointerEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            Visual_PointerExit();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            pointerDownTime = Time.time;
            Visual_PointerDown();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            bool longPress = (Time.time - pointerDownTime) > longPressThreshold;
            Visual_PointerUp(longPress);
            if (longPress || wasDragged) return;
            ToggleSelect();
        }

        private void ToggleSelect()
        {
            selected = !selected;
            Visual_Select(selected);
            transform.localPosition = selected ? Vector3.up * selectionOffset : Vector3.zero;
        }

        public void Swap(float dir = 1f)
        {
            if (!swapAnimations) return;
            transform.DOPunchRotation(Vector3.forward * swapPunchAngle * dir, swapTransition, swapVibrato, 1);
        }

        private void UpdateVisual()
        {
            // Optional: curve-based stagger if under a Slot parent
            float normalized = 0f;
            int siblings = 0;
            if (transform.parent != null && transform.parent.CompareTag("Slot"))
            {
                siblings = transform.parent.parent.childCount - 1;
                int idx = transform.parent.GetSiblingIndex();
                normalized = ExtensionMethods.Remap(idx, 0, siblings, 0, 1);
            }
            float yOffset = (siblings < 1) ? 0 : positioningCurve.Evaluate(normalized) * positioningInfluence * siblings;
            float rotCurve = rotationCurve.Evaluate(normalized);

            // Smooth follow
            Vector3 targetPos = transform.position + Vector3.up * yOffset;
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

            // Smooth rotation based on motion
            Vector3 movement = transform.position - targetPos;
            movementDelta = Vector3.Lerp(movementDelta, movement, 25f * Time.deltaTime);
            Vector3 desiredRot = (isDragging ? movementDelta : movement) * rotationAmount;
            rotationDelta = Vector3.Lerp(rotationDelta, desiredRot, rotationSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0, 0, Mathf.Clamp(rotationDelta.x, -60f, 60f));

            // Auto tilt when idle or manual tilt when hovering
            float tiltX = isHovering ? -movementDelta.y * manualTiltAmount : Mathf.Sin(Time.time + siblings) * autoTiltAmount;
            float tiltY = isHovering ? movementDelta.x * manualTiltAmount : Mathf.Cos(Time.time + siblings) * autoTiltAmount;
            Vector3 tiltEuler = new Vector3(tiltX, tiltY, transform.eulerAngles.z);
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, tiltEuler, tiltSpeed * Time.deltaTime);
        }

        #region Visual Tween Methods
        private void Visual_PointerEnter()
        {
            if (scaleAnimations)
                transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);
            transform.DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20, 1);
        }

        private void Visual_PointerExit()
        {
            if (scaleAnimations && !wasDragged)
                transform.DOScale(1f, scaleTransition).SetEase(scaleEase);
        }

        private void Visual_BeginDrag()
        {
            if (scaleAnimations)
                transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);
        }

        private void Visual_EndDrag()
        {
            if (scaleAnimations)
                transform.DOScale(1f, scaleTransition).SetEase(scaleEase);
        }

        private void Visual_PointerDown()
        {
            if (scaleAnimations)
                transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);
        }

        private void Visual_PointerUp(bool longPress)
        {
            if (scaleAnimations)
                transform.DOScale(longPress ? scaleOnHover : scaleOnSelect, scaleTransition).SetEase(scaleEase);
        }

        private void Visual_Select(bool state)
        {
            transform.DOPunchScale(Vector3.one * 0.1f * (state ? 1 : -1), scaleTransition, 10, 1);
            if (scaleAnimations)
                transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);
        }
        #endregion
    }

    public static class ExtensionMethods
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}
