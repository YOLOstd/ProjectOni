using System;

namespace ProjectOni.Data
{
    [Serializable]
    public class StatModifierTrait : IEquipmentTrait
    {
        public int healthBonus;
        public int attackDamageBonus;
        public int defenseBonus;
        public float movementSpeedMultiplier = 1f; // Default to 1 so it doesn't break math
    }
}
