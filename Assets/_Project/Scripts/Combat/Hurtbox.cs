using UnityEngine;
using ProjectOni.Core;

namespace ProjectOni.Combat
{
    /// <summary>
    /// Attached to entities to receive physical contacts.
    /// Usually, this component passes damage to the owner's Stats component.
    /// </summary>
    public class Hurtbox : MonoBehaviour
    {
        private IDamageable _damageable;

        private void Awake()
        {
            // Usually, the IDamageable is on the same object or parent
            _damageable = GetComponentInParent<IDamageable>();
        }

        public void TakeDamage(float amount)
        {
            _damageable?.TakeDamage(amount);
        }
    }
}
