using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectOni.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float dodgeForce = 15f;
        [SerializeField] private float dodgeDuration = 0.2f;

        [Header("References")]
        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private bool _isDodgeCooldown;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
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
            ApplyMovement();
        }

        private void ApplyMovement()
        {
            if (_isDodgeCooldown) return; // Can't move while dodging

            Vector2 velocity = _rb.linearVelocity;
            velocity.x = _moveInput.x * moveSpeed;
            _rb.linearVelocity = velocity;
        }

        private void Jump()
        {
            // Simple jump logic (expects ground check in a real scenario)
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        private void Dodge()
        {
            _isDodgeCooldown = true;
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
