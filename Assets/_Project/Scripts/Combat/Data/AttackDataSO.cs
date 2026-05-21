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

        /// <summary>
        /// Returns the local Unity asset references used for visuals.
        /// These cannot be serialized over the network. Each client calls this on their own
        /// copy of the ScriptableObject to reconstruct a full VisualRequest after receiving
        /// a NetworkedVisualHint via RPC.
        /// </summary>
        public virtual (GameObject projectilePrefab, AudioClip sfx, GameObject hitVFXPrefab) GetVisualRefs()
            => (null, castSFX, hitVFXPrefab);

        // Implementation of IAttackBehavior
        public abstract AttackResult Execute(AttackContext ctx);
    }
}
