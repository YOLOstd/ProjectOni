using UnityEngine;
using ProjectOni.Combat;

namespace ProjectOni.Data
{
    /// <summary>
    /// Configuration ScriptableObject for an enemy.
    /// Defines stats, AI parameters, attack behaviors, knockback, and poise values.
    /// </summary>
    [CreateAssetMenu(fileName = "New Enemy Data", menuName = "Project Oni/Enemy/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string displayName;

        [Header("Stats & Skills")]
        public BaseStatsSO baseStats;
        public AttackNode attackTree;

        [Header("AI Parameters")]
        public float aggroRange = 8f;
        public float attackRange = 2f;
        public float patrolRadius = 4f;

        [Header("Combat Physics")]
        [Tooltip("The knockback impulse applied to the PLAYER when hit by this enemy's attack.")]
        public float knockbackForce = 10f;
        [Tooltip("The recoil impulse applied back to the ENEMY when they land an attack.")]
        public float recoilForce = 3f;

        [Header("Poise / Stagger")]
        [Tooltip("Damage burst accumulated within window needed to stagger this enemy.")]
        public float staggerDamageThreshold = 20f;
        [Tooltip("Cooldown in seconds before this enemy can be staggered again.")]
        public float staggerCooldown = 3f;

        [Header("Death")]
        [Tooltip("How long the death animation plays before the enemy despawns.")]
        public float deathAnimationDuration = 1.5f;
    }
}
