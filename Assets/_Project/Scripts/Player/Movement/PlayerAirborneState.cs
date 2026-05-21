using UnityEngine;
using ProjectOni.Player;

namespace ProjectOni.Player.Movement
{
    public class PlayerAirborneState : PlayerBaseState
    {
        private bool _jumpCutApplied;

        public override void Enter()
        {
            _jumpCutApplied = false;
        }

        public override void StateUpdate()
        {
            if (!isOwner) return;

            if (Controller.IsGrounded && Controller.CurrentVelocity.y <= 0)
            {
                if (StateMachine.HasBufferedJump)
                {
                    StateMachine.ClearJumpBuffer();
                    Controller.ExecuteJump();
                    // Stay in AirborneState since we jumped again
                }
                else
                {
                    if (ProjectOni.Managers.InputManager.Instance.MoveDirection.x != 0)
                        machine.SetState(StateMachine.MoveState);
                    else
                        machine.SetState(StateMachine.IdleState);
                }
                return;
            }

            // Handle Jump Cut (Variable Jump Height)
            if (!_jumpCutApplied && Controller.CurrentVelocity.y > 0 && !StateMachine.IsJumpHeld)
            {
                Controller.ApplyJumpCut();
                _jumpCutApplied = true;
            }

            // Wall Slide Detection
            var inputDir = ProjectOni.Managers.InputManager.Instance.MoveDirection;
            bool isWallSliding = Controller.IsOnWall && !Controller.IsGrounded && Controller.CurrentVelocity.y < 0 &&
                                (inputDir.x != 0 && Mathf.Sign(inputDir.x) == Controller.WallDir);
            Controller.SetWallSliding(isWallSliding);
        }

        public override void StateFixedUpdate()
        {
            if (!isOwner) return;
            
            if (StateMachine.Combat != null && StateMachine.Combat.IsAntiGravityActive)
            {
                // During anti-gravity (aerial attack), we skip standard horizontal movement
                // to prevent AirDeceleration from instantly killing our momentum.
                // HandleAirborneAttackGravity applies a uniform 2D slow-mo drag instead.
                Controller.HandleAirborneAttackGravity();
            }
            else
            {
                float inputX = ProjectOni.Managers.InputManager.Instance.MoveDirection.x;
                if (StateMachine.Combat != null && StateMachine.Combat.IsGlobalLocked)
                {
                    inputX = 0f;
                }
                Controller.HandleHorizontalMovement(inputX);
                Controller.HandleGravity();
            }
        }
    }
}
