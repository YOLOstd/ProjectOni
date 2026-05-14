using ProjectOni.Data;
using ProjectOni.Player;
using System.Collections.Generic;

namespace ProjectOni.Combat
{
    /// <summary>
    /// Utility class to calculate final stats from base values and equipment.
    /// </summary>
    public static class StatCalculator
    {
        public static float CalculateFinalDamage(float baseDamage, IEnumerable<EquipmentInstance> equipmentList)
        {
            return CalculateStat(baseDamage, StatType.Attack, equipmentList);
        }

        public static float CalculateMaxHealth(float baseMaxHealth, IEnumerable<EquipmentInstance> equipmentList)
        {
            return CalculateStat(baseMaxHealth, StatType.Health, equipmentList);
        }

        public static float CalculateStat(float baseValue, StatType type, IEnumerable<EquipmentInstance> equipmentList)
        {
            float total = baseValue;
            if (equipmentList != null)
            {
                foreach (var item in equipmentList)
                {
                    if (!item.IsValid) continue;

                    // Add base stats from blueprint
                    foreach (var stat in item.blueprint.baseStats)
                    {
                        if (stat.type == type) total += stat.value;
                    }

                    // Add traits
                    foreach (var trait in item.GetTraits())
                    {
                        if (trait is StatModifierTrait statModifier)
                        {
                            foreach (var mod in statModifier.modifiers)
                            {
                                if (mod.type == type && !mod.isMultiplier)
                                {
                                    total += mod.value;
                                }
                            }
                        }
                    }
                }

                // Apply multipliers in a second pass if needed, 
                // but for now we'll stick to additive for simplicity 
                // unless we want to implement a specific order of operations.
            }
            return total;
        }
    }
}
