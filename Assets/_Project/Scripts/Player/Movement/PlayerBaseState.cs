using UnityEngine;
using ProjectOni.Player;

namespace ProjectOni.Player.Movement
{
    public abstract class PlayerBaseState
    {
        protected PlayerMovementStateMachine StateMachine;
        protected PlayerController Controller;
        protected InputReader Input;

        public PlayerBaseState(PlayerMovementStateMachine stateMachine)
        {
            StateMachine = stateMachine;
            Controller = stateMachine.Controller;
            Input = stateMachine.InputReader;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void FixedUpdate();
        public abstract void Exit();
    }
}
