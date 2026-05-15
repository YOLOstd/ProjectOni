using UnityEngine;

namespace ProjectOni.Combat.Data
{
    [CreateAssetMenu(fileName = "New Spell Attack", menuName = "Project Oni/Combat/Spell Attack")]
    public class SpellAttackDataSO : AttackDataSO
    {
        [Header("Spell Settings")]
        public GameObject spellProjectilePrefab;
        public float manaCost;
        public float spellSpeed = 10f;
        public Vector2 spellSpawnOffset;

        public override VisualRequest Execute(AttackContext ctx)
        {
            // Here we could handle mana consumption logic if needed
            
            return new VisualRequest
            {
                animationTrigger = animationTrigger,
                sfx = castSFX,
                projectilePrefab = spellProjectilePrefab,
                projectileSpeed = spellSpeed,
                damage = baseDamage,
                spawnOffset = spellSpawnOffset,
                hitVFXPrefab = hitVFXPrefab
            };
        }
    }
}
