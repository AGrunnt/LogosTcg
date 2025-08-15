using UnityEngine;

namespace LogosTcg
{
    public class Toggleable : MonoBehaviour
    {
        public bool IsActive = true;

        public void SetIsActive(bool newValue)
        {
            IsActive = newValue;
        }
    }
}
