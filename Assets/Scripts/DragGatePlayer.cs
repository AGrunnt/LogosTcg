using DG.Tweening.Core.Easing;
using UnityEngine;

namespace LogosTcg
{
    public class DragGatePlayer : Gate<NoParams>
    {

        protected override bool IsUnlockedInternal(NoParams gateParams)
        {
            if (true)
            {
                return false;
            }

            return true; //unlocked, lets run 
        }
    }
}
