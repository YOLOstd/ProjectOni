using System.Collections.Generic;
using UnityEngine;
using ProjectOni.Core;

namespace ProjectOni.Combat.Data
{
    [CreateAssetMenu(fileName = "New Melee Attack", menuName = "Project Oni/Combat/Melee Attack")]
    public class MeleeAttackDataSO : AttackDataSO
    {
        [Header("Melee Settings")]
        public float swingRadius = 1.5f;
        public Vector2 hitOffset;
        public float comboWindow = 1.0f;
        public string[] comboAnimationTriggers;

        [Header("Slash Prefab Settings")]
        public GameObject slashPrefab;
        public float slashDuration = 0.2f;

        [Header("Hitbox Timing")]
        public float hitboxStartTime = 0.05f;
        public float hitboxDuration = 0.1f;

        [Header("Recovery Timing")]
        public float[] comboRecoveryTimes;
        public float antiGravityTime = 0.15f;

        [Header("Movement")]
        [Tooltip("Forward push force applied per combo hit")]
        public float[] comboLungeForces;

        private Dictionary<GameObject, ComboState> _states = new();

        public override AttackResult Execute(AttackContext ctx)
        {
            // Get or create state for this caster
            if (!_states.TryGetValue(ctx.Caster, out var state))
            {
                state = new ComboState(comboAnimationTriggers.Length > 0 ? comboAnimationTriggers.Length : 1);
                _states[ctx.Caster] = state;
            }

            state.Advance(comboWindow);

            float baseRecovery = 0.5f;
            if (comboRecoveryTimes != null && comboRecoveryTimes.Length > 0)
            {
                baseRecovery = comboRecoveryTimes[Mathf.Clamp(state.Index, 0, comboRecoveryTimes.Length - 1)];
            }
            float lockTime = baseRecovery / Mathf.Max(0.1f, ctx.AttackSpeedMultiplier);

            var visuals = new VisualRequest
            {
                animationTrigger = comboAnimationTriggers.Length > 0 ? comboAnimationTriggers[state.Index] : animationTrigger,
                sfx = castSFX,
                projectilePrefab = slashPrefab,
                projectileSpeed = 0f,
                damage = CalculateDamage(ctx.SkillLevel),
                spawnOffset = hitOffset,
                hitVFXPrefab = hitVFXPrefab,
                lifetime = slashDuration,
                hitboxStartTime = hitboxStartTime,
                hitboxDuration = hitboxDuration
            };

            float lungeForce = 0f;
            if (comboLungeForces != null && comboLungeForces.Length > 0)
            {
                lungeForce = comboLungeForces[Mathf.Clamp(state.Index, 0, comboLungeForces.Length - 1)];
            }

            return new AttackResult
            {
                Success = true,
                GlobalLockTime = lockTime,
                AntiGravityTime = antiGravityTime,
                LungeForce = lungeForce,
                Visuals = visuals
            };
        }
    }
}
