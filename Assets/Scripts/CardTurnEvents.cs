using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace LogosTcg
{
    public class CardTurnEvents : MonoBehaviour
    {

        public UnityEvent OnTurnEnd;

        /*
        void OnEnable()
        {
            TrySubscribe();
        }
        */
        void OnDisable()
        {
            if (TurnManager.instance != null)
                TurnManager.instance.OnEndTurn -= HandleEndTurn;
        }

        public void TrySubscribeOnEndTurn()
        {
            if (TurnManager.instance != null)
            {
                TurnManager.instance.OnEndTurn += HandleEndTurn;
            }
            else
            {
                // If the card spawns before TurnManager is ready, wait a frame loop
                StartCoroutine(WaitForTMThenSubscribe());
            }
        }

        private IEnumerator WaitForTMThenSubscribe()
        {
            while (TurnManager.instance == null) yield return null;
            TurnManager.instance.OnEndTurn += HandleEndTurn;
        }

        private void HandleEndTurn()
        {
            OnTurnEnd?.Invoke();
        }


        public void AddOneTurn()
        {
            GetComponent<Card>().turnsInPlay = GetComponent<Card>().turnsInPlay + 1;
        }


        
    }
}
