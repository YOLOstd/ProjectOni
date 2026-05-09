using System.Collections.Generic;
using UnityEngine;

namespace ProjectOni.Data
{
    /// <summary>
    /// Defines a physical equipment slot and what item categories it can accept.
    /// </summary>
    [CreateAssetMenu(fileName = "New Slot Definition", menuName = "Project Oni/Items/Slot Definition")]
    public class EquipmentSlotDefinition : ScriptableObject
    {
        public string slotName;
        public List<ItemCategoryTag> acceptedCategories;
        public Sprite defaultIcon;
    }
}
