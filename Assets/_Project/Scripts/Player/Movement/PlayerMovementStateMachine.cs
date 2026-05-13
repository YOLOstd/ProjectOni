using UnityEngine;
using System;
using PurrNet;
using PurrNet.StateMachine;

namespace ProjectOni.Player.Movement
{
    public class PlayerMovementStateMachine : NetworkBehaviour
    {
        [Header("References")]
        public PlayerController Controller;
        public Animator Animator;
        public PurrNet.StateMachine.StateMachine machine;

        [Header("Current State Debug")]
        [SerializeField] private string currentStateName;

        // State Instances (Components)
        public PlayerIdleState IdleState { get; private set; }
        public PlayerMoveState MoveState { get; private set; }
        public PlayerAirborneState AirborneState { get; private set; }
        public PlayerDodgeState DodgeState { get; private set; }

        public event Action<StateNode> StateChanged;
        
        private float _lastJumpPressedTime = float.MinValue;
        public bool HasBufferedJump => Time.time < _lastJumpPressedTime + Controller.Stats.JumpBuffer;
        public void ClearJumpBuffer() => _lastJumpPressedTime = float.MinValue;

        public bool IsJumpHeld => ProjectOni.Managers.InputManager.Instance != null && ProjectOni.Managers.InputManager.Instance.IsJumpHeld;

        private void Awake()
        {
            if (Controller == null) Controller = GetComponent<PlayerController>();
            if (Animator == null) Animator = GetComponentInChildren<Animator>();
            if (machine == null) machine = GetComponent<PurrNet.StateMachine.StateMachine>();

            // Find States
            IdleState = GetComponent<PlayerIdleState>();
            MoveState = GetComponent<PlayerMoveState>();
            AirborneState = GetComponent<PlayerAirborneState>();
            DodgeState = GetComponent<PlayerDodgeState>();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();
            if (isOwner)
            {
                var input = ProjectOni.Managers.InputManager.Instance;
                if (input != null)
                {
                    input.JumpPressed += OnJumpPressed;
                    input.DodgePressed += OnDodgePressed;
                }
                machine.onStateChanged += OnInternalStateChanged;
            }
        }

        protected override void OnDespawned(bool asServer)
        {
            base.OnDespawned(asServer);
            if (isOwner)
            {
                var input = ProjectOni.Managers.InputManager.Instance;
                if (input != null)
                {
                    input.JumpPressed -= OnJumpPressed;
                    input.DodgePressed -= OnDodgePressed;
                }
                machine.onStateChanged -= OnInternalStateChanged;
            }
        }

        private void OnEnable()
        {
            if (isSpawned && isOwner)
            {
                var input = ProjectOni.Managers.InputManager.Instance;
                if (input != null)
                {
                    input.JumpPressed += OnJumpPressed;
                    input.DodgePressed += OnDodgePressed;
                }
            }
        }

        private void OnDisable()
        {
            if (isSpawned && isOwner)
            {
                var input = ProjectOni.Managers.InputManager.Instance;
                if (input != null)
                {
                    input.JumpPressed -= OnJumpPressed;
                    input.DodgePressed -= OnDodgePressed;
                }
            }
        }

        private void Update()
        {
            if (!isOwner) return;
            Controller.UpdateDodgeCooldown();
        }

        private void OnInternalStateChanged(StateNode previousState, StateNode newState)
        {
            if (newState == null) return;
            currentStateName = newState.GetType().Name;
            StateChanged?.Invoke(newState);
        }

        private void OnJumpPressed()
        {
            if (Controller.IsOnWall && !Controller.IsGrounded)
            {
                Controller.ExecuteWallJump(Controller.WallDir);
                machine.SetState(AirborneState);
            }
            else if (CanJump())
            {
                Controller.ExecuteJump();
                machine.SetState(AirborneState);
            }
            else
            {
                _lastJumpPressedTime = Time.time;
            }
        }

        private void OnDodgePressed()
        {
            if (Controller.CanDodge)
            {
                machine.SetState(DodgeState);
            }
        }

        private bool CanJump() => Controller.IsGrounded || Controller.CanCoyote || Controller.AirJumpsRemaining > 0 || Controller.IsOnWall;
        
        public bool IsJumpActionHeld() => ProjectOni.Managers.InputManager.Instance != null && ProjectOni.Managers.InputManager.Instance.IsJumpHeld;
    }
}
