using UnityEngine;
using ProjectOni.Core;

namespace ProjectOni.Combat
{
    /// <summary>
    /// Attached to a weapon or attack zone to deal damage.
    /// Expects a BoxCollider2D set as a Trigger.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class Hitbox : MonoBehaviour
    {
        private float _damage;

        public void Initialize(float damage)
        {
            _damage = damage;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_damage);
            }
        }
    }
}
