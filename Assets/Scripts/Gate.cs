using UnityEngine;

namespace LogosTcg
{
    public class NoParams
    {
    }

    public class DropParams
    {
        public SlotScript Source;
        public SlotScript Target;
        public Transform tf;
    }

    public abstract class Gate<T> : Toggleable
    {
        public bool IsUnlocked(T argObject)
        {
            if (!IsActive)
                return true;

            return IsUnlockedInternal(argObject);
        }

        protected abstract bool IsUnlockedInternal(T argObject);
    }
}
