using System;
using UnityEngine;

namespace ProjectOni.Data
{
    public abstract class WeaponTrait : EquipmentTraitSO
    {
        [Header("Weapon Skill Conduit")]
        public int skillLevel = 1;
        public ProjectOni.Combat.WeaponSkill weaponSkill;

        public override string GetDescription()
        {
            if (weaponSkill == null) return "No Weapon Skill Assigned";
            if (weaponSkill.startingNode == null) return $"{weaponSkill.skillName} (Lvl {skillLevel})\nNo attacks assigned.";
            
            float currentDmg = weaponSkill.startingNode.visualData.damage;
            return $"{weaponSkill.skillName} (Lvl {skillLevel})\n" +
                   $"Dmg: {currentDmg}\n" +
                   $"Duration: {weaponSkill.startingNode.totalDuration}s";
        }
    }
}
