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

        private Dictionary<GameObject, ComboState> _states = new();

        public override VisualRequest Execute(AttackContext ctx)
        {
            // Get or create state for this caster
            if (!_states.TryGetValue(ctx.Caster, out var state))
            {
                state = new ComboState(comboAnimationTriggers.Length > 0 ? comboAnimationTriggers.Length : 1);
                _states[ctx.Caster] = state;
            }

            state.Advance(comboWindow);

            return new VisualRequest
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
        }
    }
}
