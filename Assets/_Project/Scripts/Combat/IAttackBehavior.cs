using UnityEngine;

namespace ProjectOni.Combat
{
    public enum ActionSlot
    {
        Primary,
        Secondary,
        Spell1,
        Spell2
    }

    [System.Serializable]
    public struct VisualRequest
    {
        public string animationTrigger;
        public AudioClip sfx;
        public GameObject projectilePrefab;
        public float projectileSpeed;
        public float damage;
        public Vector2 spawnOffset;
        public GameObject hitVFXPrefab;
        public float lifetime;
        
        public float hitboxStartTime;
        public float hitboxDuration;

        public static VisualRequest Default => new VisualRequest();
    }

    /// <summary>
    /// A network-safe subset of VisualRequest. Only contains serializable fields (no Unity asset refs).
    /// Each client reconstructs the full VisualRequest locally by resolving prefab/audio refs from
    /// their own copy of the weapon ScriptableObject data.
    /// </summary>
    [System.Serializable]
    public struct NetworkedVisualHint
    {
        public string animationTrigger;
        public float damage;
        public Vector2 spawnOffset;
        public float projectileSpeed;
        public float lifetime;
        public float hitboxStartTime;
        public float hitboxDuration;
    }

    public struct AttackContext
    {
        public GameObject Caster;
        public LayerMask TargetLayer;
        public Vector2 Direction;
        public Vector3 Position;
        public int SkillLevel;
        public float AttackSpeedMultiplier;
        public float CastSpeedMultiplier;
    }

    public struct AttackResult
    {
        public bool Success;
        public float GlobalLockTime;
        public float AntiGravityTime;
        public float LungeForce;
        public VisualRequest Visuals;
    }

    public interface IAttackBehavior
    {
        AttackResult Execute(AttackContext ctx);
    }
}
