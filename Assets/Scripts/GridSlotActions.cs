using LogoTcg;
using System.Linq;
using UnityEngine;

namespace LogosTcg
{
    public class GridSlotActions : MonoBehaviour
    {

        [SerializeField] private Transform pullVertTransform;
        [SerializeField] private Transform pushVertTransform;
        [SerializeField] private Transform pullHorzTransform;
        [SerializeField] private Transform pushHorzTransform;


        void Start()
        {
            GetComponent<SlotScript>().SlotChg.AddListener(pullOnVertEmpty);
            GetComponent<SlotScript>().SlotChg.AddListener(pushVert);
        }

        public void pullOnVertEmpty(SlotScript slot)
        {
            //if (!pullOnEmptyBool) return;

            if(pullVertTransform != null && transform.GetComponentsInChildren<Card>().Count() == 0 && pullVertTransform.GetComponentsInChildren<Card>().Count() != 0)
            {
                var card = pullVertTransform.GetComponentsInChildren<Card>()[0];
                card.GetComponent<Gobject>().runOnline = false;
                card.transform.SetParent(this.transform, false);
                Debug.Log("ranpull");
            }
        }

        public void pushVert(SlotScript slot)
        {
            if(pushVertTransform != null && pushVertTransform.GetComponentsInChildren<Card>().Count() == 0 && transform.GetComponentsInChildren<Card>().Count() != 0)
            {
                var card = this.GetComponentsInChildren<Card>()[0];
                card.GetComponent<Gobject>().runOnline = false;
                card.transform.SetParent(pushVertTransform, false);
                Debug.Log("ranpush");

            }
        }

    }
}
