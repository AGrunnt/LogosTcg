using DG.Tweening;
using LogoTcg;
using NUnit.Framework.Internal;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

namespace LogosTcg
{
    public class CardDropHandler : MonoBehaviour, IDropHandler
    {
        Card card;
        GameNetworkManager gnm;
        BoardElements be;
        GameManager gm;

        public UnityEvent onDrop;

        void Start()
        {
            gnm = GameNetworkManager.Instance;
            card = GetComponent<Card>();
            be = BoardElements.instance;
            gm = GameManager.Instance;
        }

        // This will be called when something is dropped on this UI element
        public void OnDrop(PointerEventData eventData)
        {
            var dropped = eventData.pointerDrag;

            if (dropped.tag == "Card" && dropped.GetComponent<Card>()._definition.Abilities.Any(t => t.AbilityType.Contains("Instant")))
            {
                if(dropped.GetComponent<Card>()._definition.Abilities.Any(t => t.AbilityType.Contains("PreventLocPlacement")) && GetComponent<Card>()._definition.Type[0] == "Location" && transform.parent.parent.GetComponentsInChildren<Card>().Count() <= 1 && GetComponent<Card>().turnsInPlay == 0)
                {
                    int idx = UnityEngine.Random.Range(0, be.locDeck.childCount); // 0..childCount-1
                    transform.SetParent(be.locDeck, true);
                    transform.SetSiblingIndex(idx);
                    transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);
                    be.locDeck.GetComponent<SlotScript>().InitializeSlots();


                    dropped.transform.SetParent(be.discard, true);
                    dropped.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);
                    be.discard.GetComponent<SlotScript>().InitializeSlots();
                }
            }

 //dropped.GetComponent<Card>()?._definition?.Abilities?.AbilityType?.Any(t => string.Equals(t, "Instant", StringComparison.OrdinalIgnoreCase))?? false;

            if (dropped.tag == "Coin" || transform.parent.GetComponent<SlotScript>().faceup)
            {
                if (NetworkManager.Singleton != null)
                {
                    gnm.CoinDropServerRpc(dropped.transform.parent.parent.name, card.name, dropped.GetComponent<Coin>().value);
                }
                else
                {
                    int overkill = card.SetValue(dropped.GetComponent<Coin>().value + gm.coinModifier);
                    Card orgCard = dropped.transform.parent.parent.GetComponent<Card>();
                    orgCard.SetValue(dropped.GetComponent<Coin>().value - overkill);
                    //Debug.Log($"orgCard Val {dropped.GetComponent<Coin>().value - overkill}");
                    orgCard.GetComponent<CoinStack>().ReVisible();
                }

                Debug.Log("coin dropped");

                onDrop?.Invoke();
            }
            // Run event here

            /*
            int overkill = card.SetValue(val);
            Card orgCard = dropped.transform.parent.parent.GetComponent<Card>();
            orgCard.SetValue(dropped.GetComponent<Coin>().value - overkill);
            Debug.Log($"orgCard Val {dropped.GetComponent<Coin>().value - overkill}");
            orgCard.GetComponent<CoinStack>().ReVisible();
            */
        }

        public void DiscardZeroed()
        {
            if (card.currValue > 0) return;

            if (card._definition.Type[0] == "Location" && transform.parent.parent.GetComponentsInChildren<Card>().Count() > 1) return;

            /*
            var orgSpeed = GetComponent<Gobject>().gobjectVisual.followSpeed;
            GetComponent<Gobject>().gobjectVisual.followSpeed = 1;

            transform.SetParent(BoardElements.instance.discard, false);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            BoardElements.instance.discard.GetComponent<SlotScript>().InitializeSlots();

            GetComponent<Gobject>().gobjectVisual.followSpeed = orgSpeed;
            */

            transform.SetParent(BoardElements.instance.discard, true);
            transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);
           
            //transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            BoardElements.instance.discard.GetComponent<SlotScript>().InitializeSlots();

            gm.RmString(GetComponent<Card>()._definition.Title);

            if(GetComponent<Card>()._definition.Type[0] == "Faithless")
                FaithlessAbilities.instance.CardDiscarded(transform);
        }
    }
}