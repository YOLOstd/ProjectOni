using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectOni.Core;

namespace ProjectOni.Combat
{
    /// <summary>
    /// Attached to a weapon or attack zone.
    /// A decoupled, "dumb" collision sensor that raises an event upon hitting a Hurtbox.
    /// Expects a Collider2D set as a Trigger.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hitbox : MonoBehaviour, IPooledObject
    {
        private float _damage;
        private readonly HashSet<Collider2D> _hitColliders = new();
        private Collider2D _collider;

        /// <summary>
        /// Raised when this hitbox collides with a valid Hurtbox.
        /// </summary>
        public event Action<Hurtbox> OnHit;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            // Start disabled
            _collider.enabled = false;
        }

        private Coroutine _hitboxRoutine;

        public void Initialize(float damage, float startTime = 0f, float duration = 0f)
        {
            _damage = damage;
            _hitColliders.Clear();
            _collider.enabled = false;

            if (_hitboxRoutine != null)
            {
                StopCoroutine(_hitboxRoutine);
                _hitboxRoutine = null;
            }

            if (gameObject.activeInHierarchy)
            {
                _hitboxRoutine = StartCoroutine(HitboxRoutine(startTime, duration));
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

            if (_hitboxRoutine != null)
            {
                StopCoroutine(_hitboxRoutine);
                _hitboxRoutine = null;
            }
            OnHit = null; // Clear all subscribers to prevent memory leaks and multiplier bugs when pooled
        }

        private void OnDisable()
        {
            if (_hitboxRoutine != null)
            {
                StopCoroutine(_hitboxRoutine);
                _hitboxRoutine = null;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Only interact with trigger colliders (Hurtboxes)
            if (!collision.isTrigger) return;

            // Avoid hitting the same collider twice in a single swing/activation
            if (_hitColliders.Contains(collision))
                return;

            if (collision.TryGetComponent(out Hurtbox hurtbox))
            {
                _hitColliders.Add(collision);
                OnHit?.Invoke(hurtbox);
            }
        }
    }
}
