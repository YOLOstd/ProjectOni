using UnityEngine;
using ProjectOni.Player;

namespace ProjectOni.Player.Movement
{
    public class PlayerDodgeState : PlayerBaseState
    {
        private float _timer;

        public override void Enter()
        {
            _timer = Controller.Stats.DodgeDuration;
            
            // Visuals (Everyone)
            StateMachine.Animator.SetTrigger("Dodge");

            // Physics (Owner only)
            if (isOwner)
            {
                if (StateMachine.Combat != null)
                {
                    StateMachine.Combat.CancelGlobalLock();
                }

                if (StateMachine.Dodge != null)
                {
                    StateMachine.Dodge.Initiate(ProjectOni.Managers.InputManager.Instance.MoveDirection);
                }
            }
        }

        public override void StateUpdate()
        {
            if (!isOwner) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                if (Controller.IsGrounded)
                    machine.SetState(StateMachine.IdleState);
                else
                    machine.SetState(StateMachine.AirborneState);
            }
        }

        public override void StateFixedUpdate()
        {
            if (!isOwner) return;
            if (StateMachine.Dodge != null)
            {
                StateMachine.Dodge.HandleMovement();
            }
        }

        public override void Exit()
        {
            if (isOwner)
            {
                if (StateMachine.Dodge != null)
                {
                    StateMachine.Dodge.End();
                }
            }
        }
    }
}
