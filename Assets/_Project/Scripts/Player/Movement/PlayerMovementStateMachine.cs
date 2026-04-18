using UnityEngine;
using System;
using ProjectOni.Player;

namespace ProjectOni.Player.Movement
{
    public enum PlayerMovementStateEnum { Idle, Move, Airborne, Dodge, Attack }

    public class PlayerMovementStateMachine : MonoBehaviour
    {
        [Header("References")]
        public PlayerController Controller;
        public InputReader InputReader;
        public Animator Animator;

        [Header("Current State Debug")]
        [SerializeField] private string currentStateName;

        public PlayerBaseState CurrentState { get; private set; }

        // State Instances
        public PlayerIdleState IdleState { get; private set; }
        public PlayerMoveState MoveState { get; private set; }
        public PlayerAirborneState AirborneState { get; private set; }
        public PlayerDodgeState DodgeState { get; private set; }

        public event Action<PlayerBaseState> StateChanged;

        public bool IsJumpHeld => InputReader != null && InputReader.IsJumpHeld;

        private void Awake()
        {
            if (Controller == null) Controller = GetComponent<PlayerController>();
            if (InputReader == null) InputReader = GetComponent<InputReader>();
            if (Animator == null) Animator = GetComponentInChildren<Animator>();

            // Initialize States
            IdleState = new PlayerIdleState(this);
            MoveState = new PlayerMoveState(this);
            AirborneState = new PlayerAirborneState(this);
            DodgeState = new PlayerDodgeState(this);
        }

        private void Start()
        {
            ChangeState(IdleState);
        }

        private void OnEnable()
        {
            InputReader.JumpPressed += OnJumpPressed;
            InputReader.DodgePressed += OnDodgePressed;
        }

        private void OnDisable()
        {
            InputReader.JumpPressed -= OnJumpPressed;
            InputReader.DodgePressed -= OnDodgePressed;
        }

        private void Update()
        {
            Controller.UpdateDodgeCooldown();
            CurrentState?.Update();
        }

        private void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }

        public void ChangeState(PlayerBaseState newState)
        {
            if (CurrentState == newState) return;

            CurrentState?.Exit();
            CurrentState = newState;
            currentStateName = newState.GetType().Name;
            CurrentState.Enter();

            StateChanged?.Invoke(CurrentState);
        }

        private void OnJumpPressed()
        {
            if (Controller.IsOnWall && !Controller.IsGrounded)
            {
                Controller.ExecuteWallJump(Controller.WallDir);
                ChangeState(AirborneState);
            }
            else if (CanJump())
            {
                Controller.ExecuteJump();
                ChangeState(AirborneState);
            }
        }

        private void OnDodgePressed()
        {
            if (Controller.CanDodge)
            {
                ChangeState(DodgeState);
            }
        }

        private bool CanJump() => Controller.IsGrounded || Controller.CanCoyote || Controller.AirJumpsRemaining > 0 || Controller.IsOnWall;
        
        // Helper to check if Jump is still held for variable jump height
        // This is tricky with events, so we'll check the InputReader directly
        public bool IsJumpActionHeld() => InputReader != null && InputReader.IsJumpHeld;
    }
}
