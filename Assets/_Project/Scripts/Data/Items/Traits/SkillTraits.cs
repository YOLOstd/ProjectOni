using System;
using UnityEngine;

namespace ProjectOni.Data
{
    [CreateAssetMenu(fileName = "New Active Skill Trait", menuName = "Project Oni/Traits/Active Skill")]
    public class ActiveSkillTrait : EquipmentTraitSO
    {
        public string skillName;
        public float cooldownTime;
        public float manaCost;
        
        public override string GetDescription()
        {
            return $"Active: {skillName} | CD: {cooldownTime}s | Mana: {manaCost}";
        }
    }

    public enum PassiveTrigger { OnHitTaken, OnHitGiven, AlwaysActive, LowHealth }

    [CreateAssetMenu(fileName = "New Passive Skill Trait", menuName = "Project Oni/Traits/Passive Skill")]
    public class PassiveSkillTrait : EquipmentTraitSO
    {
        public PassiveTrigger triggerCondition;
        public float activationChance; // e.g., 0.2f for a 20% chance
        public GameObject passiveEffectPrefab; // e.g., an explosion that happens when hit

        public override string GetDescription()
        {
            string chanceText = activationChance < 1.0f ? $" ({activationChance * 100}%)" : "";
            return $"Passive: {triggerCondition}{chanceText}";
        }
    }

    [CreateAssetMenu(fileName = "New Spell Trait", menuName = "Project Oni/Traits/Spell Trait")]
    public class SpellTrait : EquipmentTraitSO
    {
        public ProjectOni.Combat.Data.SpellAttackDataSO spellData;

        public override string GetDescription()
        {
            return spellData != null ? $"Spell: {spellData.attackName}" : "Spell: None";
        }
    }
}
