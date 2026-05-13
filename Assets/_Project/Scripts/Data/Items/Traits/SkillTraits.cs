using System;
using UnityEngine;

namespace ProjectOni.Data
{
    [Serializable]
    public class ActiveSkillTrait : IEquipmentTrait
    {
        public string skillName;
        public float cooldownTime;
        public float manaCost;
        
        // Usually, you reference a prefab that contains the actual skill logic (like a fireball)
        // Or a separate ScriptableObject that defines the skill's behavior
        public GameObject skillPrefab; 
    }

    public enum PassiveTrigger { OnHitTaken, OnHitGiven, AlwaysActive, LowHealth }

    [Serializable]
    public class PassiveSkillTrait : IEquipmentTrait
    {
        public PassiveTrigger triggerCondition;
        public float activationChance; // e.g., 0.2f for a 20% chance
        public GameObject passiveEffectPrefab; // e.g., an explosion that happens when hit
    }
}
