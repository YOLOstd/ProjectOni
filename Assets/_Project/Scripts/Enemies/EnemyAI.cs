using System;
using System.Collections;
using UnityEngine;
using PurrNet;
using ProjectOni.Core;
using ProjectOni.Data;

namespace ProjectOni.Enemies
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Hurt,
        Dead
    }

    [RequireComponent(typeof(EnemyController))]
    [RequireComponent(typeof(EnemyCombat))]
    [RequireComponent(typeof(HealthComponent))]
    public class EnemyAI : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyDataSO _data;
        [SerializeField] private LayerMask _playerLayer;

        [Header("Movement Speeds")]
        [SerializeField] private float _patrolSpeed = 2f;
        [SerializeField] private float _chaseSpeed = 4.5f;

        [Header("Patrol Settings")]
        [SerializeField] private float _patrolWaitTime = 1.5f;

        private EnemyController _controller;
        private EnemyCombat _combat;
        private HealthComponent _health;

        private EnemyState _currentState = EnemyState.Idle;
        private Transform _targetPlayer;
        private Vector3 _startPosition;
        private Vector3 _patrolTarget;
        private bool _isPatrolWaiting;
        private float _patrolWaitEndTime;

        // Poise / Stagger system
        private float _poiseDamageAccumulator;
        private float _staggerCooldownEndTime;
        private float _hurtStateEndTime;
        private float _lastHealth;

        public EnemyState CurrentState => _currentState;

        private void Awake()
        {
            _controller = GetComponent<EnemyController>();
            _combat = GetComponent<EnemyCombat>();
            _health = GetComponent<HealthComponent>();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (!isServer) return;

            // Keep the enemy purely server-owned (unowned by clients) so the server has master authority
            // over its ownerAuth SyncVars on spawn and in combat.

            _currentState           = EnemyState.Idle;
            _targetPlayer           = null;
            _poiseDamageAccumulator = 0f;
            _staggerCooldownEndTime = 0f;
            _hurtStateEndTime       = 0f;
            _startPosition          = transform.position;
            _patrolTarget           = _startPosition;
            _lastHealth             = _health.Current;
            
            _health.OnHealthChanged += OnHealthChanged;
            _health.OnDied += OnDied;

            TransitionToState(EnemyState.Patrol);
        }

        protected override void OnDespawned(bool asServer)
        {
            base.OnDespawned(asServer);

            if (isServer)
            {
                _health.OnHealthChanged -= OnHealthChanged;
                _health.OnDied -= OnDied;
            }
        }

        private void Update()
        {
            if (!isServer) return;

            if (_currentState == EnemyState.Dead) return;

            // Decay poise damage accumulator over time
            if (_poiseDamageAccumulator > 0f && _data != null)
            {
                // Decay complete poise within 3 seconds
                float decayRate = _data.staggerDamageThreshold / 3f;
                _poiseDamageAccumulator = Mathf.Max(0f, _poiseDamageAccumulator - decayRate * Time.deltaTime);
            }

            UpdateStateMachine();
        }

        private void UpdateStateMachine()
        {
            switch (_currentState)
            {
                case EnemyState.Idle:
                    _controller.SetHorizontalVelocity(0f);
                    FindNearestTarget();
                    if (_targetPlayer != null)
                    {
                        TransitionToState(EnemyState.Chase);
                    }
                    else
                    {
                        TransitionToState(EnemyState.Patrol);
                    }
                    break;

                case EnemyState.Patrol:
                    FindNearestTarget();
                    if (_targetPlayer != null)
                    {
                        TransitionToState(EnemyState.Chase);
                        return;
                    }

                    ExecutePatrol();
                    break;

                case EnemyState.Chase:
                    if (_targetPlayer == null || IsTargetDead())
                    {
                        _targetPlayer = null;
                        TransitionToState(EnemyState.Patrol);
                        return;
                    }

                    ExecuteChase();
                    break;

                case EnemyState.Attack:
                    _controller.SetHorizontalVelocity(0f);
                    // Stay in attack state until lock ends
                    if (!_combat.IsLocked)
                    {
                        TransitionToState(EnemyState.Chase);
                    }
                    break;

                case EnemyState.Hurt:
                    _controller.SetHorizontalVelocity(0f);
                    if (Time.time >= _hurtStateEndTime)
                    {
                        TransitionToState(EnemyState.Chase);
                    }
                    break;
            }
        }

        private void TransitionToState(EnemyState newState)
        {
            if (_currentState == EnemyState.Dead) return;

            _currentState = newState;

            switch (_currentState)
            {
                case EnemyState.Idle:
                    _controller.SetHorizontalVelocity(0f);
                    break;

                case EnemyState.Patrol:
                    _isPatrolWaiting = false;
                    _patrolTarget = GetRandomPatrolPoint();
                    break;

                case EnemyState.Chase:
                    _isPatrolWaiting = false;
                    break;

                case EnemyState.Attack:
                    _controller.SetHorizontalVelocity(0f);
                    if (_targetPlayer != null)
                    {
                        Vector2 dir = (_targetPlayer.position - transform.position).normalized;
                        _controller.SetFacingDir((int)Mathf.Sign(dir.x));
                        _combat.ExecuteAttack(dir);
                    }
                    break;

                case EnemyState.Hurt:
                    _controller.SetHorizontalVelocity(0f);
                    _hurtStateEndTime = Time.time + 0.5f; // Hurt duration/stun lock
                    _combat.CancelVisuals();
                    // ObserversRpc triggers animator hurt visually
                    RpcTriggerHurt();
                    break;

                case EnemyState.Dead:
                    _controller.SetHorizontalVelocity(0f);
                    // Fire global events
                    GameEvents.TriggerBossDefeated(); // Keep compatibility or custom
                    GameEvents.TriggerEnemyDied(_controller);
                    StartCoroutine(DeathSequence(_data.deathAnimationDuration));
                    break;
            }
        }

        private void ExecutePatrol()
        {
            if (_isPatrolWaiting)
            {
                _controller.SetHorizontalVelocity(0f);
                if (Time.time >= _patrolWaitEndTime)
                {
                    _isPatrolWaiting = false;
                    _patrolTarget = GetRandomPatrolPoint();
                }
                return;
            }

            Vector3 diff = _patrolTarget - transform.position;
            diff.y = 0f; // ignore vertical diffs for flat patrol logic

            if (diff.magnitude < 0.2f)
            {
                _isPatrolWaiting = true;
                _patrolWaitEndTime = Time.time + _patrolWaitTime;
                _controller.SetHorizontalVelocity(0f);
                return;
            }

            float dirX = Mathf.Sign(diff.x);
            _controller.SetHorizontalVelocity(dirX * _patrolSpeed);
        }

        private void ExecuteChase()
        {
            if (_targetPlayer == null) return;

            Vector3 diff = _targetPlayer.position - transform.position;
            float horizontalDistance = Mathf.Abs(diff.x);

            // Turn to face target
            _controller.SetFacingDir((int)Mathf.Sign(diff.x));

            if (horizontalDistance <= _data.attackRange)
            {
                TransitionToState(EnemyState.Attack);
            }
            else if (diff.magnitude > _data.aggroRange * 1.2f)
            {
                _targetPlayer = null;
                TransitionToState(EnemyState.Patrol);
            }
            else
            {
                float dirX = Mathf.Sign(diff.x);
                _controller.SetHorizontalVelocity(dirX * _chaseSpeed);
            }
        }

        private Vector3 GetRandomPatrolPoint()
        {
            float randOffset = UnityEngine.Random.Range(-_data.patrolRadius, _data.patrolRadius);
            return _startPosition + new Vector3(randOffset, 0f, 0f);
        }

        private void FindNearestTarget()
        {
            if (_data == null) return;

            Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, _data.aggroRange, _playerLayer);
            float nearestDist = float.MaxValue;
            Transform nearest = null;

            foreach (var col in results)
            {
                if (col.isTrigger) continue; // ignore trigger colliders like hurtboxes

                // Make sure player has health component and is alive
                var hp = col.GetComponentInParent<HealthComponent>();
                if (hp != null && hp.IsDead) continue;

                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = col.transform;
                }
            }

            _targetPlayer = nearest;
        }

        private bool IsTargetDead()
        {
            if (_targetPlayer == null) return true;
            var hp = _targetPlayer.GetComponentInParent<HealthComponent>();
            return hp != null && hp.IsDead;
        }

        private void OnHealthChanged(float current, float max)
        {
            if (!isServer) return;

            float damage = _lastHealth - current;
            _lastHealth = current;

            if (damage > 0f && _data != null)
            {
                _poiseDamageAccumulator += damage;

                if (_poiseDamageAccumulator >= _data.staggerDamageThreshold && Time.time > _staggerCooldownEndTime)
                {
                    _poiseDamageAccumulator = 0f;
                    _staggerCooldownEndTime = Time.time + _data.staggerCooldown;
                    TransitionToState(EnemyState.Hurt);
                }
            }
        }

        private void OnDied()
        {
            if (!isServer) return;
            TransitionToState(EnemyState.Dead);
        }

        private IEnumerator DeathSequence(float delay)
        {
            Debug.Log($"[EnemyAI] DeathSequence started on server for {gameObject.name}. Waiting {delay}s for animation...");
            yield return new WaitForSeconds(delay);
            Debug.Log($"[EnemyAI] DeathSequence delay completed for {gameObject.name}. Calling Despawn(). isSpawned: {isSpawned}");
            try
            {
                Despawn();
                Debug.Log($"[EnemyAI] Despawn() executed successfully for {gameObject.name}.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[EnemyAI] Exception thrown during Despawn() on {gameObject.name}: {e.Message}\n{e.StackTrace}");
            }
        }

        [ObserversRpc]
        private void RpcTriggerHurt()
        {
            if (TryGetComponent<Animator>(out var anim))
            {
                anim.SetTrigger("Hurt");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_data == null) return;

            // Aggro Range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _data.aggroRange);

            // Attack Range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _data.attackRange);

            // Patrol Radius
            Gizmos.color = Color.green;
            Vector3 startPos = Application.isPlaying ? _startPosition : transform.position;
            Gizmos.DrawLine(startPos - new Vector3(_data.patrolRadius, 0f, 0f), startPos + new Vector3(_data.patrolRadius, 0f, 0f));
        }
    }
}
