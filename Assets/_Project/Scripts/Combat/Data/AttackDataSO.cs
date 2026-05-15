using UnityEngine;

namespace ProjectOni.Combat.Data
{
    public abstract class AttackDataSO : ScriptableObject, IAttackBehavior
    {
        [Header("Common Settings")]
        public string attackName;
        public float baseDamage;
        public float attackCooldown;
        public string animationTrigger;
        public GameObject hitVFXPrefab;
        public AudioClip castSFX;

        // Implementation of IAttackBehavior
        public abstract VisualRequest Execute(AttackContext ctx);
    }
}
