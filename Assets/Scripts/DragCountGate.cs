using DG.Tweening.Core.Easing;
using UnityEngine;

namespace LogosTcg
{
    public class DragCountGate : Gate<NoParams>
    {

        protected override bool IsUnlockedInternal(NoParams gateParams)
        {
            if (TurnManager.instance.playCount < TurnManager.instance.playCountAvailable)
            {
                return true;
            }

            return false; //unlocked, lets run
        }
    }
}
