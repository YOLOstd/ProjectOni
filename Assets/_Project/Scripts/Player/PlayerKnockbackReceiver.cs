using System;
using UnityEngine;
using PurrNet;
using ProjectOni.Combat;
using ProjectOni.Enemies;

namespace ProjectOni.Player
{
    /// <summary>
    /// Companion component to Hurtbox on the Player prefab.
    /// Handles the "Favor the Target" client-authoritative knockback model.
    /// Detects enemy hitboxes locally, self-applies knockback instantly,
    /// triggers local sprite jolt, and notifies host to apply recoil.
    /// </summary>
    public class PlayerKnockbackReceiver : NetworkBehaviour
    {
        private PlayerController _playerController;

        private void Awake()
        {
            _playerController = GetComponentInParent<PlayerController>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Only the owning player client self-applies forces
            if (!isOwner) return;

            // Only check collisions against active hitboxes
            if (!collision.TryGetComponent(out Hitbox hitbox)) return;

            // Only respond to hitboxes from enemy units
            var enemyCombat = collision.GetComponentInParent<EnemyCombat>();
            if (enemyCombat == null || enemyCombat.Data == null) return;

            // Determine knockback direction from hitbox origin to player center
            Vector2 knockbackDir = (transform.position - collision.transform.position);
            
            // Fallback if exactly identical positions
            if (knockbackDir == Vector2.zero)
            {
                knockbackDir = Vector2.right * (UnityEngine.Random.value > 0.5f ? 1f : -1f);
            }
            knockbackDir = knockbackDir.normalized;

            // 1. INSTANT: Self-apply knockback locally (client has ownership over their Rigidbody)
            if (_playerController != null)
            {
                _playerController.AddExternalForce(knockbackDir * enemyCombat.Data.knockbackForce);
            }

            // 2. INSTANT: Mask latency with a quick visual jolt on the enemy sprite
            var enemyAnimator = collision.GetComponentInParent<EnemyAnimator>();
            if (enemyAnimator != null)
            {
                enemyAnimator.PlayInstantFake(knockbackDir);
            }

            // 3. ASYNC: Call ServerRpc to apply recoil physics to the enemy on the Host
            enemyCombat.ServerApplyRecoil(knockbackDir);
        }
    }
}
