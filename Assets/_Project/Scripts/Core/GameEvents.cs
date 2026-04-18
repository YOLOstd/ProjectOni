using System;
using ProjectOni.Data;

namespace ProjectOni.Core
{
    /// <summary>
    /// Central event hub for the game.
    /// Handles decoupling of gameplay logic and UI/Systems.
    /// </summary>
    public static class GameEvents
    {
        // Player events
        public static Action<float, float> OnPlayerHealthChanged; // Current, Max
        public static Action<ItemData> OnItemEquipped;
        public static Action<ModularEquipmentData> OnWeaponSwapped;
        public static Action<EquipmentSlot, ModularEquipmentData> OnEquipmentSlotChanged;
        
        // Enemy/Boss events
        public static Action OnBossDefeated;
        
        // Helper methods for firing events safely
        public static void TriggerPlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);
        public static void TriggerItemEquipped(ItemData item) => OnItemEquipped?.Invoke(item);
        public static void TriggerWeaponSwapped(ModularEquipmentData activeWeapon) => OnWeaponSwapped?.Invoke(activeWeapon);
        public static void TriggerEquipmentSlotChanged(EquipmentSlot slot, ModularEquipmentData item) => OnEquipmentSlotChanged?.Invoke(slot, item);
        public static void TriggerBossDefeated() => OnBossDefeated?.Invoke();
    }
}
