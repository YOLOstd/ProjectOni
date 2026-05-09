using UnityEngine;

namespace ProjectOni.Data
{
    public enum ItemType
    {
        Weapon,
        Equipment,
        Material,
        Consumable,
        Special
    }

    /// <summary>
    /// Base class for all ScriptableObject items.
    /// </summary>
    public abstract class ItemData : ScriptableObject
    {
        public string itemName;
        public Sprite icon;
        public ItemType type;
    }
}
