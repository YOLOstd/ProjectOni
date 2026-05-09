using System.Collections.Generic;
using UnityEngine;
using ProjectOni.Data;
using ProjectOni.Core;

namespace ProjectOni.Player
{
    /// <summary>
    /// The absolute source of truth for what the player is currently wearing/using.
    /// Handles equipment using a decoupled Slot Definition and Item Category system.
    /// </summary>
    public class EquipmentManager : MonoBehaviour
    {
        [Header("Slot Configuration")]
        [Tooltip("Define all physical equipment slots for this character here.")]
        [SerializeField] private List<EquipmentSlotDefinition> allGameSlots; 
        
        [Header("Weapon Swapping Settings")]
        [SerializeField] private EquipmentSlotDefinition primaryWeaponSlot;
        [SerializeField] private EquipmentSlotDefinition secondaryWeaponSlot;

        [Header("State")]
        public bool isUsingPrimary = true;

        // Internal source of truth
        private Dictionary<EquipmentSlotDefinition, ModularEquipmentData> currentEquipment = new Dictionary<EquipmentSlotDefinition, ModularEquipmentData>();

        private void Awake()
        {
            // Initialize the dictionary based on defined slots
            foreach (var slot in allGameSlots)
            {
                if (slot != null && !currentEquipment.ContainsKey(slot))
                {
                    currentEquipment[slot] = null;
                }
            }
        }

        private void Start()
        {
            // Initial notification to sync UI and systems
            NotifyAllSlots();
            UpdateActiveWeapon();
        }

        private void NotifyAllSlots()
        {
            foreach (var slot in currentEquipment.Keys)
            {
                GameEvents.TriggerEquipmentSlotChanged(slot, currentEquipment[slot]);
            }
        }

        /// <summary>
        /// Equips an item into a specific slot if it's compatible.
        /// Returns the item that was previously in that slot (if any).
        /// </summary>
        public ModularEquipmentData Equip(ModularEquipmentData item, EquipmentSlotDefinition slot)
        {
            if (slot == null) return null;

            // Safety check: Is this even a valid slot on this manager?
            if (!currentEquipment.ContainsKey(slot))
            {
                Debug.LogWarning($"[EquipmentManager] {slot.slotName} is not a registered slot on this character!");
                return null;
            }

            // Decoupled Check: Can the item fit in this slot?
            if (item != null)
            {
                if (item.category == null || !slot.acceptedCategories.Contains(item.category))
                {
                    Debug.LogWarning($"[EquipmentManager] Cannot equip {item.itemName} in {slot.slotName}! " +
                                     $"Slot only accepts: {string.Join(", ", slot.acceptedCategories.ConvertAll(c => c.categoryName))}");
                    return null;
                }
            }

            ModularEquipmentData oldItem = currentEquipment[slot];
            currentEquipment[slot] = item;

            // Notify systems
            GameEvents.TriggerEquipmentSlotChanged(slot, item);

            // If we updated a weapon slot that is currently active, refresh visuals/logic
            if ((slot == primaryWeaponSlot && isUsingPrimary) || (slot == secondaryWeaponSlot && !isUsingPrimary))
            {
                UpdateActiveWeapon();
            }

            return oldItem;
        }

        /// <summary>
        /// Attempts to equip an item into the first available compatible slot.
        /// </summary>
        public bool EquipToFirstCompatibleSlot(ModularEquipmentData item)
        {
            if (item == null || item.category == null) return false;

            // 1. Try to find an empty compatible slot
            foreach (var slot in allGameSlots)
            {
                if (IsSlotEmpty(slot) && slot.acceptedCategories.Contains(item.category))
                {
                    Equip(item, slot);
                    return true;
                }
            }

            // 2. Optional: If no empty slot, could replace the first compatible one? 
            // For now, we'll just return false and let the UI handle explicit replacement.
            return false;
        }

        public void SwapWeapons()
        {
            isUsingPrimary = !isUsingPrimary;
            UpdateActiveWeapon();
        }

        public ModularEquipmentData GetActiveWeapon()
        {
            EquipmentSlotDefinition activeSlot = isUsingPrimary ? primaryWeaponSlot : secondaryWeaponSlot;
            if (activeSlot == null) return null;
            
            return currentEquipment.ContainsKey(activeSlot) ? currentEquipment[activeSlot] : null;
        }

        private void UpdateActiveWeapon()
        {
            ModularEquipmentData active = GetActiveWeapon();
            GameEvents.TriggerWeaponSwapped(active);
            
            if (active != null)
                Debug.Log($"[EquipmentManager] Active weapon is now: {active.itemName}");
        }

        /// <summary>
        /// Check if a specific slot is currently empty.
        /// </summary>
        public bool IsSlotEmpty(EquipmentSlotDefinition slot)
        {
            if (slot == null) return false;
            return currentEquipment.ContainsKey(slot) && currentEquipment[slot] == null;
        }
    }
}
