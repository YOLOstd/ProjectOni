using UnityEngine;
using ProjectOni.Player;

namespace ProjectOni.Player.Movement
{
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(PlayerMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            // Transition to Idle animation
        }

        public override void Update()
        {
            if (!Controller.IsGrounded)
            {
                StateMachine.ChangeState(StateMachine.AirborneState);
                return;
            }

            if (Input.MoveDirection.x != 0)
            {
                StateMachine.ChangeState(StateMachine.MoveState);
                return;
            }

            Controller.SetCrouching(Input.IsCrouchHeld);
        }

        public override void FixedUpdate()
        {
            // Apply grounding force and handle stopping horizontal momentum
            Controller.HandleHorizontalMovement(0);
        }

        public override void Exit() { }
    }
}
