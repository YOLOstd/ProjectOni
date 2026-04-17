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
        
        // Enemy/Boss events
        public static Action OnBossDefeated;
        
        // Helper methods for firing events safely
        public static void TriggerPlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);
        public static void TriggerItemEquipped(ItemData item) => OnItemEquipped?.Invoke(item);
        public static void TriggerBossDefeated() => OnBossDefeated?.Invoke();
    }
}
