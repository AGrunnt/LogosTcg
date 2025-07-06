using System;
using UnityEngine;

using DG.Tweening;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.Collections;
using UnityEngine.UI;
using Unity.VisualScripting;
using LogoTcg;
using UnityEngine.InputSystem;

namespace LogosTcg
{
    public class GobjectVisual : MonoBehaviour
    {
        private bool initalize = false;

        [Header("Gobject")]
        public Gobject parentGobject;
        public Transform gobjectTransform; //was priv
        public Vector3 rotationDelta; //was priv
        public int savedIndex; //waspriv
        public Vector3 movementDelta; //was none
        private Canvas canvas;

        [Header("References")]
        public Transform visualShadow;
        private Vector3 shadowDragOffset = new Vector3(5, -15, 0); //private
        private Vector3 shadowHoverOffset = new Vector3(3, -10, 0); //private
        private Vector2 shadowDistance;
        private bool shadowOffsetActive = false;
        [SerializeField] private Transform shakeParent;
        [SerializeField] private Transform tiltParent;
        [SerializeField] private Transform gobjectShadow;
        [SerializeField] public Transform holder;

        [Header("Follow Parameters")]
        [SerializeField] private float followSpeed = 30;

        [Header("Rotation Parameters")]
        [SerializeField] private float rotationAmount = 20;
        [SerializeField] private float rotationSpeed = 20;
        [SerializeField] private float autoTiltAmount = 30;
        [SerializeField] private float manualTiltAmount = 20;
        [SerializeField] private float tiltSpeed = 20;

        [Header("Scale Parameters")]
        [SerializeField] private bool scaleAnimations = true;
        [SerializeField] private float scaleOnHover = 1.15f;
        [SerializeField] private float scaleOnSelect = 1.25f;
        [SerializeField] private float scaleTransition = .15f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;

        [Header("Select Parameters")]
        [SerializeField] private float selectPunchAmount = 20;

        [Header("Hober Parameters")]
        [SerializeField] private float hoverPunchAngle = 5;
        [SerializeField] private float hoverTransition = .15f;

        [Header("Swap Parameters")]
        [SerializeField] private bool swapAnimations = true;
        [SerializeField] private float swapRotationAngle = 30;
        [SerializeField] private float swapTransition = .15f;
        [SerializeField] private int swapVibrato = 5;

        [Header("Curve")]
        [SerializeField] private CurveParameters curve;

        private float curveYOffset;
        private float curveRotationOffset;
        private Coroutine pressCoroutine;

        private void Start()
        {
            canvas.sortingLayerName = "Cards";
            shadowDistance = visualShadow.localPosition;
            GetComponent<Canvas>().sortingOrder = transform.parent.GetSiblingIndex();
        }
        void CopyRectTransform(RectTransform src, RectTransform dst, Vector2 offset)
        {
            // anchors & pivot
            dst.anchorMin = src.anchorMin;
            dst.anchorMax = src.anchorMax;
            dst.pivot = src.pivot;

            // size
            dst.sizeDelta = src.sizeDelta;

            // rotation & scale
            dst.localRotation = src.localRotation;
            dst.localScale = src.localScale;

            // position with offset
            dst.anchoredPosition = src.anchoredPosition + offset;
        }

        public void Initialize(Gobject target, int index = 0)
        {
            //Debug.Log(target.transform.name);

            //Declarations
            parentGobject = target;
            gobjectTransform = target.transform;
            canvas = GetComponent<Canvas>();
            transform.position = gobjectTransform.position;
            visualShadow.GetComponent<Image>().sprite = parentGobject.Shadow.GetComponent<Image>().sprite;
            CopyRectTransform(parentGobject.Shadow.GetComponent<RectTransform>(), visualShadow.GetComponent<RectTransform>(), new Vector2(3f, -6f));


            //Event Listening
            parentGobject.PointerEnterEvent.AddListener(PointerEnter);
            parentGobject.PointerExitEvent.AddListener(PointerExit);
            parentGobject.BeginDragEvent.AddListener(BeginDrag);
            parentGobject.EndDragEvent.AddListener(EndDrag);
            parentGobject.PointerDownEvent.AddListener(PointerDown);
            parentGobject.PointerUpEvent.AddListener(PointerUp);
            parentGobject.SelectEvent.AddListener(Select);

            //Initialization
            initalize = true;
        }

        public void UpdateIndex(int length)
        {
            transform.SetSiblingIndex(parentGobject.transform.parent.GetSiblingIndex());
        }

        void Update()
        {
            if (!initalize || parentGobject == null) return;

            HandPositioning();
            SmoothFollow();
            FollowRotation();

            if(!parentGobject.isDragging)
                GobjectTilt();

        }

        private void HandPositioning()
        {
            curveYOffset = (curve.positioning.Evaluate(parentGobject.NormalizedPosition()) * curve.positioningInfluence) * parentGobject.SiblingAmount();
            curveYOffset = parentGobject.SiblingAmount() < 5 ? 0 : curveYOffset;
            curveRotationOffset = curve.rotation.Evaluate(parentGobject.NormalizedPosition());
        }

        private void SmoothFollow()
        {
            Vector3 verticalOffset = (Vector3.up * (parentGobject.isDragging ? 0 : curveYOffset));
            transform.position = Vector3.Lerp(transform.position, gobjectTransform.position + verticalOffset, followSpeed * Time.deltaTime);
        }

        private void FollowRotation()
        {
            Vector3 movement = (transform.position - gobjectTransform.position);
            
            movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
            Vector3 movementRotation = (parentGobject.isDragging ? movementDelta : movement) * rotationAmount;
            rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, rotationSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Clamp(rotationDelta.x, -60, 60));
        }

        private void GobjectTilt()
        {
            savedIndex = parentGobject.isDragging ? savedIndex : parentGobject.ParentIndex();
            float sine = Mathf.Sin(Time.time + savedIndex) * (parentGobject.isHovering ? .2f : 1);
            float cosine = Mathf.Cos(Time.time + savedIndex) * (parentGobject.isHovering ? .2f : 1);

            Vector3 offset = transform.position - Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            float tiltX = parentGobject.isHovering ? ((offset.y * -1) * manualTiltAmount) : 0;
            float tiltY = parentGobject.isHovering ? ((offset.x) * manualTiltAmount) : 0;
            float tiltZ = parentGobject.isDragging ? tiltParent.eulerAngles.z : (curveRotationOffset * (curve.rotationInfluence * parentGobject.SiblingAmount()));

            float lerpX = Mathf.LerpAngle(tiltParent.eulerAngles.x, tiltX + (sine * autoTiltAmount), tiltSpeed * Time.deltaTime);
            float lerpY = Mathf.LerpAngle(tiltParent.eulerAngles.y, tiltY + (cosine * autoTiltAmount), tiltSpeed * Time.deltaTime);
            float lerpZ = Mathf.LerpAngle(tiltParent.eulerAngles.z, tiltZ, tiltSpeed / 2 * Time.deltaTime);

            tiltParent.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);
        }

        private void Select(Gobject gobject, bool state)
        {
            DOTween.Kill(2, true);
            float dir = state ? 1 : 0;
            shakeParent.DOPunchPosition(shakeParent.up * selectPunchAmount * dir, scaleTransition, 10, 1);
            shakeParent.DOPunchRotation(Vector3.forward * (hoverPunchAngle / 2), hoverTransition, 20, 1).SetId(2);

            if (scaleAnimations)
                transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

            if (parentGobject.selected)
            {
                autoTiltAmount = 20;
                visualShadow.localPosition += shadowHoverOffset;
            }
            else
            {
                autoTiltAmount = 0; visualShadow.localPosition -= shadowHoverOffset;
            }
        }

        public void Swap(float dir = 1)
        {
            if (!swapAnimations)
                return;

            DOTween.Kill(2, true);
            shakeParent.DOPunchRotation((Vector3.forward * swapRotationAngle) * dir, swapTransition, swapVibrato, 1).SetId(3);
        }

        //Scale up and up layer
        private void BeginDrag(Gobject gobject)
        {
            if (scaleAnimations)
                transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

            canvas.sortingLayerName = "FrontCards";
        }

        //Scales back and lower layer
        private void EndDrag(Gobject gobject)
        {
            transform.DOScale(1, scaleTransition).SetEase(scaleEase);
            canvas.sortingLayerName = "Cards";
        }

        // scale up, shake, offset shadow
        private void PointerEnter(Gobject gobject)
        {
            if (scaleAnimations)
                transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

            DOTween.Kill(2, true);
            shakeParent.DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20, 1).SetId(2);


            if (!shadowOffsetActive)
            {
                visualShadow.localPosition += shadowHoverOffset;
                shadowOffsetActive = true;
            }

        }


        private void PointerExit(Gobject gobject)
        {
            if (!parentGobject.wasDragged)
            {
                transform.DOScale(1, scaleTransition).SetEase(scaleEase);
            }

            if (shadowOffsetActive)
            {
                visualShadow.localPosition -= shadowHoverOffset;
                shadowOffsetActive = false;
            } 
        }

        private void PointerUp(Gobject gobject, bool longPress)
        {
            if (scaleAnimations)
                transform.DOScale(longPress ? scaleOnHover : scaleOnSelect, scaleTransition).SetEase(scaleEase);

            visualShadow.localPosition -= shadowDragOffset;
        }

        private void PointerDown(Gobject gobject)
        {
            if (scaleAnimations)
                transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

            visualShadow.localPosition += shadowDragOffset;
        }

    }
}