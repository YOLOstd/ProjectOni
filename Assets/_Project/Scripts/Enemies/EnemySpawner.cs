using System.Collections;
using UnityEngine;
using PurrNet;
using ProjectOni.Core;

namespace ProjectOni.Enemies
{
    /// <summary>
    /// Server-side spawn point. Spawns one enemy on level load.
    /// Can optionally respawn the enemy on death (great for combat testing).
    /// Place in the scene instead of raw enemy PrefabInstances.
    /// </summary>
    public class EnemySpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private float _initialDelay = 0f;
        [SerializeField] private bool _respawnOnDeath = true;
        [SerializeField] private float _respawnDelay = 3f;

        private GameObject _spawnedEnemy;
        private HealthComponent _spawnedHealth;

        protected override void OnSpawned()
        {
            if (!isServer) return;
            if (_initialDelay > 0f)
                StartCoroutine(SpawnAfterDelay(_initialDelay));
            else
                SpawnEnemy();
        }

        protected override void OnDespawned(bool asServer)
        {
            if (asServer)
            {
                CleanupSpawnedListeners();
            }
        }

        protected override void OnDestroy()
        {
            CleanupSpawnedListeners();
            base.OnDestroy();
        }

        private IEnumerator SpawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SpawnEnemy();
        }

        private void SpawnEnemy()
        {
            if (_enemyPrefab == null) return;
            
            _spawnedEnemy = Instantiate(_enemyPrefab, transform.position, transform.rotation);
            
            if (_spawnedEnemy != null)
            {
                // Prep initial SyncVars BEFORE spawning on the network to ensure they are packed in the spawn payload
                var entityState = _spawnedEnemy.GetComponentInChildren<EntityState>();
                var health = _spawnedEnemy.GetComponentInChildren<HealthComponent>();
                if (entityState != null && health != null)
                {
                    float initialMax = entityState.MaxHealth.value > 0f ? entityState.MaxHealth.value : health.BaseMaxHealth;
                    entityState.MaxHealth.value = initialMax;
                    entityState.CurrentHealth.value = initialMax;
                }

                // Safety net: explicitly register with PurrNet if IL-weaver didn't auto-spawn it
                if (NetworkManager.main != null)
                {
                    NetworkManager.main.Spawn(_spawnedEnemy);
                }
            }
            
            if (_respawnOnDeath && _spawnedEnemy != null)
            {
                _spawnedHealth = _spawnedEnemy.GetComponentInChildren<HealthComponent>();
                if (_spawnedHealth != null)
                {
                    _spawnedHealth.OnDied += OnEnemyDied;
                }
            }
        }

        private void OnEnemyDied()
        {
            CleanupSpawnedListeners();
            if (_respawnOnDeath && gameObject.activeInHierarchy)
            {
                StartCoroutine(RespawnSequence());
            }
        }

        private IEnumerator RespawnSequence()
        {
            yield return new WaitForSeconds(_respawnDelay);
            SpawnEnemy();
        }

        private void CleanupSpawnedListeners()
        {
            if (_spawnedHealth != null)
            {
                _spawnedHealth.OnDied -= OnEnemyDied;
                _spawnedHealth = null;
            }
            _spawnedEnemy = null;
        }
    }
}
