using UnityEngine;

namespace ProjectOni.Player
{
    public enum PlayerState
    {
        Idle,
        Run,
        Air,
        Attack,
        Dodge
    }

    /// <summary>
    /// Simple Player State Machine to manage animation triggers and movement constraints.
    /// </summary>
    public class PlayerStateMachine : MonoBehaviour
    {
        [Header("Current State")]
        [SerializeField] private PlayerState currentState;

        private Animator _animator;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
        }

        public void ChangeState(PlayerState newState)
        {
            if (currentState == newState) return;

            // Exit current state logic
            ExitState(currentState);

            // Enter new state logic
            currentState = newState;
            EnterState(currentState);
        }

        private void EnterState(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.Idle:
                    UpdateAnimation("Idle");
                    break;
                case PlayerState.Run:
                    UpdateAnimation("Run");
                    break;
                case PlayerState.Air:
                    UpdateAnimation("Air");
                    break;
                case PlayerState.Attack:
                    UpdateAnimation("Attack");
                    break;
                case PlayerState.Dodge:
                    UpdateAnimation("Dodge");
                    break;
            }
        }

        private void ExitState(PlayerState state)
        {
            // Reset timers or flags if needed
        }

        private void UpdateAnimation(string triggerName)
        {
            if (_animator != null)
            {
                _animator.Play(triggerName);
            }
        }

        public bool CanMove() => currentState != PlayerState.Dodge;
        public bool CanAttack() => currentState != PlayerState.Dodge && currentState != PlayerState.Attack;
    }
}
