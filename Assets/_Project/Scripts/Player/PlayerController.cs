using System;
using UnityEngine;
using PurrNet;
using ProjectOni.Core;
using ProjectOni.UI;

namespace ProjectOni.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private TarodevController.PlayerMovementData _stats;
        [SerializeField] private CapsuleCollider2D _bodyCol;
        [SerializeField] private BoxCollider2D _feetCol;
        
        private Rigidbody2D _rb;
        private Vector2 _frameVelocity;

        public TarodevController.PlayerMovementData Stats => _stats;
        public Vector2 CurrentVelocity => _frameVelocity;

        #region Collision Data
        private bool _grounded;
        private bool _onWall;
        private int _lastWallDir;
        
        private SyncVar<int> _facingDir = new(1, ownerAuth: true);
        
        private float _frameLeftGrounded = float.MinValue;
        private bool _coyoteUsable;
        private float _wallJumpUnlockTime;

        public bool IsGrounded => _grounded;
        public bool IsOnWall => _onWall;
        public int WallDir => _lastWallDir;
        
        public SyncVar<bool> IsWallSliding = new(ownerAuth: true);
        
        public bool CanCoyote => _coyoteUsable && !_grounded && Time.time < _frameLeftGrounded + _stats.CoyoteTime;
        public int AirJumpsRemaining { get; private set; }
        public int FacingDir => _facingDir.value;
        #endregion

        #region Events
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        #endregion

        #region Dodge API & Physics Boundary
        public void SetVelocity(Vector2 velocity) => _frameVelocity = velocity;
        public Vector2 GetVelocity() => _frameVelocity;
        public void TeleportTo(Vector2 targetPosition) => transform.position = targetPosition;
        #endregion

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_bodyCol == null) _bodyCol = GetComponentInChildren<CapsuleCollider2D>();
            if (_feetCol == null) _feetCol = GetComponentInChildren<BoxCollider2D>();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();
            
            if (!isOwner)
            {
                Debug.Log($"[PlayerController] Remote player spawned. isOwner: false");
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.linearVelocity = Vector2.zero;
                var vcam = GetComponentInChildren<Unity.Cinemachine.CinemachineCamera>();
                if (vcam != null)
                {
                    vcam.enabled = false;
                }
            }
            else
            {
                Debug.Log($"[PlayerController] Local owner spawned. isOwner: true. Resolving components in children...");
                
                var entityState = GetComponentInChildren<EntityState>();
                var health      = GetComponentInChildren<HealthComponent>();
                var stats       = GetComponentInChildren<StatController>();
                var equipment   = GetComponentInChildren<EquipmentManager>();

                Debug.Log($"[PlayerController] Resolved components: EntityState={entityState != null}, HealthComponent={health != null}, StatController={stats != null}, EquipmentManager={equipment != null}");

                if (stats != null)
                {
                    Debug.Log("[PlayerController] Initializing StatController with EntityState.");
                    stats.Initialize(entityState);
                }
                else
                {
                    Debug.LogError("[PlayerController] StatController was not found in children!");
                }

                if (HUDManager.Instance != null)
                {
                    Debug.Log("[PlayerController] Binding HUD to local player components.");
                    HUDManager.Instance.BindLocalPlayer(health, stats, equipment);
                }
                else
                {
                    Debug.LogWarning("[PlayerController] HUDManager.Instance is null when spawning local player!");
                }
            }
        }

        private void FixedUpdate()
        {
            if (!isOwner) return;

            CheckCollisions();
            
            // Note: Movement logic is now driven by the State Machine calling methods here
            ApplyMovement();
        }

        private void CheckCollisions()
        {
            var groundMask = _stats.GroundLayer;
            bool groundHit = Physics2D.BoxCast(_feetCol.bounds.center, _feetCol.size, 0, Vector2.down, _stats.GrounderDistance, groundMask);
            bool ceilingHit = Physics2D.CapsuleCast(_bodyCol.bounds.center, _bodyCol.size, _bodyCol.direction, 0, Vector2.up, _stats.GrounderDistance, groundMask);

            bool leftWall = Physics2D.CapsuleCast(_bodyCol.bounds.center, _bodyCol.size, _bodyCol.direction, 0, Vector2.left, _stats.GrounderDistance, _stats.WallLayer);
            bool rightWall = Physics2D.CapsuleCast(_bodyCol.bounds.center, _bodyCol.size, _bodyCol.direction, 0, Vector2.right, _stats.GrounderDistance, _stats.WallLayer);

            _onWall = leftWall || rightWall;
            _lastWallDir = leftWall ? -1 : 1;

            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            if (!_grounded && groundHit && _frameVelocity.y <= 0f)
            {
                _grounded = true;
                _coyoteUsable = true;
                AirJumpsRemaining = _stats.MaxAirJumps;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = Time.time;
                GroundedChanged?.Invoke(false, 0);
            }
        }

        #region Physics Services for States

        public void HandleHorizontalMovement(float inputX)
        {
            if (Time.time < _wallJumpUnlockTime) return;

            if (inputX != 0) _facingDir.value = (int)Mathf.Sign(inputX);

            float speed = _stats.MaxSpeed;
            
            if (inputX == 0)
            {
                float deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                // Digital behavior: snap to full speed if there is any input
                // This ensures keyboard and controller feel the same and prevents diagonal slowdown
                float targetX = Mathf.Sign(inputX);
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, targetX * speed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        public void ApplyAttackLunge(float lungeForce)
        {
            // Push the player forward in the direction they are facing
            _frameVelocity.x = _facingDir.value * lungeForce;
        }

        public void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else if (IsWallSliding.value)
            {
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.WallSlideSpeed, _stats.FallAcceleration * Time.fixedDeltaTime);
            }
            else
            {
                // Simple falling gravity. Jump cut is handled by ApplyJumpCut()
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, _stats.FallAcceleration * Time.fixedDeltaTime);
            }
        }

        public void OnAirborneAttackInitiated()
        {
            // Removed instant vertical velocity dampening.
            // Slow-mo trajectory is now handled dynamically in HandleAirborneAttackGravity.
        }

        public void HandleAirborneAttackGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else
            {
                // Smoothly drag both horizontal and vertical velocity to simulate "slow mo" air resistance.
                // This creates a smooth slowdown that preserves the original direction/arc.
                _frameVelocity = Vector2.Lerp(_frameVelocity, Vector2.zero, _stats.AirborneAttackDrag * Time.fixedDeltaTime);
        
                // Accelerate downwards at a gentle pace towards a floaty fall speed ceiling
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.AirborneAttackMaxFallSpeed, _stats.AirborneAttackFallAcceleration * Time.fixedDeltaTime);
            }
        }

        public void SetWallSliding(bool sliding)
        {
            IsWallSliding.value = sliding;
        }

        public void ExecuteJump()
        {
            _frameVelocity.y = _stats.JumpPower;

            if (!_grounded && !CanCoyote) AirJumpsRemaining--;

            _grounded = false; 
            _coyoteUsable = false;
            Jumped?.Invoke();
            GroundedChanged?.Invoke(false, 0f);
        }

        public void ExecuteWallJump(int wallDir)
        {
            _facingDir.value = -wallDir;
            _frameVelocity.x = _stats.WallJumpXForce * -wallDir;
            _frameVelocity.y = _stats.WallJumpYForce;
            _wallJumpUnlockTime = Time.time + _stats.WallJumpLockTime;
            
            _grounded = false;
            _coyoteUsable = false;
            AirJumpsRemaining = _stats.MaxAirJumps;

            Jumped?.Invoke();
            GroundedChanged?.Invoke(false, 0f);
        }

        public void ApplyJumpCut()
        {
            _frameVelocity.y *= _stats.JumpCutMultiplier;
        }

        #endregion

        private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;
    }
}