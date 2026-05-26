using System.Collections;
using UnityEngine;
using ProjectOni.Core;

namespace ProjectOni.Enemies
{
    /// <summary>
    /// Fully modular death VFX. Self-subscribes to HealthComponent.OnDied.
    /// Handles particle burst + sprite dissolve. Pool-safe.
    /// Attach to the Ghoul prefab root alongside EnemyAnimator.
    /// </summary>
    public class EnemyDeathEffect : MonoBehaviour
    {
        [Header("Particle VFX")]
        [SerializeField] private GameObject _deathParticlePrefab;

        [Header("Dissolve")]
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private float _dissolveDuration = 0.8f;

        private static readonly int DissolveAmountID = Shader.PropertyToID("_Dissolve_Amount");

        private HealthComponent _health;
        private Material _matInstance;

        // ─── Lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            _health = GetComponentInParent<HealthComponent>();
            if (_sprite == null) _sprite = GetComponentInChildren<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (_health != null) _health.OnDied += OnDied;
            ResetEffect();
        }

        private void OnDisable()
        {
            if (_health != null) _health.OnDied -= OnDied;
        }

        private void OnDestroy()
        {
            // Prevent orphaned material instance on scene unload / permanent destruction.
            if (_matInstance != null)
                Destroy(_matInstance);
        }

        // ─── Death Reaction ──────────────────────────────────────────────────

        private void OnDied()
        {
            // 1. Particle burst (pooled — no GC allocation)
            if (_deathParticlePrefab != null && VFXPoolManager.Instance != null)
            {
                var vfx = VFXPoolManager.Instance.Spawn(_deathParticlePrefab, transform.position);
                if (vfx != null && vfx.TryGetComponent<PooledVFX>(out var pooled))
                {
                    pooled.ReleaseAfter(1.5f);
                }
            }

            // 2. Sprite dissolve
            if (_sprite != null)
            {
                // _sprite.material auto-instantiates a clone on first access.
                // Cache it so we can reset and destroy it safely.
                if (_matInstance == null)
                    _matInstance = _sprite.material;

                StartCoroutine(DissolveRoutine());
            }
        }

        // ─── Pool Reset ──────────────────────────────────────────────────────

        /// <summary>
        /// Called by EnemyAnimator.OnDespawned when the pool recycles this object.
        /// Stops any in-progress dissolve and resets the sprite to fully visible.
        /// </summary>
        public void ResetEffect()
        {
            // Kill any running dissolve — prevents stale coroutine running on a
            // freshly-spawned enemy if the server despawns mid-dissolve (e.g. wave wipe).
            StopAllCoroutines();

            if (_sprite != null)
            {
                // _sprite.material auto-instantiates a clone on first access.
                // Cache it so we can reset and destroy it safely.
                if (_matInstance == null)
                    _matInstance = _sprite.material;

                // Restore full opacity. No need to reassign _sprite.material —
                // the SpriteRenderer already holds this instance reference.
                _matInstance.SetFloat(DissolveAmountID, 0f);
            }
        }

        // ─── Dissolve Coroutine ──────────────────────────────────────────────

        private IEnumerator DissolveRoutine()
        {
            float elapsed = 0f;
            while (elapsed < _dissolveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _dissolveDuration);
                // Quadratic ease-in: slow start, fast finish — feels like crumbling apart
                _matInstance.SetFloat(DissolveAmountID, t * t);
                yield return null;
            }
            _matInstance.SetFloat(DissolveAmountID, 1f);
        }
    }
}
