using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using LogoTcg;
using UnityEngine.InputSystem; // ? NEW

namespace LogosTcb
{
    public class HorizontalGobjectHolder : MonoBehaviour
    {
        [SerializeField] private Gobject selectedGobject;
        [SerializeReference] private Gobject hoveredGobject;

        [SerializeField] private GameObject slotPrefab;
        private RectTransform rect;

        [Header("Spawn Settings")]
        [SerializeField] private int gobjectsToSpawn = 7;
        public List<Gobject> gobjects;

        bool isCrossing = false;
        [SerializeField] private bool tweenGobjectReturn = true;

        // ? NEW INPUT ACTIONS
        private InputAction deleteAction;
        private InputAction rightClickAction;

        void Awake()
        {
            // Setup InputActions
            deleteAction = new InputAction(binding: "<Keyboard>/delete");
            rightClickAction = new InputAction(binding: "<Mouse>/rightButton");
        }

        void OnEnable()
        {
            deleteAction.Enable();
            rightClickAction.Enable();
        }

        void OnDisable()
        {
            deleteAction.Disable();
            rightClickAction.Disable();
        }

        void Start()
        {
            for (int i = 0; i < gobjectsToSpawn; i++)
            {
                Instantiate(slotPrefab, transform);
            }

            rect = GetComponent<RectTransform>();
            gobjects = GetComponentsInChildren<Gobject>().ToList();

            int gobjectCount = 0;

            foreach (Gobject gobject in gobjects)
            {
                gobject.PointerEnterEvent.AddListener(GobjectPointerEnter);
                gobject.PointerExitEvent.AddListener(GobjectPointerExit);
                gobject.BeginDragEvent.AddListener(BeginDrag);
                gobject.EndDragEvent.AddListener(EndDrag);
                gobject.name = gobjectCount.ToString();
                gobjectCount++;
            }

            StartCoroutine(Frame());

            IEnumerator Frame()
            {
                yield return new WaitForSecondsRealtime(.1f);
                for (int i = 0; i < gobjects.Count; i++)
                {
                    if (gobjects[i].gobjectVisual != null)
                        gobjects[i].gobjectVisual.UpdateIndex(transform.childCount);
                }
            }
        }

        private void BeginDrag(Gobject gobject)
        {
            selectedGobject = gobject;
        }

        void EndDrag(Gobject gobject)
        {
            if (selectedGobject == null)
                return;

            selectedGobject.transform.DOLocalMove(selectedGobject.selected ? new Vector3(0, selectedGobject.selectionOffset, 0) : Vector3.zero, tweenGobjectReturn ? .15f : 0).SetEase(Ease.OutBack);

            rect.sizeDelta += Vector2.right;
            rect.sizeDelta -= Vector2.right;

            selectedGobject = null;
        }

        void GobjectPointerEnter(Gobject gobject)
        {
            hoveredGobject = gobject;
        }

        void GobjectPointerExit(Gobject gobject)
        {
            hoveredGobject = null;
        }

        void Update()
        {
            if (deleteAction.WasPressedThisFrame())
            {
                if (hoveredGobject != null)
                {
                    Destroy(hoveredGobject.transform.parent.gameObject);
                    gobjects.Remove(hoveredGobject);
                }
            }

            if (rightClickAction.WasPressedThisFrame())
            {
                foreach (Gobject gobject in gobjects)
                {
                    gobject.Deselect();
                }
            }

            if (selectedGobject == null || isCrossing)
                return;

            for (int i = 0; i < gobjects.Count; i++)
            {
                if (selectedGobject.transform.position.x > gobjects[i].transform.position.x)
                {
                    if (selectedGobject.ParentIndex() < gobjects[i].ParentIndex())
                    {
                        Swap(i);
                        break;
                    }
                }

                if (selectedGobject.transform.position.x < gobjects[i].transform.position.x)
                {
                    if (selectedGobject.ParentIndex() > gobjects[i].ParentIndex())
                    {
                        Swap(i);
                        break;
                    }
                }
            }
        }

        void Swap(int index)
        {
            isCrossing = true;

            Transform focusedParent = selectedGobject.transform.parent;
            Transform crossedParent = gobjects[index].transform.parent;

            gobjects[index].transform.SetParent(focusedParent);
            gobjects[index].transform.localPosition = gobjects[index].selected ? new Vector3(0, gobjects[index].selectionOffset, 0) : Vector3.zero;
            selectedGobject.transform.SetParent(crossedParent);

            isCrossing = false;

            if (gobjects[index].gobjectVisual == null)
                return;

            bool swapIsRight = gobjects[index].ParentIndex() > selectedGobject.ParentIndex();
            gobjects[index].gobjectVisual.Swap(swapIsRight ? -1 : 1);

            foreach (Gobject gobject in gobjects)
            {
                gobject.gobjectVisual.UpdateIndex(transform.childCount);
            }
        }
    }
}
