using UnityEngine;
using ProjectOni.Core;
using PurrNet;

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
                // Networked interaction: Take ownership of the target if it's a networked object
                if (collision.TryGetComponent(out NetworkIdentity targetIdentity))
                {
                    var hitterIdentity = GetComponentInParent<NetworkIdentity>();
                    if (hitterIdentity != null && hitterIdentity.isOwner)
                    {
                        ProjectOni.Networking.NetworkedInteraction.RequestOwnershipOnHit(targetIdentity, hitterIdentity);
                    }
                }

                damageable.TakeDamage(_damage);
            }
        }
    }
}
