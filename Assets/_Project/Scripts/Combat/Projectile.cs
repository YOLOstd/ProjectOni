using UnityEngine;
using ProjectOni.Core;

namespace ProjectOni.Combat
{
    public class Projectile : MonoBehaviour
    {
        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private bool _isOwner;
        private LayerMask _hitMask;
        private GameObject _hitVFX;

        private Rigidbody2D _rb;

        public void Initialize(Vector2 direction, float speed, float damage, bool isOwner, LayerMask hitMask, GameObject hitVFX = null)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _isOwner = isOwner;
            _hitMask = hitMask;
            _hitVFX = hitVFX;

            _rb = GetComponent<Rigidbody2D>();
            if (_rb != null)
            {
                _rb.linearVelocity = _direction * _speed;
            }

            // Rotate towards direction
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // Auto-despawn after 5 seconds
            Destroy(gameObject, 5f);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Check if layer is in hit mask
            if (((1 << collision.gameObject.layer) & _hitMask) != 0)
            {
                if (_isOwner && collision.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(_damage);
                }

                if (_hitVFX != null)
                {
                    Instantiate(_hitVFX, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }
    }
}
