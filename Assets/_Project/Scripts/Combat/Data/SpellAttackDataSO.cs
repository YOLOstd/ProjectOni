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

        [Header("Recovery Timing")]
        public float recoveryTime = 0.5f;

        public override (GameObject projectilePrefab, AudioClip sfx, GameObject hitVFXPrefab) GetVisualRefs()
            => (spellProjectilePrefab, castSFX, hitVFXPrefab);

        public override AttackResult Execute(AttackContext ctx)
        {
            // Here we could handle mana consumption logic if needed
            
            float lockTime = recoveryTime / Mathf.Max(0.1f, ctx.CastSpeedMultiplier);

            var visuals = new VisualRequest
            {
                animationTrigger = animationTrigger,
                sfx = castSFX,
                projectilePrefab = spellProjectilePrefab,
                projectileSpeed = spellSpeed,
                damage = CalculateDamage(ctx.SkillLevel),
                spawnOffset = spellSpawnOffset,
                hitVFXPrefab = hitVFXPrefab
            };

            return new AttackResult
            {
                Success = true,
                GlobalLockTime = lockTime,
                Visuals = visuals
            };
        }
    }
}
