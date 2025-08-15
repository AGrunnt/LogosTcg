using DG.Tweening.Core.Easing;
using System.Linq;
using UnityEngine;

namespace LogosTcg
{
    public class DropCountGate : Gate<DropParams>
    {
        

        protected override bool IsUnlockedInternal(DropParams gateParams)
        {
            if (gateParams.Target.maxChildrenCards > gateParams.Target.GetComponentsInChildren<Card>().Count())
            {
                return true;
            }
            
            return false; //unlocked, lets run
        }
    }
}
