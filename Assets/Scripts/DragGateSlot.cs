using DG.Tweening.Core.Easing;
using UnityEngine;

namespace LogosTcg
{
    public class DragGateSlots : Gate<NoParams>
    {

        protected override bool IsUnlockedInternal(NoParams gateParams)
        {
            if (transform.parent.GetComponent<SlotScript>().draggable)
            {
                return true;
            }

            return false; //unlocked, lets run
        }
    }
}
