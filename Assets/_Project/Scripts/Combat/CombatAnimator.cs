using System.Collections.Generic;
using UnityEngine;
using PurrNet;
using ProjectOni.Core;

namespace ProjectOni.Combat
{
    /// <summary>
    /// Handles local/replicated combat visuals (animations, SFX, projectiles).
    /// Shares the Animator with PlayerAnimator — does NOT own it.
    /// Does NOT use NetworkAnimator for triggers because it is called inside an ObserversRpc.
    /// </summary>
    public class CombatAnimator : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The shared Animator on the visual child. Leave empty to auto-find in parent.")]
        [SerializeField] private Animator _anim;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private Transform _projectileSpawnPoint;

        private NetworkIdentity _identity;
        private HashSet<int> _validParameters = new();
        private readonly List<GameObject> _activeVisuals = new();

        private void Awake()
        {
            // Search the parent hierarchy to share the same Animator instance that PlayerAnimator uses.
            if (_anim == null) _anim = GetComponentInParent<Animator>();
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
            _identity = GetComponentInParent<NetworkIdentity>();

            CacheAnimatorParameters();
        }

        public void CancelVisuals()
        {
            foreach (var visual in _activeVisuals)
            {
                if (visual == null) continue;
                if (visual.TryGetComponent<PooledVFX>(out var pooled))
                {
                    pooled.Release();
                }
                else
                {
                    Destroy(visual);
                }
            }
            _activeVisuals.Clear();
        }

        private void CacheAnimatorParameters()
        {
            _validParameters.Clear();
            if (_anim == null) return;
            foreach (var param in _anim.parameters)
                _validParameters.Add(param.nameHash);
        }

        /// <summary>
        /// Called on ALL clients via CombatController's ObserversRpc.
        /// </summary>
        public void PlayVisual(VisualRequest request, Vector2 direction)
        {
            // We use the raw _anim here because this method is already running inside an RPC broadcast.
            // Using NetworkAnimator.SetTrigger here would cause a double-broadcast.
            if (!string.IsNullOrEmpty(request.animationTrigger))
                SafeSetTrigger(Animator.StringToHash(request.animationTrigger));

            if (_audioSource != null && request.sfx != null)
                _audioSource.PlayOneShot(request.sfx);

            if (request.projectilePrefab != null)
                SpawnProjectile(request, direction);
        }

        private void SafeSetTrigger(int hash)
        {
            if (_anim != null && _validParameters.Contains(hash))
                _anim.SetTrigger(hash);
        }

        private void SpawnProjectile(VisualRequest request, Vector2 direction)
        {
            Vector3 basePos = transform.position;
            if (_projectileSpawnPoint != null)
            {
                Vector3 localSpawnOffset = transform.InverseTransformPoint(_projectileSpawnPoint.position);
                if (direction.x < 0)
                {
                    localSpawnOffset.x *= -1f;
                }
                basePos = transform.TransformPoint(localSpawnOffset);
            }

            Vector3 spawnOffset = (Vector3)request.spawnOffset;
            if (direction.x < 0)
            {
                spawnOffset.x *= -1f;
            }
            Vector3 spawnPos = basePos + spawnOffset;

            // Melee slashes (speed = 0) should follow the player
            Transform parent = request.projectileSpeed == 0 ? (_projectileSpawnPoint != null ? _projectileSpawnPoint : transform) : null;
            
            GameObject projGO;
            if (VFXPoolManager.Instance != null)
            {
                projGO = VFXPoolManager.Instance.Spawn(request.projectilePrefab, spawnPos, Quaternion.identity, parent);
            }
            else
            {
                projGO = Instantiate(request.projectilePrefab, spawnPos, Quaternion.identity, parent);
            }

            _activeVisuals.RemoveAll(x => x == null);
            _activeVisuals.Add(projGO);

            // 1. Calculate the angle assuming the attack is ALWAYS on the right hemisphere
            float baseAngle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;

            // 2. Apply this constrained right-side rotation
            projGO.transform.rotation = Quaternion.Euler(0, 0, baseAngle);

            // 3. If attacking left, perfectly mirror the entire right-side setup
            if (direction.x < 0) 
            {
                projGO.transform.rotation *= Quaternion.Euler(0, 180, 0); 
            }

            // Scale particle system speed based on target lifetime (slashDuration)
            if (request.lifetime > 0)
            {
                var particleSystems = projGO.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particleSystems)
                {
                    var main = ps.main;
                    if (main.duration > 0)
                    {
                        main.simulationSpeed = main.duration / request.lifetime;
                    }
                }
            }

            if (projGO.TryGetComponent(out Projectile projectile))
            {
                bool isOwner = _identity != null && _identity.isOwner;
                projectile.Initialize(direction, request.projectileSpeed, request.damage, isOwner, LayerMask.GetMask("Enemy"), request.hitVFXPrefab);

                // Ghost mode: disable all physics colliders on non-owner machines.
                if (!isOwner)
                {
                    foreach (var col in projGO.GetComponentsInChildren<Collider2D>())
                        col.enabled = false;
                }
            }
            else if (projGO.TryGetComponent(out Hitbox hitbox))
            {
                // Only activate the damage hitbox on the owning client.
                // On remote clients the VFX (particle children) still play, but no collision damage fires.
                bool isOwner = _identity != null && _identity.isOwner;
                if (isOwner)
                {
                    hitbox.Initialize(request.damage, request.hitboxStartTime, request.hitboxDuration);
                }
                else
                {
                    // Ghost mode: ensure every collider on the prefab is disabled so nothing
                    // triggers on the non-owner machine, even if the hitbox prefab had one enabled by default.
                    foreach (var col in projGO.GetComponentsInChildren<Collider2D>())
                        col.enabled = false;
                }
                float destroyTime = request.lifetime > 0 ? request.lifetime : 0.2f;
                if (projGO.TryGetComponent<PooledVFX>(out var pooled))
                {
                    pooled.ReleaseAfter(destroyTime);
                }
                else
                {
                    Destroy(projGO, destroyTime);
                }
            }
            else if (request.lifetime > 0)
            {
                if (projGO.TryGetComponent<PooledVFX>(out var pooled))
                {
                    pooled.ReleaseAfter(request.lifetime);
                }
                else
                {
                    Destroy(projGO, request.lifetime);
                }
            }
        }
    }
}
