using PurrNet;
using UnityEngine;

namespace ProjectOni.Core
{
    public class LevelComponent : NetworkBehaviour
    {
        /// <summary>
        /// In PurrNet, SyncVar is a wrapper class, not an attribute.
        /// Access value via .value and subscribe via .OnChanged.
        /// </summary>
        public readonly SyncVar<int> level = new(1);

        public int Level => level.value;

        public void SetLevel(int newLevel)
        {
            if (isServer)
                level.value = newLevel;
        }

        public void AddLevel(int amount)
        {
            if (isServer)
                level.value += amount;
        }
    }
}
