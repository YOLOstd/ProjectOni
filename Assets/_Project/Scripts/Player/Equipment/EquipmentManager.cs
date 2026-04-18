using UnityEngine;
using ProjectOni.Data;
using ProjectOni.Core;

namespace ProjectOni.Player
{
    public class EquipmentManager : MonoBehaviour
    {
        [Header("Weapon Slots")]
        [SerializeField] private ModularEquipmentData primaryWeapon;
        [SerializeField] private ModularEquipmentData secondaryWeapon;

        [Header("Armor Slots")]
        [SerializeField] private ModularEquipmentData helmetSlot;
        [SerializeField] private ModularEquipmentData chestSlot;
        [SerializeField] private ModularEquipmentData bootsSlot;

        [Header("Accessory Slots")]
        [SerializeField] private ModularEquipmentData ring1Slot;
        [SerializeField] private ModularEquipmentData ring2Slot;
        
        [Header("Settings")]
        public bool isUsingPrimary = true;

        private void Start()
        {
            // Initial notification of the active weapon and all slots
            UpdateActiveWeapon();
            NotifyAllSlots();
        }

        private void NotifyAllSlots()
        {
            GameEvents.TriggerEquipmentSlotChanged(EquipmentSlot.WeaponPrimary, primaryWeapon);
            GameEvents.TriggerEquipmentSlotChanged(EquipmentSlot.WeaponSecondary, secondaryWeapon);
            GameEvents.TriggerEquipmentSlotChanged(EquipmentSlot.Helmet, helmetSlot);
            GameEvents.TriggerEquipmentSlotChanged(EquipmentSlot.Chest, chestSlot);
            GameEvents.TriggerEquipmentSlotChanged(EquipmentSlot.Boots, bootsSlot);
            GameEvents.TriggerEquipmentSlotChanged(EquipmentSlot.Ring1, ring1Slot);
            GameEvents.TriggerEquipmentSlotChanged(EquipmentSlot.Ring2, ring2Slot);
        }

        /// <summary>
        /// Equips an item into a specific slot.
        /// </summary>
        public void Equip(ModularEquipmentData item, EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.WeaponPrimary: primaryWeapon = item; break;
                case EquipmentSlot.WeaponSecondary: secondaryWeapon = item; break;
                case EquipmentSlot.Helmet: helmetSlot = item; break;
                case EquipmentSlot.Chest: chestSlot = item; break;
                case EquipmentSlot.Boots: bootsSlot = item; break;
                case EquipmentSlot.Ring1: ring1Slot = item; break;
                case EquipmentSlot.Ring2: ring2Slot = item; break;
            }

            GameEvents.TriggerEquipmentSlotChanged(slot, item);

            if ((slot == EquipmentSlot.WeaponPrimary && isUsingPrimary) || 
                (slot == EquipmentSlot.WeaponSecondary && !isUsingPrimary))
            {
                UpdateActiveWeapon();
            }
        }

        /// <summary>
        /// Swaps between primary and secondary weapon.
        /// Called by InputReader or UI.
        /// </summary>
        public void SwapWeapons()
        {
            isUsingPrimary = !isUsingPrimary;
            UpdateActiveWeapon();
        }

        /// <summary>
        /// Returns the currently active weapon data.
        /// </summary>
        public ModularEquipmentData GetActiveWeapon()
        {
            return isUsingPrimary ? primaryWeapon : secondaryWeapon;
        }

        private void UpdateActiveWeapon()
        {
            ModularEquipmentData active = GetActiveWeapon();
            GameEvents.TriggerWeaponSwapped(active);
            
            if (active != null)
            {
                Debug.Log($"Active weapon is now: {active.itemName}");
            }
        }
    }
}
