using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LogosTcg
{
    public class FaithlessAbilities : MonoBehaviour
    {
        BoardElements be;
        GameManager gm;

        public static FaithlessAbilities instance;

        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
            be = BoardElements.instance;
            gm = GameManager.Instance;
        }
        public void RunAbilities(Transform tf)
        {
            List<Ability> abilities = tf.GetComponent<Card>()._definition.Abilities;

            if(abilities.Any(ab => ab.AbilityType.Contains("ReviveFaithless")))
            {
                if(be.discard.GetComponentsInChildren<Card>(includeInactive: true).Count(c => c._definition?.Type[0] == "Faithless") > 0)
                {
                    GetComponent<DealCards>().SendTopTo(be.discard, tf.parent);
                } else
                {
                    foreach(Transform locTf in be.locSlots)
                    {
                        if(locTf.GetComponentsInChildren<Card>() == null)
                        {
                            GetComponent<DealCards>().SendTopTo(be.locDeck, locTf);
                            break;
                        }
                    }
                    
                }
            }

            if (abilities.Any(ab => ab.AbilityType.Contains("Remove"))) //should add case for when Support and Adjacent
            {
                int stackIndex = tf.parent.transform.GetSiblingIndex();

                for(int i = stackIndex - 1; i >= stackIndex + 1; i++)
                {
                    Card firstSupport = be.stackSlots[i].GetComponentsInChildren<Card>().FirstOrDefault(c => string.Equals(c._definition.Type[0], "Support"));
                    if (firstSupport != null)
                    {
                        Transform fsTf = firstSupport.transform;
                        fsTf.SetParent(be.discard, true);
                        fsTf.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);
                        be.discard.GetComponent<SlotScript>().InitializeSlots();
                    }

                }
            }

            if (abilities.Any(ab => ab.AbilityType.Contains("IncCost")))
            {
                gm.coinModifier = gm.coinModifier + 1;
            }
        }


        public void CardDiscarded(Transform tf)
        {
            List<Ability> abilities = tf.GetComponent<Card>()._definition.Abilities;
            if (abilities.Any(ab => ab.AbilityType.Contains("IncCost")))
            {
                gm.coinModifier = gm.coinModifier - 1;
            }
        }
    }
}
