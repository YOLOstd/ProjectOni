using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PurrNet;
using ProjectOni.Core;

namespace ProjectOni.Enemies
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyAnimator : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator _anim;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Transform _visualChild;

        private EnemyController _controller;
        private HealthComponent _health;
        private Coroutine _joltCoroutine;
        private Vector3 _originalSpriteLocalPos;
        private HashSet<int> _validParameters = new();
        private EnemyDeathEffect _deathEffect;

        private static readonly int SpeedKey = Animator.StringToHash("Speed");
        private static readonly int IsGroundedKey = Animator.StringToHash("IsGrounded");

        private void Awake()
        {
            _controller = GetComponent<EnemyController>();
            _health = GetComponent<HealthComponent>();
            _deathEffect = GetComponentInChildren<EnemyDeathEffect>();

            if (_anim == null) _anim = GetComponentInChildren<Animator>();
            if (_sprite == null) _sprite = GetComponentInChildren<SpriteRenderer>();
            
            if (_sprite != null)
            {
                _originalSpriteLocalPos = _sprite.transform.localPosition;
            }

            CacheAnimatorParameters();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();
            if (_health != null) _health.OnDied += OnDied;

            // Late-Joiner Catch-Up: dead body from pool — stay dead immediately.
            if (_health != null && _health.Current <= 0)
            {
                SafeSetBool(Animator.StringToHash("IsDead"), true);
                foreach (var col in GetComponentsInChildren<Collider2D>(true))
                    col.enabled = false;
            }
            else
            {
                // Pool Reset: snap to Idle, force immediate evaluation to prevent one-frame flicker.
                SafeSetBool(Animator.StringToHash("IsDead"), false);
                _anim?.Play("Idle", 0, 0f);
                _anim?.Update(0f); // Forces animation graph evaluation this frame — prevents dead-pose flicker
                _deathEffect?.ResetEffect(); // Ensure visual/dissolve states are completely reset for pooling
                foreach (var col in GetComponentsInChildren<Collider2D>(true))
                    col.enabled = true;
            }
        }

        protected override void OnDespawned(bool asServer)
        {
            base.OnDespawned(asServer);
            if (_health != null) _health.OnDied -= OnDied;
            if (_joltCoroutine != null) StopCoroutine(_joltCoroutine);
            _deathEffect?.ResetEffect(); // Pool reset — stop dissolve, restore material
        }

        private void CacheAnimatorParameters()
        {
            _validParameters.Clear();
            if (_anim == null) return;
            foreach (var param in _anim.parameters)
            {
                _validParameters.Add(param.nameHash);
            }
        }

        private void Update()
        {
            HandleSpriteFlip();
            HandleAnimatorParameters();
        }

        private void HandleSpriteFlip()
        {
            if (_visualChild == null) return;

            // Symmetrical horizontal flipping/mirroring of visual effects and sprites
            // MUST be handled via Y-axis rotation instead of modifying local scale.
            if (_controller.FacingDir < 0)
            {
                _visualChild.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else
            {
                _visualChild.localRotation = Quaternion.identity;
            }
        }

        private void HandleAnimatorParameters()
        {
            if (_anim == null) return;

            // Drive speed and grounded states
            // Map horizontal velocity absolute value to Animator Speed parameter
            float speed = Mathf.Abs(_controller.CurrentVelocity.x);
            SafeSetFloat(SpeedKey, speed);
            SafeSetBool(IsGroundedKey, _controller.IsGrounded);
        }

        private void OnDied()
        {
            SafeSetBool(Animator.StringToHash("IsDead"), true);
            foreach (var col in GetComponentsInChildren<Collider2D>(true))
                col.enabled = false;
        }

        // ─── Play Instant Fake ────────────────────────────────────────────────

        /// <summary>
        /// Visually jolt the sprite renderer locally on a client when a hit occurs.
        /// Purely client-side cosmetic to mask latency while server physics catches up.
        /// </summary>
        public void PlayInstantFake(Vector2 hitDirection)
        {
            if (_joltCoroutine != null)
            {
                StopCoroutine(_joltCoroutine);
            }
            
            if (gameObject.activeInHierarchy)
            {
                _joltCoroutine = StartCoroutine(JoltRoutine(hitDirection));
            }
        }

        private IEnumerator JoltRoutine(Vector2 hitDirection)
        {
            if (_sprite == null) yield break;

            Vector3 startPos = _originalSpriteLocalPos;
            // Jolt in hit direction by ~0.15 units (approx 3-5px in game scale)
            Vector3 joltOffset = (Vector3)(hitDirection.normalized * 0.15f);
            Vector3 targetPos = startPos + joltOffset;

            float duration = 0.1f;
            float elapsed = 0f;

            // Instant displacement
            _sprite.transform.localPosition = targetPos;

            // Elastic snap-back
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Cubic ease-out return
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                _sprite.transform.localPosition = Vector3.Lerp(targetPos, startPos, ease);
                yield return null;
            }

            _sprite.transform.localPosition = startPos;
            _joltCoroutine = null;
        }

        // ─── Safe Animator Helpers ────────────────────────────────────────────

        private void SafeSetFloat(int hash, float value)
        {
            if (_anim != null && _validParameters.Contains(hash))
            {
                _anim.SetFloat(hash, value);
            }
        }

        private void SafeSetBool(int hash, bool value)
        {
            if (_anim != null && _validParameters.Contains(hash))
            {
                _anim.SetBool(hash, value);
            }
        }
    }
}
