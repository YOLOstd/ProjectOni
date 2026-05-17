using UnityEngine;
using System.Collections.Generic;
using ProjectOni.Core;
using PurrNet;

namespace ProjectOni.Combat
{
    /// <summary>
    /// Attached to a weapon or attack zone to deal damage.
    /// Expects a Collider2D set as a Trigger.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hitbox : MonoBehaviour, IPooledObject
    {
        private float _damage;
        private readonly HashSet<Collider2D> _hitColliders = new();
        private Collider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            // Start disabled
            _collider.enabled = false;
        }

        public void Initialize(float damage, float startTime = 0f, float duration = 0f)
        {
            _damage = damage;
            _hitColliders.Clear();
            _collider.enabled = false;

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(HitboxRoutine(startTime, duration));
            }
        }

        private System.Collections.IEnumerator HitboxRoutine(float startTime, float duration)
        {
            if (startTime > 0)
                yield return new WaitForSeconds(startTime);

            _collider.enabled = true;

            if (duration > 0)
            {
                yield return new WaitForSeconds(duration);
                _collider.enabled = false;
            }
        }

        public void ResetState()
        {
            _damage = 0f;
            _hitColliders.Clear();
            if (_collider != null) _collider.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Avoid hitting the same collider twice in a single swing/activation
            if (_hitColliders.Contains(collision))
                return;

            if (collision.TryGetComponent(out IDamageable damageable))
            {
                _hitColliders.Add(collision);

                // Networked interaction: Take ownership of the target if it's a networked object
                if (collision.TryGetComponent(out NetworkIdentity targetIdentity))
                {
                    var hitterIdentity = GetComponentInParent<NetworkIdentity>();
                    if (hitterIdentity != null && hitterIdentity.isOwner)
                    {
                        ProjectOni.Networking.NetworkedInteraction.RequestOwnershipOnHit(targetIdentity, hitterIdentity);
                    }
                }

                damageable.TakeDamage(_damage);
            }
        }
    }
}
