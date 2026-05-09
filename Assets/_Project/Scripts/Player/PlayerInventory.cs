using System.Collections.Generic;
using UnityEngine;
using ProjectOni.Data;
using ProjectOni.Core;

namespace ProjectOni.Player
{
    /// <summary>
    /// Acts as the player's "Bag" or repository for unequipped items.
    /// Equipment state is handled by the EquipmentManager.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        [Header("Bag Storage")]
        [SerializeField] private List<ItemData> items = new List<ItemData>();
        [SerializeField] private int maxSlots = 20;

        public List<ItemData> BagItems => items;

        /// <summary>
        /// Adds an item to the bag.
        /// </summary>
        /// <returns>True if the item was added, false if inventory is full.</returns>
        public bool AddToBag(ItemData item)
        {
            if (items.Count >= maxSlots)
            {
                Debug.LogWarning("[PlayerInventory] Bag is full!");
                return false;
            }

            items.Add(item);
            Debug.Log($"[PlayerInventory] Added {item.itemName} to bag.");
            
            // Note: We might want a BagChanged event in the future for UI updates
            return true;
        }

        /// <summary>
        /// Removes an item from the bag.
        /// </summary>
        public void RemoveFromBag(ItemData item)
        {
            if (items.Contains(item))
            {
                items.Remove(item);
                Debug.Log($"[PlayerInventory] Removed {item.itemName} from bag.");
            }
        }

        /// <summary>
        /// Placeholder for finding items by type or criteria.
        /// </summary>
        public List<ItemData> GetItemsOfType(ItemType type)
        {
            return items.FindAll(i => i.type == type);
        }
    }
}
