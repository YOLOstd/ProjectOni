using UnityEngine;

namespace ProjectOni.Combat
{
    [CreateAssetMenu(fileName = "New Spell Skill", menuName = "Combat/Spell Skill")]
    public class SpellSkill : CombatSkill
    {
        [Header("Spell Specifics")]
        public float manaCost;
        public float cooldown;
    }
}
