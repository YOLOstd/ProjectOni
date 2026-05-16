using System.Collections.Generic;
using UnityEngine;
using ProjectOni.Data;
using ProjectOni.Core;
using PurrNet;

namespace ProjectOni.Player
{
    /// <summary>
    /// The absolute source of truth for what the player is currently wearing/using.
    /// Handles equipment using a decoupled Slot Definition and Item Category system.
    /// </summary>
    public class EquipmentManager : NetworkBehaviour
    {
        [Header("Slot Configuration")]
        [Tooltip("Define all physical equipment slots for this character here.")]
        [SerializeField] private List<EquipmentSlotDefinition> allGameSlots; 
        
        [Header("Weapon Swapping Settings")]
        [SerializeField] private EquipmentSlotDefinition primaryWeaponSlot;
        [SerializeField] private EquipmentSlotDefinition secondaryWeaponSlot;

        [Header("State")]
        public readonly SyncVar<bool> isUsingPrimary = new(true, 0f, true);
        public readonly SyncVar<EquipmentInstance> activeWeaponVisual = new(default, 0f, true);

        // Internal source of truth
        private Dictionary<EquipmentSlotDefinition, EquipmentInstance> currentEquipment = new Dictionary<EquipmentSlotDefinition, EquipmentInstance>();

        private void Awake()
        {
            // Initialize the dictionary based on defined slots
            foreach (var slot in allGameSlots)
            {
                if (slot != null && !currentEquipment.ContainsKey(slot))
                {
                    currentEquipment[slot] = default;
                }
            }
        }

        protected override void OnSpawned()
        {
            activeWeaponVisual.onChangedWithOld += OnActiveWeaponVisualChanged;

            if (isOwner)
            {
                // Initial notification to sync local UI and systems
                NotifyAllSlots();
                UpdateActiveWeapon();
            }
            else
            {
                // Ensure observers see the current weapon on spawn
                OnActiveWeaponVisualChanged(default, activeWeaponVisual.value);
                
                // For non-owner proxies, we still want to fire the slot changed event
                // so that nameplates or other systems can react if they want, 
                // but primarily for the visual update.
                // However, since activeWeaponVisual handles the visual, 
                // we don't necessarily need to NotifyAllSlots for proxies unless 
                // there's proxy-specific UI.
            }
        }

        protected override void OnDespawned(bool asServer)
        {
            activeWeaponVisual.onChangedWithOld -= OnActiveWeaponVisualChanged;
        }

        private void OnActiveWeaponVisualChanged(EquipmentInstance oldVal, EquipmentInstance newVal)
        {
            GameEvents.TriggerWeaponSwapped(newVal);
        }

        private void NotifyAllSlots()
        {
            foreach (var slot in currentEquipment.Keys)
            {
                GameEvents.TriggerEquipmentSlotChanged(this, slot, currentEquipment[slot]);
            }
        }

        /// <summary>
        /// Equips an item into a specific slot if it's compatible.
        /// Returns the item that was previously in that slot (if any).
        /// </summary>
        public EquipmentInstance Equip(EquipmentInstance item, EquipmentSlotDefinition slot)
        {
            if (!isOwner && !isServer) return default;
            if (slot == null) return default;

            // Safety check: Is this even a valid slot on this manager?
            if (!currentEquipment.ContainsKey(slot))
            {
                Debug.LogWarning($"[EquipmentManager] {slot.slotName} is not a registered slot on this character!");
                return default;
            }

            // Decoupled Check: Can the item fit in this slot?
            if (item.IsValid)
            {
                if (item.blueprint.category == null || !slot.acceptedCategories.Contains(item.blueprint.category))
                {
                    Debug.LogWarning($"[EquipmentManager] Cannot equip {item.blueprint.itemName} in {slot.slotName}! " +
                                     $"Slot only accepts: {string.Join(", ", slot.acceptedCategories.ConvertAll(c => c.categoryName))}");
                    return default;
                }
            }

            EquipmentInstance oldItem = currentEquipment[slot];
            currentEquipment[slot] = item;

            // Notify local systems
            Debug.Log($"[EquipmentManager] Equipping {item.blueprint.itemName} to slot: {slot.slotName} (isOwner: {isOwner}, isServer: {isServer})");
            GameEvents.TriggerEquipmentSlotChanged(this, slot, item);

            // If we are on the server, notify the clients (especially the owner)
            if (isServer)
            {
                int slotIndex = allGameSlots.IndexOf(slot);
                if (slotIndex >= 0)
                {
                    RpcNotifySlotChanged(slotIndex, item);
                }
            }

            // If we updated a weapon slot that is currently active, refresh visuals/logic
            if ((slot == primaryWeaponSlot && isUsingPrimary.value) || (slot == secondaryWeaponSlot && !isUsingPrimary.value))
            {
                UpdateActiveWeapon();
            }

            return oldItem;
        }

        [ObserversRpc]
        private void RpcNotifySlotChanged(int slotIndex, EquipmentInstance item)
        {
            // The server already processed this, skip to avoid double processing/events
            if (isServer) return;

            if (slotIndex < 0 || slotIndex >= allGameSlots.Count) return;
            var slot = allGameSlots[slotIndex];

            // Update local state
            if (!currentEquipment.ContainsKey(slot))
                currentEquipment[slot] = item;
            else
                currentEquipment[slot] = item;

            // Fire local event for UI
            GameEvents.TriggerEquipmentSlotChanged(this, slot, item);
        }

        /// <summary>
        /// Attempts to equip an item into the first available compatible slot.
        /// </summary>
        public bool EquipToFirstCompatibleSlot(EquipmentInstance item)
        {
            if (!item.IsValid || item.blueprint.category == null) return false;

            // 1. Try to find an empty compatible slot
            foreach (var slot in allGameSlots)
            {
                if ((isOwner || isServer) && IsSlotEmpty(slot) && slot.acceptedCategories.Contains(item.blueprint.category))
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
            if (!isOwner) return;
            isUsingPrimary.value = !isUsingPrimary.value;
            UpdateActiveWeapon();
        }

        public EquipmentInstance GetActiveWeapon()
        {
            // Observers use the synced visual data since they don't have the full dictionary
            if (!isOwner && isSpawned) return activeWeaponVisual.value;

            EquipmentSlotDefinition activeSlot = isUsingPrimary.value ? primaryWeaponSlot : secondaryWeaponSlot;
            if (activeSlot == null) return default;
            
            return currentEquipment.ContainsKey(activeSlot) ? currentEquipment[activeSlot] : default;
        }

        private void UpdateActiveWeapon()
        {
            EquipmentInstance active = GetActiveWeapon();
            activeWeaponVisual.value = active;
            
            if (active.IsValid && isOwner)
                Debug.Log($"[EquipmentManager] Active weapon is now: {active.blueprint.itemName}");
        }

        /// <summary>
        /// Check if a specific slot is currently empty.
        /// </summary>
        public bool IsSlotEmpty(EquipmentSlotDefinition slot)
        {
            if (slot == null) return false;
            return currentEquipment.ContainsKey(slot) && !currentEquipment[slot].IsValid;
        }

        public EquipmentInstance GetItemInSlot(EquipmentSlotDefinition slot)
        {
            if (slot == null) return default;
            return currentEquipment.TryGetValue(slot, out var item) ? item : default;
        }

        /// <summary>Returns all currently equipped items (for stat recalculation).</summary>
        public IEnumerable<EquipmentInstance> GetAllEquipped() => currentEquipment.Values;
    }
}
