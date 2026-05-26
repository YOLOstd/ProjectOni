using System;
using UnityEngine;
using PurrNet;
using ProjectOni.Core;

namespace ProjectOni.Core
{
    /// <summary>
    /// Networked health state. Reusable on players AND enemies.
    ///
    /// Player prefab   — isOwner = the player client  → ownerAuth writes health directly.
    /// Enemy prefab    — no owner assigned             → server is effective owner via ServerRpc.
    ///
    /// TakeDamage() automatically routes to a ServerRpc when called from a non-owner client,
    /// so callers (Hitbox, Projectile) don't need to know who owns the target.
    /// </summary>
    public class HealthComponent : NetworkBehaviour, IDamageable
    {
        [Header("Base Health (used if StatController is absent)")]
        [SerializeField] private float _baseMaxHealth = 100f;

        private EntityState _entityState;

        public float Current => _entityState != null ? _entityState.CurrentHealth.value : 0f;
        public float Max     => _entityState != null ? _entityState.MaxHealth.value : 0f;
        public bool  IsDead  => Current <= 0f;

        // UI / VFX / audio listen here — not global GameEvents
        public event Action<float, float> OnHealthChanged; // (current, max)
        public event Action               OnDied;

        // ─── Lifecycle ────────────────────────────────────────────────────────

        private void Awake()
        {
            _entityState = GetComponent<EntityState>();
        }

        protected override void OnSpawned()
        {
            if (_entityState != null)
            {
                _entityState.CurrentHealth.onChangedWithOld += OnCurrentHealthSync;
                _entityState.MaxHealth.onChangedWithOld     += OnMaxHealthSync;

                // Initialise on the owner or server (for unowned/server-auth entities like enemies)
                if (isOwner || isServer)
                {
                    if (_entityState.MaxHealth.value <= 0f)
                    {
                        _entityState.MaxHealth.value = _baseMaxHealth;
                    }
                    // Reset health to maximum when spawned (initially or recycled from pool)
                    _entityState.CurrentHealth.value = _entityState.MaxHealth.value;
                }
            }
        }

        protected override void OnDespawned(bool asServer)
        {
            if (_entityState != null)
            {
                _entityState.CurrentHealth.onChangedWithOld -= OnCurrentHealthSync;
                _entityState.MaxHealth.onChangedWithOld     -= OnMaxHealthSync;
            }
        }

        // ─── Public API ───────────────────────────────────────────────────────



        /// <summary>
        /// IDamageable entry point. Directly modifies the synced state with a dead-horse guard.
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (_entityState == null) return;

            float currentVal = _entityState.CurrentHealth.value;
            if (currentVal <= 0f) return;

            _entityState.CurrentHealth.value = Mathf.Max(0f, currentVal - amount);
        }

        public void Heal(float amount)
        {
            if (_entityState == null) return;
            if (!isOwner) return;
            _entityState.CurrentHealth.value = Mathf.Min(Max, Current + amount);
        }

        // SyncVar callbacks fire on ALL clients (including the one that wrote)
        private void OnCurrentHealthSync(float oldVal, float newVal)
        {
            OnHealthChanged?.Invoke(newVal, Max);

            if (newVal <= 0f && oldVal > 0f)
            {
                OnDied?.Invoke();
                Debug.Log($"[HealthComponent] {gameObject.name} died.");
            }
        }

        private void OnMaxHealthSync(float oldVal, float newVal)
        {
            OnHealthChanged?.Invoke(Current, newVal);
        }
    }
}
