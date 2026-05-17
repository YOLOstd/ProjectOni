using UnityEngine;

namespace ProjectOni.Combat.Data
{
    public abstract class AttackDataSO : ScriptableObject, IAttackBehavior
    {
        [Header("Common Settings")]
        public string attackName;
        public float baseDamage;
        public float damageGrowthPerLevel = 2f;
        public float attackCooldown = 0.5f;
        public float knockbackForce = 5f;
        public string animationTrigger;
        public Sprite skillIcon;
        public GameObject hitVFXPrefab;
        public AudioClip castSFX;

        public float CalculateDamage(int level)
        {
            return baseDamage + Mathf.Max(0, level - 1) * damageGrowthPerLevel;
        }

        // Implementation of IAttackBehavior
        public abstract VisualRequest Execute(AttackContext ctx);
    }
}
