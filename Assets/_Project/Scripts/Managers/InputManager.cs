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
        
        // Gameplay Actions
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dodgeAction;
        private InputAction _interactAction;

        // UI / Menu Actions
        private InputAction _playerToggleMenuAction;
        private InputAction _uiToggleMenuAction;

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
            
            // Explicitly cache the ToggleMenu action from BOTH maps
            var playerMap = _playerInput.actions.FindActionMap("Player");
            _playerToggleMenuAction = playerMap != null ? playerMap.FindAction("ToggleMenu") : null;
            
            var uiMap = _playerInput.actions.FindActionMap("UI");
            _uiToggleMenuAction = uiMap != null ? uiMap.FindAction("ToggleMenu") : null;
        }

        private void OnEnable()
        {
            if (_jumpAction != null) _jumpAction.performed += OnJumpTriggered;
            if (_jumpAction != null) _jumpAction.canceled += OnJumpCanceled;
            if (_dodgeAction != null) _dodgeAction.performed += OnDodgeTriggered;
            if (_interactAction != null) _interactAction.performed += OnInteractTriggered;
            
            // Subscribe safely. DO NOT call .Enable() manually here!
            if (_playerToggleMenuAction != null)
                _playerToggleMenuAction.performed += OnToggleMenuTriggered;
            else
                Debug.LogError("InputManager: 'ToggleMenu' missing in 'Player' map!");

            if (_uiToggleMenuAction != null)
                _uiToggleMenuAction.performed += OnToggleMenuTriggered;
            else
                Debug.LogError("InputManager: 'ToggleMenu' missing in 'UI' map!");
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
            
            if (_playerToggleMenuAction != null)
                _playerToggleMenuAction.performed -= OnToggleMenuTriggered;
            if (_uiToggleMenuAction != null)
                _uiToggleMenuAction.performed -= OnToggleMenuTriggered;
        }

        private void Start()
        {
            EnableGameControls();
        }

        private void Update()
        {
            if (_moveAction == null) return;
            MoveDirection = _moveAction.ReadValue<Vector2>();
        }

        private void OnJumpTriggered(InputAction.CallbackContext obj) => JumpPressed?.Invoke();
        private void OnJumpCanceled(InputAction.CallbackContext obj) => JumpReleased?.Invoke();
        private void OnDodgeTriggered(InputAction.CallbackContext obj) => DodgePressed?.Invoke();
        private void OnInteractTriggered(InputAction.CallbackContext obj) => InteractPressed?.Invoke();
        
        private void OnToggleMenuTriggered(InputAction.CallbackContext obj) 
        {
            MenuTogglePressed?.Invoke();
        }

        public void EnableGameControls() 
        {
            _playerInput.SwitchCurrentActionMap("Player"); 
        }

        public void EnableMenuControls() 
        {
            _playerInput.SwitchCurrentActionMap("UI");
        }
    }
}