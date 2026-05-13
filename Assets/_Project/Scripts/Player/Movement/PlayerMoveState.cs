using UnityEngine;
using ProjectOni.Player;

namespace ProjectOni.Player.Movement
{
    public class PlayerMoveState : PlayerBaseState
    {
        public override void StateUpdate()
        {
            if (!isOwner) return;

            if (!Controller.IsGrounded)
            {
                machine.SetState(StateMachine.AirborneState);
                return;
            }

            var input = ProjectOni.Managers.InputManager.Instance;
            if (input.MoveDirection.x == 0 && Controller.CurrentVelocity.x == 0)
            {
                machine.SetState(StateMachine.IdleState);
                return;
            }


        }

        public override void StateFixedUpdate()
        {
            if (!isOwner) return;
            Controller.HandleHorizontalMovement(ProjectOni.Managers.InputManager.Instance.MoveDirection.x);
        }
    }
}
