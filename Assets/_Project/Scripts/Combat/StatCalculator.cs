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

        public static float CalculateFinalDamage(float baseDamage, ModularEquipmentData mainWeapon, ModularEquipmentData otherEquipment)
        {
            float bonus = 0;
            
            // Extract from main weapon
            if (mainWeapon != null)
            {
                var trait = mainWeapon.GetTrait<StatModifierTrait>();
                if (trait != null) bonus += trait.attackDamageBonus;
            }

            // Extract from other equipment (this needs to be call per slot or in a list)
            // For now, let's just make a generic version that takes a list of modular data
            return baseDamage + bonus;
        }

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
