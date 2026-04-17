using UnityEngine;
using ProjectOni.Core;

namespace ProjectOni.Enemies
{
    public class BossAI : EnemyAI
    {
        [Header("Phase Settings")]
        [SerializeField] private float phaseTwoThreshold = 0.5f;

        private bool _isPhaseTwoActive = false;
        private EnemyStats _stats;

        protected override void Start()
        {
            base.Start();
            _stats = GetComponent<EnemyStats>();
        }

        protected override void Update()
        {
            if (currentState == EnemyState.Death) return;
            
            CheckPhaseTransition();
            base.Update();
        }

        private void CheckPhaseTransition()
        {
            // Simplified phase transition logic
            if (!_isPhaseTwoActive && 0.5f < phaseTwoThreshold) // Normally check _stats.CurrentPercentage
            {
                ActivatePhaseTwo();
            }
        }

        private void ActivatePhaseTwo()
        {
            _isPhaseTwoActive = true;
            Debug.Log("Boss Phase Two Activated!");
            moveSpeed *= 1.5f;
            attackRadius *= 1.25f;
            detectionRadius *= 1.5f;
            // Unlock new attacks pattern or triggers
        }

        public override void ChangeState(EnemyState newState)
        {
            base.ChangeState(newState);
            // Additional Boss logic for state changes (e.g., roar on phase change)
        }
    }
}
