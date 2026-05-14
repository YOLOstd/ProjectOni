using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace ProjectOni.Managers
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dodgeAction;

        private InputAction _interactAction;
        private InputAction _toggleMenuAction;

        public Vector2 MoveDirection { get; private set; }

        public bool IsJumpHeld => _jumpAction != null && _jumpAction.IsPressed();
        
        public event Action JumpPressed;
        public event Action JumpReleased;
        public event Action DodgePressed;
        public event Action InteractPressed;
        public event Action MenuTogglePressed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _playerInput = GetComponent<PlayerInput>();
            
            _moveAction = _playerInput.actions["Move"];
            _jumpAction = _playerInput.actions["Jump"];
            _dodgeAction = _playerInput.actions["Dodge"];

            _interactAction = _playerInput.actions.FindAction("Interact");
            _toggleMenuAction = _playerInput.actions.FindAction("ToggleMenu");

        }

        private void OnEnable()
        {
            if (_jumpAction != null) _jumpAction.performed += OnJumpTriggered;
            if (_jumpAction != null) _jumpAction.canceled += OnJumpCanceled;
            if (_dodgeAction != null) _dodgeAction.performed += OnDodgeTriggered;
            if (_interactAction != null) _interactAction.performed += OnInteractTriggered;
            
            if (_toggleMenuAction != null)
            {
                _toggleMenuAction.performed += OnToggleMenuTriggered;
                _toggleMenuAction.Enable();
            }
        }

        private void OnDisable()
        {
            if (_jumpAction != null)
            {
                _jumpAction.performed -= OnJumpTriggered;
                _jumpAction.canceled -= OnJumpCanceled;
            }
            if (_dodgeAction != null) _dodgeAction.performed -= OnDodgeTriggered;
            if (_interactAction != null) _interactAction.performed -= OnInteractTriggered;
            
            if (_toggleMenuAction != null)
                _toggleMenuAction.performed -= OnToggleMenuTriggered;
        }

        private void Start()
        {
            EnableGameControls();
        }

        private void Update()
        {
            if (_moveAction == null) return;
            
            MoveDirection = _moveAction.ReadValue<Vector2>();


            if (MoveDirection != Vector2.zero)
            {
                // Debug.Log($"InputManager: Moving {MoveDirection}");
            }
        }

        private void OnJumpTriggered(InputAction.CallbackContext obj) 
        {
            JumpPressed?.Invoke();
        }
        private void OnJumpCanceled(InputAction.CallbackContext obj) => JumpReleased?.Invoke();
        private void OnDodgeTriggered(InputAction.CallbackContext obj) 
        {
            DodgePressed?.Invoke();
        }
        private void OnInteractTriggered(InputAction.CallbackContext obj) => InteractPressed?.Invoke();
        private void OnToggleMenuTriggered(InputAction.CallbackContext obj) 
        {
            MenuTogglePressed?.Invoke();
        }

        public void EnableGameControls() 
        {
            Debug.Log("InputManager: Switching to 'Player' action map.");
            _playerInput.SwitchCurrentActionMap("Player"); 
        }

        public void EnableMenuControls() 
        {
            Debug.Log("InputManager: Switching to 'UI' action map.");
            _playerInput.SwitchCurrentActionMap("UI");
        }
    }
}
