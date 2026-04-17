using UnityEngine;
using ProjectOni.Player;

namespace ProjectOni.Player.Movement
{
    public class PlayerMoveState : PlayerBaseState
    {
        public PlayerMoveState(PlayerMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() { }

        public override void Update()
        {
            if (!Controller.IsGrounded)
            {
                StateMachine.ChangeState(StateMachine.AirborneState);
                return;
            }

            if (Input.MoveDirection.x == 0 && Controller.CurrentVelocity.x == 0)
            {
                StateMachine.ChangeState(StateMachine.IdleState);
                return;
            }

            Controller.SetCrouching(Input.IsCrouchHeld);
        }

        public override void FixedUpdate()
        {
            Controller.HandleHorizontalMovement(Input.MoveDirection.x);
        }

        public override void Exit() { }
    }
}
