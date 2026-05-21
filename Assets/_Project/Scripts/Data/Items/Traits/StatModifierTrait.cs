using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOni.Data
{
    [Serializable]
    public struct TraitStatModifier
    {
        public StatType type;
        public float value;
        public bool isMultiplier; // True for multipliers (e.g. 1.1 for +10%), false for additive
    }

    [CreateAssetMenu(fileName = "New Stat Modifier Trait", menuName = "Project Oni/Traits/Stat Modifier")]
    public class StatModifierTrait : EquipmentTraitSO
    {
        public List<TraitStatModifier> modifiers = new List<TraitStatModifier>();

        public float GetBonus(StatType type, bool multiplier)
        {
            float total = multiplier ? 1f : 0f;
            foreach (var mod in modifiers)
            {
                if (mod.type == type && mod.isMultiplier == multiplier)
                {
                    if (multiplier) total *= mod.value;
                    else total += mod.value;
                }
            }
            return total;
        }
        public override string GetDescription()
        {
            if (modifiers == null || modifiers.Count == 0) return "No modifiers.";
            
            var lines = new System.Collections.Generic.List<string>();
            foreach (var mod in modifiers)
            {
                string sign = mod.value >= 0 ? "+" : "";
                string type = mod.isMultiplier ? "%" : "";
                lines.Add($"{sign}{mod.value}{type} {mod.type}");
            }
            return string.Join("\n", lines);
        }
    }
}
