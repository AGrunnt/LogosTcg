using DG.Tweening.Core.Easing;
using Unity.Netcode;
using UnityEngine;

namespace LogosTcg
{
    public class DragGatePlayer : Gate<NoParams>
    {

        protected override bool IsUnlockedInternal(NoParams gateParams)
        {
            if (NetworkManager.Singleton == null)
                return true;

            if ((int)NetworkManager.Singleton.LocalClientId == TurnManager.instance.currPlayer)
            {
                return true;
            }

            return false; //unlocked, lets run 
        }
    }
}
