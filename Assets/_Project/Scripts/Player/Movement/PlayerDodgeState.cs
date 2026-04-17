using UnityEngine;
using ProjectOni.Player;

namespace ProjectOni.Player.Movement
{
    public class PlayerDodgeState : PlayerBaseState
    {
        private float _timer;

        public PlayerDodgeState(PlayerMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            _timer = Controller.Stats.DodgeDuration;
            Controller.InitiateDodge(Input.MoveDirection);
        }

        public override void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                if (Controller.IsGrounded)
                    StateMachine.ChangeState(StateMachine.IdleState);
                else
                    StateMachine.ChangeState(StateMachine.AirborneState);
            }
        }

        public override void FixedUpdate()
        {
            Controller.HandleDodgeMovement();
        }

        public override void Exit()
        {
            Controller.EndDodge();
        }
    }
}
