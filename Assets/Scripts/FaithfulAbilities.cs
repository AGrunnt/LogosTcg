using System.Collections.Generic;
using UnityEngine;

namespace LogosTcg
{
    public class FaithfulAbilities : MonoBehaviour
    {
        GameManager gm;

        private void Start()
        {
            gm = GameManager.Instance;
        }

        public void RunAbilities(Transform tf)
        {
            List<Ability> abilities = tf.GetComponent<Card>()._definition.Abilities;

            foreach (Ability ab in abilities)
            {
                if (ab.AbilityType[0] == "Add" && gm.inString.Contains(ab.Target[0]))
                {
                    tf.GetComponent<Card>().SetValue(int.Parse(ab.Tag[0]));
                }

                if (ab.AbilityType[0] == "Minus" && gm.inString.Contains(ab.Target[0]))
                {
                    tf.GetComponent<Card>().SetValue(-int.Parse(ab.Tag[0]));
                }
            }
        }
    }
}
