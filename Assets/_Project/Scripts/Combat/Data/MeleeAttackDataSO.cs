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

            Vector3 origin = ctx.Position + (Vector3)(ctx.Direction * hitOffset.x) + (Vector3.up * hitOffset.y);
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, swingRadius, ctx.TargetLayer);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(baseDamage);
                }
            }

            return new VisualRequest
            {
                animationTrigger = comboAnimationTriggers.Length > 0 ? comboAnimationTriggers[state.Index] : animationTrigger,
                sfx = castSFX,
                damage = baseDamage,
                hitVFXPrefab = hitVFXPrefab
            };
        }
    }
}
