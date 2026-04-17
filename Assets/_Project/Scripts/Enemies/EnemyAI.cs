using UnityEngine;

namespace ProjectOni.Enemies
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Death
    }

    /// <summary>
    /// Base class for all enemy AI behavior.
    /// Controls switching between states: Patrol, Chase, Attack.
    /// </summary>
    public abstract class EnemyAI : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] protected float detectionRadius = 10f;
        [SerializeField] protected float attackRadius = 2f;
        [SerializeField] protected float moveSpeed = 3f;

        protected EnemyState currentState = EnemyState.Idle;
        protected Transform target;

        protected virtual void Start()
        {
            // Usually find player by tag or static reference
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        protected virtual void Update()
        {
            if (currentState == EnemyState.Death) return;

            UpdateState();
        }

        protected virtual void UpdateState()
        {
            if (target == null) return;

            float distance = Vector2.Distance(transform.position, target.position);

            if (distance < attackRadius)
            {
                ChangeState(EnemyState.Attack);
            }
            else if (distance < detectionRadius)
            {
                ChangeState(EnemyState.Chase);
            }
            else
            {
                ChangeState(EnemyState.Patrol);
            }
        }

        public virtual void ChangeState(EnemyState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            // Handle transition logic
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);
        }
    }
}
