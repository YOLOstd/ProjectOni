using UnityEngine;
using PurrNet;
using ProjectOni.Core;

namespace ProjectOni.Combat
{
    /// <summary>
    /// A networked tag component attached to collision triggers (sensors).
    /// Acts as a sensor holding a public reference to its associated HealthComponent.
    /// </summary>
    public class Hurtbox : NetworkBehaviour
    {
        public HealthComponent Health;

        private void Awake()
        {
            // Resolve local HealthComponent in parent if not assigned in Inspector
            if (Health == null)
            {
                Health = GetComponentInParent<HealthComponent>();
            }

            // Warn if missing trigger collider
            var col = GetComponent<Collider2D>();
            if (col == null)
            {
                Debug.LogWarning($"[Hurtbox] '{gameObject.name}' on parent '{transform.parent?.name}' is missing a Collider2D component! " +
                                 $"It will not be able to detect trigger overlaps. Please add a Collider2D set as 'Is Trigger'.");
            }
            else if (!col.isTrigger)
            {
                Debug.LogWarning($"[Hurtbox] '{gameObject.name}' on parent '{transform.parent?.name}' has a Collider2D, but it is not set as 'Is Trigger'!");
            }
        }
    }
}
