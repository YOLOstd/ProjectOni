using UnityEngine;
using ProjectOni.Data;
using ProjectOni.Core;

namespace ProjectOni.Player
{
    public class PlayerInventory : MonoBehaviour
    {
        [Header("Weapon")]
        public WeaponData currentWeapon;

        [Header("Equipment Slots")]
        public EquipmentData equippedChest;
        public EquipmentData[] equippedRings = new EquipmentData[2];

        private PlayerStats _stats;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
        }

        private void Start()
        {
            // Initial equipment sync if items are pre-assigned in Editor
            if (currentWeapon != null) EquiptItem(currentWeapon);
            if (equippedChest != null) EquiptItem(equippedChest);
            foreach (var ring in equippedRings) if (ring != null) EquiptItem(ring);
        }

        public void EquiptItem(ItemData item)
        {
            // Add item to appropriate slot logic
            switch (item.type)
            {
                case ItemType.Weapon:
                    currentWeapon = item as WeaponData;
                    break;
                case ItemType.Chest:
                    equippedChest = item as EquipmentData;
                    break;
                case ItemType.Ring:
                    // Simple logic to fill first empty or override first slot
                    if (equippedRings[0] == null) equippedRings[0] = item as EquipmentData;
                    else equippedRings[1] = item as EquipmentData;
                    break;
            }

            // Fire event for UI and other systems
            GameEvents.TriggerItemEquipped(item);
            
            // Re-sync stats (if using StatCalculator, though PlayerStats currently handles its own state)
            // _stats.ReCalculateStats(); 
        }
    }
}
