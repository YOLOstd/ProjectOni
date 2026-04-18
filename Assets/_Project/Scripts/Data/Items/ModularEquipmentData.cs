using System.Collections.Generic;
using UnityEngine;

namespace ProjectOni.Data
{
    [CreateAssetMenu(fileName = "New Modular Item", menuName = "Project Oni/Items/Modular Item")]
    public class ModularEquipmentData : ItemData
    {
        public EquipmentSlot allowedSlot; // The UI checks this before allowing an equip

        [SerializeReference] 
        public List<IEquipmentTrait> traits = new List<IEquipmentTrait>();

        /// <summary>
        /// Returns the first trait of type T if it exists on this item.
        /// </summary>
        public T GetTrait<T>() where T : class, IEquipmentTrait
        {
            foreach (var trait in traits)
            {
                if (trait is T match) return match;
            }
            return null;
        }

        /// <summary>
        /// Returns true if the item has a specific trait.
        /// </summary>
        public bool HasTrait<T>() where T : class, IEquipmentTrait
        {
            return GetTrait<T>() != null;
        }
    }
}
