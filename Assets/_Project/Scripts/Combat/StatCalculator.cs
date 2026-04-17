using ProjectOni.Data;
using ProjectOni.Player;

namespace ProjectOni.Combat
{
    /// <summary>
    /// Utility class to calculate final stats from base values and equipment.
    /// </summary>
    public static class StatCalculator
    {
        public static float CalculateFinalDamage(float baseDamage, WeaponData weapon, EquipmentData[] accessories)
        {
            float weaponDamage = weapon != null ? weapon.baseDamage : 0;
            float equipmentBonus = 0;

            if (accessories != null)
            {
                foreach (var equipment in accessories)
                {
                    if (equipment != null) equipmentBonus += equipment.damageBonus;
                }
            }

            return baseDamage + weaponDamage + equipmentBonus;
        }

        public static float CalculateMaxHealth(float baseMaxHealth, EquipmentData chest, EquipmentData[] rings)
        {
            float bonus = 0;
            if (chest != null) bonus += chest.healthBonus;
            if (rings != null)
            {
                foreach (var ring in rings)
                {
                    if (ring != null) bonus += ring.healthBonus;
                }
            }

            return baseMaxHealth + bonus;
        }
    }
}
