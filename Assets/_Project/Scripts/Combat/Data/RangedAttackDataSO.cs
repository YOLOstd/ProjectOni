using UnityEngine;

namespace ProjectOni.Combat.Data
{
    [CreateAssetMenu(fileName = "New Ranged Attack", menuName = "Project Oni/Combat/Ranged Attack")]
    public class RangedAttackDataSO : AttackDataSO
    {
        [Header("Ranged Settings")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 10f;
        public Vector2 spawnOffset;

        [Header("Recovery Timing")]
        public float recoveryTime = 0.5f;

        public override (GameObject projectilePrefab, AudioClip sfx, GameObject hitVFXPrefab) GetVisualRefs()
            => (projectilePrefab, castSFX, hitVFXPrefab);

        public override AttackResult Execute(AttackContext ctx)
        {
            float lockTime = recoveryTime / Mathf.Max(0.1f, ctx.AttackSpeedMultiplier);

            var visuals = new VisualRequest
            {
                animationTrigger = animationTrigger,
                sfx = castSFX,
                projectilePrefab = projectilePrefab,
                projectileSpeed = projectileSpeed,
                damage = CalculateDamage(ctx.SkillLevel),
                spawnOffset = spawnOffset,
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
