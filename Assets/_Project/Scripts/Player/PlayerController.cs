using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace TarodevController
{
    /// <summary>
    /// Hey!
    /// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
    /// I have a premium version on Patreon, which has every feature you'd expect from a polished controller. Link: https://www.patreon.com/tarodev
    /// You can play and compete for best times here: https://tarodev.itch.io/extended-ultimate-2d-controller
    /// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/tarodev
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [SerializeField] private PlayerMovementData _stats;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private PlayerInput _playerInput;
        private InputAction _moveAction, _jumpAction, _dodgeAction, _crouchAction;
        
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;

        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public event Action DodgingChanged;
        public event Action CrouchingChanged;
        public event Action WallChanged;

        public bool IsDodging => _dodging;
        public bool IsCrouching => _crouching;
        public bool IsOnWall => _onWall;
        public bool IsWallSliding => _onWall && _frameVelocity.y < 0 && _frameInput.Move.x != 0 && Mathf.Sign(_frameInput.Move.x) == _lastWallDir;

        #endregion

        private float _time;
        private int _airJumpsRemaining;
        private bool _dodgeToConsume;
        private bool _canDodge = true;
        private bool _dodging;
        private float _dodgeTimer;
        private float _dodgeCooldownTimer;
        private Vector2 _dodgeDir;
        private bool _onWall;
        private int _lastWallDir;
        private bool _crouching;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _playerInput = GetComponent<PlayerInput>();

            _moveAction = _playerInput.actions["Move"];
            _jumpAction = _playerInput.actions["Jump"];
            _dodgeAction = _playerInput.actions["Dodge"];
            _crouchAction = _playerInput.actions["Crouch"];

        }

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();
        }

        private void GatherInput()
        {
            _frameInput = new FrameInput
            {
                JumpDown = _jumpAction.WasPressedThisFrame(),
                JumpHeld = _jumpAction.IsPressed(),
                DodgeDown = _dodgeAction.WasPressedThisFrame(),
                CrouchDown = _crouchAction.IsPressed(),
                Move = _moveAction.ReadValue<Vector2>()
            };

            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }

            if (_frameInput.DodgeDown && _canDodge)
            {
                _dodgeToConsume = true;
            }

            if (!_crouching && _grounded && _frameInput.CrouchDown)
            {
                _crouching = true;
                CrouchingChanged?.Invoke();
            }
            else if (_crouching && (!_frameInput.CrouchDown || !_grounded))
            {
                _crouching = false;
                CrouchingChanged?.Invoke();
            }
        }

        private void FixedUpdate()
        {
            CheckCollisions();

            HandleDodge();

            if (!_dodging)
            {
                HandleJump();
                HandleDirection();
                HandleGravity();
                HandleLedgeCorrection();
            }
            
            ApplyMovement();
        }

        #region Collisions
        
        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        private void CheckCollisions()
        {
            // Ground and Ceiling (collides with everything except the player)
            var groundMask = ~_stats.PlayerLayer;

            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, groundMask);
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, groundMask);

            // Walls
            bool leftWall = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.left, _stats.GrounderDistance, _stats.WallLayer);
            bool rightWall = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.right, _stats.GrounderDistance, _stats.WallLayer);

#if UNITY_EDITOR
            // Debug visualization
            Debug.DrawLine(_col.bounds.center, _col.bounds.center + Vector3.down * (_col.size.y / 2 + _stats.GrounderDistance), groundHit ? Color.green : Color.red);
#endif

            _onWall = leftWall || rightWall;
            _lastWallDir = leftWall ? -1 : 1;

            // Hit a Ceiling
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                _canDodge = true;
                _airJumpsRemaining = _stats.MaxAirJumps;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }

        }

        #endregion


        #region Jumping

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

        private void HandleJump()
        {
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.linearVelocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            // Strict Wall Jump Condition: Moving down while pushing into wall
            bool canWallJump = _onWall && _frameVelocity.y < 0 && _frameInput.Move.x != 0 && Mathf.Sign(_frameInput.Move.x) == _lastWallDir;

            if (_grounded || CanUseCoyote) ExecuteJump(false);
            else if (canWallJump) ExecuteJump(true);
            else if (_airJumpsRemaining > 0)
            {
                _airJumpsRemaining--;
                ExecuteJump(false);
            }

            _jumpToConsume = false;
        }

        private void ExecuteJump(bool isWallJump)
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            
            if (isWallJump)
            {
                _frameVelocity = new Vector2(-_lastWallDir * _stats.WallJumpPower * 0.7f, _stats.WallJumpPower);
            }
            else
            {
                _frameVelocity.y = _stats.JumpPower;
            }
            
            Jumped?.Invoke();
        }

        #endregion

        #region Horizontal

        private void HandleDirection()
        {
            var speed = _crouching ? _stats.MaxSpeed * _stats.CrouchSpeedModifier : _stats.MaxSpeed;
            
            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * speed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else
            {
                // Strict Wall Slide: Only if pushing into wall AND falling
                bool isPushingIntoWall = _frameInput.Move.x != 0 && Mathf.Sign(_frameInput.Move.x) == _lastWallDir;
                
                if (_onWall && _frameVelocity.y < 0 && isPushingIntoWall)
                {
                    _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.WallSlideSpeed, _stats.FallAcceleration * Time.fixedDeltaTime);
                }
                else
                {
                    var inAirGravity = _stats.FallAcceleration;
                    if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                    _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
                }
            }
        }

        private void HandleDodge()
        {
            if (_dodgeToConsume)
            {
                _dodgeDir = _frameInput.Move.normalized;
                if (_dodgeDir == Vector2.zero) _dodgeDir = new Vector2(_lastWallDir, 0); // Default to last face dir
                
                _dodging = true;
                _canDodge = false;
                _dodgeTimer = _stats.DodgeDuration;
                _dodgeCooldownTimer = _stats.DodgeCooldown;
                _dodgeToConsume = false;
                DodgingChanged?.Invoke();
            }

            if (_dodging)
            {
                _frameVelocity = _dodgeDir * _stats.DodgePower;
                _dodgeTimer -= Time.fixedDeltaTime;

                if (_dodgeTimer <= 0)
                {
                    _dodging = false;

                    if (_grounded)
                    {
                        _frameVelocity = Vector2.zero;
                    }
                    else
                    {
                        // Clamp air momentum to DodgeEndSpeed for a controlled glide
                        _frameVelocity.x = Mathf.Clamp(_frameVelocity.x, -_stats.DodgeEndSpeed, _stats.DodgeEndSpeed);
                        _frameVelocity.y = Mathf.Clamp(_frameVelocity.y, -_stats.DodgeEndSpeed, _stats.DodgeEndSpeed);
                    }

                    DodgingChanged?.Invoke();
                }
            }

            if (!_canDodge)
            {
                _dodgeCooldownTimer -= Time.fixedDeltaTime;
                if (_dodgeCooldownTimer <= 0) _canDodge = true;
            }
        }

        #endregion


        private void HandleLedgeCorrection()
        {
            if (_frameVelocity.y <= 0) return;

            // Simple nudge if we hit a ceiling corner
            var headY = _col.bounds.max.y;
            var leftHit = Physics2D.Raycast(new Vector2(_col.bounds.min.x, headY + 0.05f), Vector2.up, 0.1f, ~_stats.PlayerLayer);
            var rightHit = Physics2D.Raycast(new Vector2(_col.bounds.max.x, headY + 0.05f), Vector2.up, 0.1f, ~_stats.PlayerLayer);

            if (leftHit ^ rightHit)
            {
                _frameVelocity.x += leftHit ? 2 : -2;
            }
        }

        private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a PlayerMovementData asset to the Player Controller's Stats slot", this);
        }
#endif
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public bool DodgeDown;
        public bool CrouchDown;
        public Vector2 Move;
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public event Action DodgingChanged;
        public event Action CrouchingChanged;
        public bool IsDodging { get; }
        public bool IsCrouching { get; }
        public bool IsOnWall { get; }
        public bool IsWallSliding { get; }
        public Vector2 FrameInput { get; }
    }
}