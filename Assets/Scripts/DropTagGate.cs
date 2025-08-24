using DG.Tweening.Core.Easing;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LogosTcg
{
    public class DropTagGate : Gate<DropParams>
    {
        public List<string> tags;

        protected override bool IsUnlockedInternal(DropParams gateParams)
        {
            if (tags.Contains(gateParams.tf.gameObject.tag))
            {
                return true;
            }

            return false; //unlocked, lets run
        }
    }
}
