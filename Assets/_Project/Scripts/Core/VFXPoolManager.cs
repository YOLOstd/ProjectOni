using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace ProjectOni.Core
{
    public class VFXPoolManager : MonoBehaviour
    {
        public static VFXPoolManager Instance { get; private set; }

        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _pools = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            ClearAllPools();
        }

        public void ClearAllPools()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            _pools.Clear();
        }

        private ObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
        {
            if (prefab == null) return null;

            if (_pools.TryGetValue(prefab, out var pool))
            {
                return pool;
            }

            ObjectPool<GameObject> newPool = null;
            newPool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    var instance = Instantiate(prefab);
                    if (!instance.TryGetComponent<PooledVFX>(out var pooledVFX))
                    {
                        pooledVFX = instance.AddComponent<PooledVFX>();
                    }
                    pooledVFX.Initialize(newPool);
                    return instance;
                },
                actionOnGet: (instance) =>
                {
                    if (instance != null)
                    {
                        instance.SetActive(true);
                        if (instance.TryGetComponent<PooledVFX>(out var pooledVFX))
                        {
                            pooledVFX.ResetObject();
                        }
                    }
                },
                actionOnRelease: (instance) =>
                {
                    if (instance != null)
                    {
                        instance.SetActive(false);
                    }
                },
                actionOnDestroy: (instance) =>
                {
                    if (instance != null)
                    {
                        Destroy(instance);
                    }
                },
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 50
            );

            _pools.Add(prefab, newPool);
            return newPool;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            if (prefab == null) return null;

            var pool = GetOrCreatePool(prefab);
            if (pool == null) return null;

            var instance = pool.Get();
            if (instance == null)
            {
                Debug.LogWarning($"[VFXPoolManager] Retrieved a destroyed pooled object for prefab {prefab.name}. Clearing pool and retrying.");
                pool.Clear();
                instance = pool.Get();
            }

            if (instance != null)
            {
                var t = instance.transform;
                t.SetParent(parent);
                t.position = position;
                t.rotation = rotation;
            }

            return instance;
        }
    }
}
