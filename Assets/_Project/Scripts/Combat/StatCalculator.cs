using ProjectOni.Data;
using ProjectOni.Player;

namespace ProjectOni.Combat
{
    /// <summary>
    /// Utility class to calculate final stats from base values and equipment.
    /// </summary>
    public static class StatCalculator
    {
        public static float CalculateFinalDamage(float baseDamage, System.Collections.Generic.IEnumerable<ModularEquipmentData> equipmentList)
        {
            float flatBonus = 0;
            if (equipmentList != null)
            {
                foreach (var item in equipmentList)
                {
                    if (item == null) continue;
                    var trait = item.GetTrait<StatModifierTrait>();
                    if (trait != null) flatBonus += trait.attackDamageBonus;
                }
            }
            return baseDamage + flatBonus;
        }

        public static float CalculateMaxHealth(float baseMaxHealth, System.Collections.Generic.IEnumerable<ModularEquipmentData> equipmentList)
        {
            float bonus = 0;
            if (equipmentList != null)
            {
                foreach (var item in equipmentList)
                {
                    if (item == null) continue;
                    var trait = item.GetTrait<StatModifierTrait>();
                    if (trait != null) bonus += trait.healthBonus;
                }
            }
            return baseMaxHealth + bonus;
        }
    }
}
