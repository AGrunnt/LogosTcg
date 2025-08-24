using DG.Tweening.Core.Easing;
using System.Collections.Generic;
using UnityEngine;

namespace LogosTcg
{
    public class DragPhaseGate : Gate<NoParams>
    {
        TurnManager tm;
        public List<string> phases;

        private void Awake()
        {
            tm = TurnManager.instance;
        }
        protected override bool IsUnlockedInternal(NoParams gateParams)
        {
            if (phases.Contains(tm.currPhase))
            {
                return true;
            }

            return false; //unlocked, lets run
        }
    }
}
