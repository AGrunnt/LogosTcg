using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;


namespace LogosTcg
{
    public class ColumnAbilities : MonoBehaviour
    {
        public static ColumnAbilities instance;
        BoardElements be;

        private void Awake()
        {
            be = BoardElements.instance;
            instance = this;
        }

        public void RunAbilities(Transform tf)
        {
            List<Ability> abilities = tf.GetComponent<Card>()._definition.Abilities;

            foreach (Ability ab in abilities)
            {
                if (ab.AbilityType[0] == "Occupy")
                {
                    DisallowFaithfless();
                    SetAltFilled();
                }

                if (ab.AbilityType[0] == "Disoccupy")
                {
                    if(GetComponent<ColumnScript>().Occuppied)
                    {
                        SetAltNotFilled();
                        Unoccupy();
                    }
                    AllowFaithfless();

                }
            }
        }

        public void AllowFaithfless()
        {
            GetComponent<ColumnScript>().FaithlessAllowed = true;
        }

        public void DisallowFaithfless()
        {
            GetComponent<ColumnScript>().FaithlessAllowed = false;
        }

        public void SetAltFilled()
        {
            GetComponent<ColumnScript>().AltFilled = true;
        }

        public void SetAltNotFilled()
        {
            GetComponent<ColumnScript>().AltFilled = false;
        }

        public void Unoccupy()
        {
            GetComponent<ColumnScript>().Occuppied = false;
        }

        public void RemoveNeutral()
        {
            List<Card> cards = GetComponentsInChildren<Card>().ToList();

            foreach (Card card in cards)
            {
                if (card._definition.Type[0] == "Neutral")
                {
                    card.transform.SetParent(be.discard, true);
                    card.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);
                    be.discard.GetComponent<SlotScript>().InitializeSlots();
                }
            }

        }
    }
}
