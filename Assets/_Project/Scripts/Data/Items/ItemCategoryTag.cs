using UnityEngine;

namespace ProjectOni.Data
{
    /// <summary>
    /// Defines what an item IS (e.g., Weapon, Ring, Armor).
    /// </summary>
    [CreateAssetMenu(fileName = "New Item Category", menuName = "Project Oni/Items/Item Category")]
    public class ItemCategoryTag : ScriptableObject
    {
        public string categoryName;
    }
}
