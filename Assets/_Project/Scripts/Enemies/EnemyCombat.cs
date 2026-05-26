using System;
using UnityEngine;
using PurrNet;
using ProjectOni.Combat;
using ProjectOni.Data;
using ProjectOni.Core;

namespace ProjectOni.Enemies
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyCombat : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyDataSO _data;
        [SerializeField] private CombatAnimator _combatAnimator;
        [SerializeField] private LayerMask _targetLayer;

        private EnemyController _enemyController;
        private float _lockEndTime;

        public EnemyDataSO Data => _data;
        public bool IsLocked => Time.time < _lockEndTime;

        private void Awake()
        {
            _enemyController = GetComponent<EnemyController>();
            if (_combatAnimator == null) _combatAnimator = GetComponentInChildren<CombatAnimator>();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();
            
            _lockEndTime = 0f; // Pool reset: clear any combat lock from previous life
            // Subscribe on all clients (Favor the Target logic runs client-side on player machines)
            if (_combatAnimator != null)
            {
                _combatAnimator.OnHitDetected += HandleEnemyHitTarget;
            }
        }

        protected override void OnDespawned(bool asServer)
        {
            base.OnDespawned(asServer);
            if (_combatAnimator != null)
            {
                _combatAnimator.OnHitDetected -= HandleEnemyHitTarget;
            }
        }

        /// <summary>
        /// Execute the enemy's ground melee attack.
        /// Runs on server/host only.
        /// </summary>
        public void ExecuteAttack(Vector2 direction)
        {
            if (!isServer) return;
            if (_data == null || _data.attackTree == null) return;

            AttackContext ctx = new AttackContext
            {
                Caster = gameObject,
                TargetLayer = _targetLayer,
                Direction = direction.normalized,
                Position = transform.position,
                SkillLevel = 1,
                AttackSpeedMultiplier = 1f,
                CastSpeedMultiplier = 1f
            };

            AttackResult result = _data.attackTree.Execute(ctx);

            // Set lock time using scaled GlobalLockTime (Never totalDuration)
            _lockEndTime = Time.time + result.GlobalLockTime;

            // Apply lunge force if configured
            if (result.LungeForce > 0f)
            {
                _enemyController.ApplyRecoil(direction.normalized * result.LungeForce);
            }

            // Replicate visuals on all peers
            RpcPlayVisuals(direction.normalized);
        }

        public void CancelVisuals()
        {
            if (_combatAnimator != null)
            {
                _combatAnimator.CancelVisuals();
            }
        }

        /// <summary>
        /// Applies recoil to this enemy when their attack hits a player.
        /// Can be called by any client (PlayerKnockbackReceiver) to notify host. Gated to host execution.
        /// </summary>
        [ServerRpc(requireOwnership: false)]
        public void ServerApplyRecoil(Vector2 hitDirection)
        {
            if (_data == null) return;
            // Host applies a push back to the enemy's physics controller
            _enemyController.ApplyRecoil(-hitDirection.normalized * _data.recoilForce);
        }

        // ─── Visual Replication ───────────────────────────────────────────────

        [ObserversRpc(runLocally: true, requireServer: false)]
        private void RpcPlayVisuals(Vector2 direction)
        {
            if (_combatAnimator != null && _data != null && _data.attackTree != null)
            {
                _combatAnimator.PlayVisual(_data.attackTree.visualData, direction);
            }
        }

        private void HandleEnemyHitTarget(Hurtbox playerHurtbox, float damage)
        {
            // FAVOR THE TARGET: Only the machine that owns the target player processes it
            if (!playerHurtbox.isOwner) return;

            // Zero-Latency! The client owns this player, meaning they have write-authority 
            // over the ownerAuth: true SyncVar. We just apply the damage directly.
            if (playerHurtbox.Health != null)
            {
                playerHurtbox.Health.TakeDamage(damage);
            }
        }

        /// <summary>
        /// Applies damage to this enemy.
        /// Can be called by any client. Gated to host execution.
        /// </summary>
        [ServerRpc(requireOwnership: false)]
        public void ServerApplyDamage(float damage)
        {
            if (isOwner)
            {
                var health = GetComponent<HealthComponent>();
                if (health != null) health.TakeDamage(damage);
            }
            else if (owner.HasValue)
            {
                TargetApplyDamage(owner.Value, damage);
            }
            else
            {
                // Fallback: If no client owner is assigned, the server is the sole authority
                var health = GetComponent<HealthComponent>();
                if (health != null) health.TakeDamage(damage);
            }
        }

        [TargetRpc]
        private void TargetApplyDamage(PlayerID target, float damage)
        {
            var health = GetComponent<HealthComponent>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }
}
