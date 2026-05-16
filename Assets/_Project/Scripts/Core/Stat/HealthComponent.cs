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

        // Owner-auth: owner writes, server+clients read via SyncVar sync.
        // For enemies the server is the de-facto owner so this still works.
        private readonly SyncVar<float> _currentHealth = new(0f, ownerAuth: true);
        private readonly SyncVar<float> _maxHealth     = new(0f, ownerAuth: true);

        public float Current => _currentHealth.value;
        public float Max     => _maxHealth.value;
        public bool  IsDead  => _currentHealth.value <= 0f;

        // UI / VFX / audio listen here — not global GameEvents
        public event Action<float, float> OnHealthChanged; // (current, max)
        public event Action               OnDied;

        // ─── Lifecycle ────────────────────────────────────────────────────────

        protected override void OnSpawned()
        {
            _currentHealth.onChangedWithOld += OnCurrentHealthSync;
            _maxHealth.onChangedWithOld     += OnMaxHealthSync;

            // Initialise on the owner (or server for enemies)
            if (isOwner || isServer)
            {
                _maxHealth.value     = _baseMaxHealth;
                _currentHealth.value = _baseMaxHealth;
            }
        }

        protected override void OnDespawned(bool asServer)
        {
            _currentHealth.onChangedWithOld -= OnCurrentHealthSync;
            _maxHealth.onChangedWithOld     -= OnMaxHealthSync;
        }

        // ─── Public API ───────────────────────────────────────────────────────

        /// <summary>Called by StatController when equipment changes the health cap.</summary>
        public void SetMaxHealth(float newMax)
        {
            if (!isOwner && !isServer) return;

            float pct = _maxHealth.value > 0f
                ? _currentHealth.value / _maxHealth.value
                : 1f;

            _maxHealth.value     = newMax;
            _currentHealth.value = Mathf.Round(newMax * pct);
        }

        /// <summary>
        /// IDamageable entry point. Safe to call from any client —
        /// will route through ServerRpc if the caller is not the owner/server.
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (isOwner || isServer)
            {
                ApplyDamage(amount);
            }
            else
            {
                // Non-owner client (e.g. hitting an enemy) — ask the server
                ServerRequestDamage(amount);
            }
        }

        public void Heal(float amount)
        {
            if (!isOwner && !isServer) return;
            _currentHealth.value = Mathf.Min(_maxHealth.value, _currentHealth.value + amount);
        }

        // ─── Internal ─────────────────────────────────────────────────────────

        private void ApplyDamage(float amount)
        {
            if (IsDead) return;
            _currentHealth.value = Mathf.Max(0f, _currentHealth.value - amount);
        }

        [ServerRpc(requireOwnership: false)]
        private void ServerRequestDamage(float amount)
        {
            ApplyDamage(amount);
        }

        // SyncVar callbacks fire on ALL clients (including the one that wrote)
        private void OnCurrentHealthSync(float oldVal, float newVal)
        {
            OnHealthChanged?.Invoke(newVal, _maxHealth.value);

            if (newVal <= 0f && oldVal > 0f)
            {
                OnDied?.Invoke();
                Debug.Log($"[HealthComponent] {gameObject.name} died.");
            }
        }

        private void OnMaxHealthSync(float oldVal, float newVal)
        {
            OnHealthChanged?.Invoke(_currentHealth.value, newVal);
        }
    }
}
