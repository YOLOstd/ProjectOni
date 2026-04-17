using UnityEngine;
using ProjectOni.Core;

namespace ProjectOni.Enemies
{
    public class EnemyStats : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 50f;
        [SerializeField] private float currentHealth;

        private LootDropper _lootDropper;
        private Animator _animator;

        private void Start()
        {
            currentHealth = maxHealth;
            _lootDropper = GetComponent<LootDropper>();
            _animator = GetComponent<Animator>();
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            Debug.Log($"{gameObject.name} took {amount} damage. Current Health: {currentHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // Simple hit feedback trigger if animator exists
                if (_animator != null) _animator.SetTrigger("Hit");
            }
        }

        private void Die()
        {
            Debug.Log($"{gameObject.name} died!");
            
            if (_animator != null)
            {
                _animator.SetTrigger("Die");
            }

            // Trigger loot after death (might want to delay slightly in real use case)
            if (_lootDropper != null)
            {
                _lootDropper.DropLoot();
            }

            // Disable AI and physics
            if (TryGetComponent(out EnemyAI ai)) ai.enabled = false;
            if (TryGetComponent(out Collider2D col)) col.enabled = false;
            
            // In a real game, would pool or destroy
            Destroy(gameObject, 3f);
        }
    }
}
