using UnityEngine;
using ProjectOni.Player;

namespace ProjectOni.Player.Movement
{
    public class PlayerAirborneState : PlayerBaseState
    {
        private bool _jumpCutApplied;

        public PlayerAirborneState(PlayerMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            _jumpCutApplied = false;
        }

        public override void Update()
        {
            if (Controller.IsGrounded && Controller.CurrentVelocity.y <= 0)
            {
                if (Input.MoveDirection.x != 0)
                    StateMachine.ChangeState(StateMachine.MoveState);
                else
                    StateMachine.ChangeState(StateMachine.IdleState);
                return;
            }

            // Handle Jump Cut (Variable Jump Height)
            if (!_jumpCutApplied && Controller.CurrentVelocity.y > 0 && !StateMachine.IsJumpHeld)
            {
                Controller.ApplyJumpCut();
                _jumpCutApplied = true;
            }

            // Wall Slide Detection
            bool isWallSliding = Controller.IsOnWall && !Controller.IsGrounded && Controller.CurrentVelocity.y < 0 &&
                                (Input.MoveDirection.x != 0 && Mathf.Sign(Input.MoveDirection.x) == Controller.WallDir);
            Controller.SetWallSliding(isWallSliding);
        }

        public override void FixedUpdate()
        {
            Controller.HandleHorizontalMovement(Input.MoveDirection.x);
            Controller.HandleGravity();
        }

        public override void Exit() { }
    }
}
