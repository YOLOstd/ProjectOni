using UnityEngine;
using ProjectOni.Player;

namespace ProjectOni.Player.Movement
{
    public class PlayerIdleState : PlayerBaseState
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
            if (input.MoveDirection.x != 0)
            {
                machine.SetState(StateMachine.MoveState);
                return;
            }


        }

        public override void StateFixedUpdate()
        {
            if (!isOwner) return;
            // Apply grounding force and handle stopping horizontal momentum
            Controller.HandleHorizontalMovement(0);
        }
    }
}
