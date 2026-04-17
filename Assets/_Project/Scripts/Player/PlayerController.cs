using System;
using UnityEngine;

namespace ProjectOni.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
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
        private float _frameLeftGrounded = float.MinValue;
        private bool _coyoteUsable;
        private bool _bufferedJumpUsable;

        public bool IsGrounded => _grounded;
        public bool IsOnWall => _onWall;
        public int WallDir => _lastWallDir;
        public bool IsCrouching { get; private set; }
        public bool IsWallSliding { get; private set; }
        
        public bool CanCoyote => _coyoteUsable && !_grounded && Time.time < _frameLeftGrounded + _stats.CoyoteTime;
        public int AirJumpsRemaining { get; private set; }
        #endregion

        #region Events
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public event Action DodgingChanged;
        public event Action CrouchingChanged;
        #endregion

        #region Dodge Data
        public bool CanDodge { get; private set; } = true;
        public bool IsDodging { get; private set; }
        private float _dodgeCooldownTimer;
        private Vector2 _dodgeDir;
        private float _activeDodgePower;
        #endregion

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_bodyCol == null) _bodyCol = GetComponentInChildren<CapsuleCollider2D>();
            if (_feetCol == null) _feetCol = GetComponentInChildren<BoxCollider2D>();
        }

        private void FixedUpdate()
        {
            CheckCollisions();
            
            // Note: Movement logic is now driven by the State Machine calling methods here
            ApplyMovement();
        }

        private void CheckCollisions()
        {
            var groundMask = ~_stats.PlayerLayer;
            bool groundHit = Physics2D.BoxCast(_feetCol.bounds.center, _feetCol.size, 0, Vector2.down, _stats.GrounderDistance, groundMask);
            bool ceilingHit = Physics2D.CapsuleCast(_bodyCol.bounds.center, _bodyCol.size, _bodyCol.direction, 0, Vector2.up, _stats.GrounderDistance, groundMask);

            bool leftWall = Physics2D.CapsuleCast(_bodyCol.bounds.center, _bodyCol.size, _bodyCol.direction, 0, Vector2.left, _stats.GrounderDistance, _stats.WallLayer);
            bool rightWall = Physics2D.CapsuleCast(_bodyCol.bounds.center, _bodyCol.size, _bodyCol.direction, 0, Vector2.right, _stats.GrounderDistance, _stats.WallLayer);

            _onWall = leftWall || rightWall;
            _lastWallDir = leftWall ? -1 : 1;

            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                CanDodge = true;
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
            float speed = IsCrouching ? _stats.MaxSpeed * _stats.CrouchSpeedModifier : _stats.MaxSpeed;
            
            if (inputX == 0)
            {
                float deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, inputX * speed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        public void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else if (IsWallSliding)
            {
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.WallSlideSpeed, _stats.FallAcceleration * Time.fixedDeltaTime);
            }
            else
            {
                // Simple falling gravity. Jump cut is handled by ApplyJumpCut()
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, _stats.FallAcceleration * Time.fixedDeltaTime);
            }
        }

        public void SetCrouching(bool crouching)
        {
            if (IsCrouching == crouching) return;
            IsCrouching = crouching;
            CrouchingChanged?.Invoke();
        }

        public void SetWallSliding(bool sliding)
        {
            IsWallSliding = sliding;
        }

        public void ExecuteJump()
        {
            Debug.Log($"[PlayerController] ExecuteJump called. Grounded: {_grounded}, AirJumps: {AirJumpsRemaining}");
            
            _frameVelocity.y = _stats.JumpPower;
            _grounded = false; 
            _coyoteUsable = false;
            _bufferedJumpUsable = false;
            if (!_grounded && !CanCoyote) AirJumpsRemaining--;
            Jumped?.Invoke();
        }

        public void ApplyJumpCut()
        {
            _frameVelocity.y *= _stats.JumpCutMultiplier;
        }

        public void InitiateDodge(Vector2 inputDir)
        {
            IsDodging = true;
            CanDodge = false;
            _dodgeCooldownTimer = _stats.DodgeCooldown;
            
            // If no input, dodge in forward direction or last wall dir
            _dodgeDir = inputDir != Vector2.zero ? inputDir.normalized : new Vector2(_lastWallDir, 0);
            
            _activeDodgePower = _grounded ? _stats.DodgePower : _stats.AirDodgePower;
            _frameVelocity = _dodgeDir * _activeDodgePower;
            
            DodgingChanged?.Invoke();
        }

        public void HandleDodgeMovement()
        {
            // Maintain dodge velocity during the state
            _frameVelocity = _dodgeDir * _activeDodgePower;
        }

        public void EndDodge()
        {
            IsDodging = false;
            if (!_grounded)
            {
                _frameVelocity.x = Mathf.Clamp(_frameVelocity.x, -_stats.DodgeEndSpeed, _stats.DodgeEndSpeed);
                _frameVelocity.y = Mathf.Clamp(_frameVelocity.y, -_stats.DodgeEndSpeed, _stats.DodgeEndSpeed);
            }
            DodgingChanged?.Invoke();
        }

        public void UpdateDodgeCooldown()
        {
            if (_dodgeCooldownTimer > 0)
            {
                _dodgeCooldownTimer -= Time.deltaTime;
                if (_dodgeCooldownTimer <= 0) CanDodge = true;
            }
        }

        #endregion

        private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;
    }
}