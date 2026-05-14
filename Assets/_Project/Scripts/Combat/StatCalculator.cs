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
            float additiveTotal = 0;
            float multiplierTotal = 0;

            if (equipmentList != null)
            {
                foreach (var item in equipmentList)
                {
                    if (!item.IsValid) continue;

                    // 1. Process Blueprint Base Stats
                    foreach (var stat in item.blueprint.baseStats)
                    {
                        if (stat.type == type)
                        {
                            if (stat.isMultiplier) multiplierTotal += stat.value;
                            else additiveTotal += stat.value;
                        }
                    }

                    // 2. Process Traits
                    foreach (var trait in item.GetTraits())
                    {
                        if (trait is StatModifierTrait statModifier)
                        {
                            foreach (var mod in statModifier.modifiers)
                            {
                                if (mod.type == type)
                                {
                                    if (mod.isMultiplier) multiplierTotal += mod.value;
                                    else additiveTotal += mod.value;
                                }
                            }
                        }
                    }
                }
            }

            // Final Formula: (Base + Flat Additions) * (1 + Sum of Multipliers)
            // Multipliers are treated as percentage additions (e.g. 0.1 = +10%)
            return (baseValue + additiveTotal) * (1 + multiplierTotal);
        }
    }
}
