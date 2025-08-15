using DG.Tweening.Core.Easing;
using UnityEngine;

namespace LogosTcg
{
    public class DropPlayerGate : Gate<DropParams>
    {

        protected override bool IsUnlockedInternal(DropParams gateParams)
        {
            if (gateParams.Target.owner == TurnManager.instance.currPlayer)
            {
                return true;
            }

            return false; //unlocked, lets run
        }
    }
}
