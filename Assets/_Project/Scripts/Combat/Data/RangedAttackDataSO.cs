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

        public override VisualRequest Execute(AttackContext ctx)
        {
            return new VisualRequest
            {
                animationTrigger = animationTrigger,
                sfx = castSFX,
                projectilePrefab = projectilePrefab,
                projectileSpeed = projectileSpeed,
                damage = CalculateDamage(ctx.SkillLevel),
                spawnOffset = spawnOffset,
                hitVFXPrefab = hitVFXPrefab
            };
        }
    }
}
