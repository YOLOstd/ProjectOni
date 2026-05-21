using UnityEngine;

namespace ProjectOni.Combat
{
    public abstract class CombatSkill : ScriptableObject
    {
        [Header("Identity & UI")]
        public string skillName;
        public Sprite skillIcon;

        [Header("Combo Root")]
        [Tooltip("The starting attack node of the combo tree for this skill.")]
        public AttackNode startingNode;

        [Header("Tags")]
        [Tooltip("Used by items and passives to check if they should buff this skill.")]
        public SkillTag skillTags;
    }
}
