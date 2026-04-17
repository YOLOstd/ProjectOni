using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectOni.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float dodgeForce = 15f;
        [SerializeField] private float dodgeDuration = 0.2f;

        [Header("Ground Check")]
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private float groundCheckOffset = -0.5f;
        [SerializeField] private LayerMask groundLayer;

        [Header("References")]
        private Rigidbody2D _rb;
        private PlayerStateMachine _stateMachine;
        private Vector2 _moveInput;
        private bool _isDodgeCooldown;
        private bool _isGrounded;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _stateMachine = GetComponent<PlayerStateMachine>();
        }

        // Input Actions Handlers
        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        public void OnJump(InputValue value)
        {
            if (value.isPressed)
            {
                Jump();
            }
        }

        public void OnDodge(InputValue value)
        {
            if (value.isPressed && !_isDodgeCooldown)
            {
                Dodge();
            }
        }

        private void FixedUpdate()
        {
            CheckGround();
            ApplyMovement();
            UpdateStates();
        }

        private void CheckGround()
        {
            Vector2 checkPos = (Vector2)transform.position + Vector2.up * groundCheckOffset;
            _isGrounded = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer);
        }

        private void ApplyMovement()
        {
            if (_isDodgeCooldown) return;

            Vector2 velocity = _rb.linearVelocity;
            velocity.x = _moveInput.x * moveSpeed;
            _rb.linearVelocity = velocity;

            // Flip character
            if (_moveInput.x != 0)
            {
                transform.localScale = new Vector3(_moveInput.x > 0 ? 1 : -1, 1, 1);
            }
        }

        private void UpdateStates()
        {
            if (_stateMachine == null || _isDodgeCooldown) return;

            if (!_isGrounded)
            {
                _stateMachine.ChangeState(PlayerState.Air);
            }
            else if (_moveInput.x != 0)
            {
                _stateMachine.ChangeState(PlayerState.Run);
            }
            else
            {
                _stateMachine.ChangeState(PlayerState.Idle);
            }
        }

        private void Jump()
        {
            if (!_isGrounded) return;
            
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0); // Reset vertical velocity for consistent jump height
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        private void Dodge()
        {
            _isDodgeCooldown = true;
            if (_stateMachine != null) _stateMachine.ChangeState(PlayerState.Dodge);

            Vector2 dodgeDir = _moveInput != Vector2.zero ? _moveInput.normalized : Vector2.right * transform.localScale.x;
            _rb.AddForce(dodgeDir * dodgeForce, ForceMode2D.Impulse);
            
            Invoke(nameof(ResetDodge), dodgeDuration);
        }

        private void ResetDodge()
        {
            _isDodgeCooldown = false;
        }
    }
}
