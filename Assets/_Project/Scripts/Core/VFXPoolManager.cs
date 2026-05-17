using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

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
                    instance.SetActive(true);
                    if (instance.TryGetComponent<PooledVFX>(out var pooledVFX))
                    {
                        pooledVFX.ResetObject();
                    }
                },
                actionOnRelease: (instance) =>
                {
                    instance.SetActive(false);
                },
                actionOnDestroy: (instance) =>
                {
                    Destroy(instance);
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
            var t = instance.transform;
            t.SetParent(parent);
            t.position = position;
            t.rotation = rotation;

            return instance;
        }
    }
}
