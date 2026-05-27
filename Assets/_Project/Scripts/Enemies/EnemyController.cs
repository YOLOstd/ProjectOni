using System;
using UnityEngine;
using PurrNet;
using ProjectOni.Core;
using ProjectOni.Data;

namespace ProjectOni.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : NetworkBehaviour
    {
        [Header("Collision & Physics Settings")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private CapsuleCollider2D _bodyCol;
        [SerializeField] private float _gravityValue = 50f;
        [SerializeField] private float _maxFallSpeed = 30f;
        [SerializeField] private float _groundDecel = 25f;
        [SerializeField] private float _airDecel = 5f;

        private Rigidbody2D _rb;
        private Vector2 _frameVelocity;
        private Vector2 _externalVelocity;
        private bool _grounded;

        // Facings
        private readonly SyncVar<int> _facingDir = new(1, ownerAuth: true);

        public int FacingDir => _facingDir.value;
        public bool IsGrounded => _grounded;
        public Vector2 CurrentVelocity => _rb.linearVelocity;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_bodyCol == null) _bodyCol = GetComponentInChildren<CapsuleCollider2D>();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();
            
            // Re-enable colliders disabled by death on all clients
            foreach (var col in GetComponentsInChildren<Collider2D>(true))
                col.enabled = true;

            // Explicitly declare the body type based on authority ONCE.
            // In Host mode, isServer is true, so it guarantees Dynamic stays Dynamic.
            _rb.bodyType = isServer ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;

            if (!isServer)
            {
                _rb.linearVelocity = Vector2.zero;
                return; // Pure clients stop here
            }

            // --- SERVER ONLY BELOW THIS LINE ---
            _frameVelocity    = Vector2.zero;
            _externalVelocity = Vector2.zero;

            var entityState = GetComponent<EntityState>();
            var stats       = GetComponent<StatController>();
            if (stats != null && entityState != null)
                stats.Initialize(entityState);
        }

        private void FixedUpdate()
        {
            if (!isOwner) return;

            CheckCollisions();
            DecayExternalVelocity();
            HandleGravity();
            ApplyMovement();
        }

        private void CheckCollisions()
        {
            if (_bodyCol == null)
            {
                _grounded = false;
                return;
            }

            float castDistance = 0.05f;
            RaycastHit2D hit = Physics2D.CapsuleCast(
                _bodyCol.bounds.center,
                _bodyCol.size,
                _bodyCol.direction,
                0f,
                Vector2.down,
                castDistance,
                _groundLayer
            );

            _grounded = hit.collider != null;
        }

        private void DecayExternalVelocity()
        {
            float decelX = _grounded ? _groundDecel : _airDecel;
            _externalVelocity.x = Mathf.MoveTowards(_externalVelocity.x, 0f, decelX * Time.fixedDeltaTime);
            _externalVelocity.y = 0f; // Gravity handles Y naturally
        }

        private void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = -2f; // Grounding force to keep grounded
            }
            else
            {
                // Fall towards max fall speed
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_maxFallSpeed, _gravityValue * Time.fixedDeltaTime);
            }
        }

        private void ApplyMovement()
        {
            _rb.linearVelocity = _frameVelocity + _externalVelocity;
        }

        // ─── Public API ───────────────────────────────────────────────────────

        public void SetHorizontalVelocity(float xVelocity)
        {
            if (!isOwner) return;
            _frameVelocity.x = xVelocity;

            if (xVelocity != 0f)
            {
                _facingDir.value = (int)Mathf.Sign(xVelocity);
            }
        }

        public void SetFacingDir(int dir)
        {
            if (!isOwner) return;
            if (dir != 0)
            {
                _facingDir.value = (int)Mathf.Sign(dir);
            }
        }

        /// <summary>
        /// Apply an external force to this enemy (e.g. recoil when attacking).
        /// Runs on Host only.
        /// </summary>
        public void ApplyRecoil(Vector2 force)
        {
            if (!isOwner) return;

            // X goes to external velocity to slide/decay smoothly
            _externalVelocity.x += force.x;
            
            // Y goes to frame velocity so gravity controls the arc
            _frameVelocity.y += force.y;
        }
    }
}
