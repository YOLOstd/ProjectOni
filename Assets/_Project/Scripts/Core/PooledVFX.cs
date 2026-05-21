using UnityEngine;
using UnityEngine.Pool;

namespace ProjectOni.Core
{
    public class PooledVFX : MonoBehaviour
    {
        private IObjectPool<GameObject> _pool;
        private TrailRenderer[] _trails;
        private ParticleSystem[] _particleSystems;
        private IPooledObject[] _pooledObjects;
        private float _releaseTime;

        private void Awake()
        {
            _trails = GetComponentsInChildren<TrailRenderer>(true);
            _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            _pooledObjects = GetComponentsInChildren<IPooledObject>(true);
        }

        public void Initialize(IObjectPool<GameObject> pool)
        {
            _pool = pool;
        }

        public void ResetObject()
        {
            // Reset visuals
            if (_trails != null)
            {
                for (int i = 0; i < _trails.Length; i++)
                {
                    _trails[i].Clear();
                }
            }

            if (_particleSystems != null)
            {
                for (int i = 0; i < _particleSystems.Length; i++)
                {
                    _particleSystems[i].Clear();
                    _particleSystems[i].Play();
                }
            }

            // Reset logical states
            if (_pooledObjects != null)
            {
                for (int i = 0; i < _pooledObjects.Length; i++)
                {
                    _pooledObjects[i].ResetState();
                }
            }

            // Reset timer
            _releaseTime = 0f;
        }

        public void ReleaseAfter(float delay)
        {
            _releaseTime = Time.time + delay;
        }

        private void Update()
        {
            if (_releaseTime > 0f && Time.time >= _releaseTime)
            {
                _releaseTime = 0f;
                Release();
            }
        }

        public void Release()
        {
            if (_pool != null)
            {
                _pool.Release(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
