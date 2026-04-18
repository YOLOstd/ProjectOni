using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace ProjectOni.Player
{
    /// <summary>
    /// Acts as the "Ears" of the player. Listens to Unity's Hardware Input
    /// and translates it into game-ready properties and events.
    /// </summary>
    public class InputReader : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dodgeAction;
        private InputAction _crouchAction;
        private InputAction _toggleMenuAction;

        public Vector2 MoveDirection { get; private set; }
        public bool IsCrouchHeld { get; private set; }
        public bool IsJumpHeld => _jumpAction != null && _jumpAction.IsPressed();
        
        // Events for discrete actions
        public event Action JumpPressed;
        public event Action JumpReleased;
        public event Action DodgePressed;
        public event Action MenuTogglePressed;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            
            _moveAction = _playerInput.actions["Move"];
            _jumpAction = _playerInput.actions["Jump"];
            _dodgeAction = _playerInput.actions["Dodge"];
            _crouchAction = _playerInput.actions["Crouch"];
            
            // Safe lookup for experimental/new actions
            _toggleMenuAction = _playerInput.actions.FindAction("ToggleMenu", false);
        }

        private void OnEnable()
        {
            _jumpAction.performed += OnJumpTriggered;
            _jumpAction.canceled += OnJumpCanceled;
            _dodgeAction.performed += OnDodgeTriggered;
            
            if (_toggleMenuAction != null)
                _toggleMenuAction.performed += OnToggleMenuTriggered;
        }

        private void OnDisable()
        {
            _jumpAction.performed -= OnJumpTriggered;
            _jumpAction.canceled -= OnJumpCanceled;
            _dodgeAction.performed -= OnDodgeTriggered;
            
            if (_toggleMenuAction != null)
                _toggleMenuAction.performed -= OnToggleMenuTriggered;
        }

        private void Update()
        {
            MoveDirection = _moveAction.ReadValue<Vector2>();
            IsCrouchHeld = _crouchAction.IsPressed();
        }

        private void OnJumpTriggered(InputAction.CallbackContext obj) => JumpPressed?.Invoke();
        private void OnJumpCanceled(InputAction.CallbackContext obj) => JumpReleased?.Invoke();
        private void OnDodgeTriggered(InputAction.CallbackContext obj) => DodgePressed?.Invoke();
        private void OnToggleMenuTriggered(InputAction.CallbackContext obj) => MenuTogglePressed?.Invoke();
    }
}
