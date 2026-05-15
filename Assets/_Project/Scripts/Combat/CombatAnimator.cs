using System.Collections.Generic;
using UnityEngine;
using PurrNet;

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

        private void Awake()
        {
            // Search the parent hierarchy to share the same Animator instance that PlayerAnimator uses.
            if (_anim == null) _anim = GetComponentInParent<Animator>();
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
            _identity = GetComponentInParent<NetworkIdentity>();

            CacheAnimatorParameters();
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
            Vector3 spawnPos = _projectileSpawnPoint != null ? _projectileSpawnPoint.position : transform.position;
            spawnPos += (Vector3)request.spawnOffset;

            GameObject projGO = Instantiate(request.projectilePrefab, spawnPos, Quaternion.identity);
            if (projGO.TryGetComponent(out Projectile projectile))
            {
                bool isOwner = _identity != null && _identity.isOwner;
                projectile.Initialize(direction, request.projectileSpeed, request.damage, isOwner, LayerMask.GetMask("Enemy"), request.hitVFXPrefab);
            }
        }
    }
}
