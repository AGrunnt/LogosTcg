using UnityEngine;

namespace LogosTcg
{
    public class GridSlotActions : MonoBehaviour
    {
        [SerializeField] private bool pullOnEmptyBool = true;
        [SerializeField] private Transform pullTransform;

        void Start()
        {
            GetComponent<SlotScript>().SlotChg.AddListener(pullOnEmpty);
        }

        private void pullOnEmpty(SlotScript slot)
        {
            if (!pullOnEmptyBool) return;

            if(transform.childCount == 0)
            {
                pullTransform.GetComponentsInChildren<Card>()[0].transform.SetParent(this.transform);
            }
        }

    }
}
