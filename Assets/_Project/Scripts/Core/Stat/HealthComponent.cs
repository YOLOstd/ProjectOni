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

                // Initialise on the owner
                if (isOwner)
                {
                    if (_entityState.MaxHealth.value <= 0f)
                    {
                        _entityState.MaxHealth.value     = _baseMaxHealth;
                        _entityState.CurrentHealth.value = _baseMaxHealth;
                    }
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
        /// IDamageable entry point. Safe to call from any client —
        /// will route through ServerRpc if the caller is not the owner.
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (_entityState == null) return;

            if (isOwner)
            {
                ApplyDamage(amount);
            }
            else
            {
                // Non-owner client (or server for a client-owned entity) — ask the server
                ServerRequestDamage(amount);
            }
        }

        public void Heal(float amount)
        {
            if (_entityState == null) return;
            if (!isOwner) return;
            _entityState.CurrentHealth.value = Mathf.Min(Max, Current + amount);
        }

        // ─── Internal ─────────────────────────────────────────────────────────

        private void ApplyDamage(float amount)
        {
            if (IsDead) return;
            _entityState.CurrentHealth.value = Mathf.Max(0f, Current - amount);
        }

        [ServerRpc(requireOwnership: false)]
        private void ServerRequestDamage(float amount)
        {
            if (isOwner)
            {
                ApplyDamage(amount);
            }
            else
            {
                // Target has a client owner. Forward the damage command to the owner.
                if (owner.HasValue)
                {
                    TargetApplyDamage(owner.Value, amount);
                }
            }
        }

        [TargetRpc]
        private void TargetApplyDamage(PlayerID target, float amount)
        {
            ApplyDamage(amount);
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
