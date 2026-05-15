using System;
using ProjectOni.Data;
using ProjectOni.Player;

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
        public static Action<EquipmentInstance> OnWeaponSwapped;
        public static Action<EquipmentManager, EquipmentSlotDefinition, EquipmentInstance> OnEquipmentSlotChanged;
        
        // Enemy/Boss events
        public static Action OnBossDefeated;
        
        // Helper methods for firing events safely
        public static void TriggerPlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);
        public static void TriggerWeaponSwapped(EquipmentInstance activeWeapon) => OnWeaponSwapped?.Invoke(activeWeapon);
        public static void TriggerEquipmentSlotChanged(EquipmentManager manager, EquipmentSlotDefinition slot, EquipmentInstance item) => OnEquipmentSlotChanged?.Invoke(manager, slot, item);
        public static void TriggerBossDefeated() => OnBossDefeated?.Invoke();
    }
}
