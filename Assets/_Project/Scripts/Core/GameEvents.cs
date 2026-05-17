using System;
using ProjectOni.Data;
using ProjectOni.Player;

namespace ProjectOni.Core
{
    /// <summary>
    /// Central event hub for the game.
    /// Handles decoupling of gameplay logic and UI/Systems.
    /// 
    /// Note: Health UI no longer uses a global health event.
    ///       Subscribe to HealthComponent.OnHealthChanged on the local player instead.
    /// </summary>
    public static class GameEvents
    {
        // Player events
        public static Action<EquipmentManager, EquipmentInstance> OnWeaponSwapped;
        public static Action<EquipmentManager, EquipmentSlotDefinition, EquipmentInstance> OnEquipmentSlotChanged;

        /// <summary>Fired after StatController finishes a full recalculation.</summary>
        public static Action<StatController> OnStatsRecalculated;

        // Enemy/Boss events
        public static Action OnBossDefeated;

        // Helpers
        public static void TriggerWeaponSwapped(EquipmentManager manager, EquipmentInstance activeWeapon) => OnWeaponSwapped?.Invoke(manager, activeWeapon);
        public static void TriggerEquipmentSlotChanged(EquipmentManager manager, EquipmentSlotDefinition slot, EquipmentInstance item) => OnEquipmentSlotChanged?.Invoke(manager, slot, item);
        public static void TriggerStatsRecalculated(StatController stats) => OnStatsRecalculated?.Invoke(stats);
        public static void TriggerBossDefeated() => OnBossDefeated?.Invoke();
    }
}
