using System;
using UnityEngine;

namespace ProjectOni.Data
{
    public abstract class WeaponTrait : EquipmentTraitSO
    {
        [Header("Weapon Skill Conduit")]
        public int skillLevel = 1;
        public ProjectOni.Combat.Data.AttackDataSO attackData;

        public override string GetDescription()
        {
            if (attackData == null) return "No Weapon Skill Assigned";
            
            float currentDmg = attackData.CalculateDamage(skillLevel);
            return $"{attackData.attackName} (Lvl {skillLevel})\n" +
                   $"Dmg: {currentDmg} (Base: {attackData.baseDamage} +{attackData.damageGrowthPerLevel}/lvl)\n" +
                   $"CD: {attackData.attackCooldown}s | KB: {attackData.knockbackForce}";
        }
    }
}
