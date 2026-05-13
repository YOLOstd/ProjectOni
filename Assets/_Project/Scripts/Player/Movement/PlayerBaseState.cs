using UnityEngine;
using PurrNet.StateMachine;

namespace ProjectOni.Player.Movement
{
    public abstract class PlayerBaseState : StateNode
    {
        protected PlayerMovementStateMachine StateMachine;
        protected PlayerController Controller;

        protected virtual void Awake()
        {
            StateMachine = GetComponentInParent<PlayerMovementStateMachine>();
            Controller = GetComponentInParent<PlayerController>();
        }

        public override void Enter() { }
        public override void Exit() { }
        public override void StateUpdate() { }
        
        protected virtual void FixedUpdate()
        {
            if (isCurrentState)
            {
                StateFixedUpdate();
            }
        }

        public virtual void StateFixedUpdate() { }
    }
}
