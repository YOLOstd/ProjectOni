using UnityEngine;
using ProjectOni.Core;

namespace ProjectOni.Player
{
    public class PlayerStats : MonoBehaviour, IDamageable
    {
        [Header("Base Stats")]
        [SerializeField] private float baseMaxHealth = 100f;
        [SerializeField] private float baseDamage = 10f;

        [Header("Current State")]
        [SerializeField] private float currentHealth;
        private float _maxHealth;

        public float BaseDamage => baseDamage;
        public float BaseMaxHealth => baseMaxHealth;
        public float CurrentHealth => currentHealth;

        private void Start()
        {
            InitializeStats();
        }

        public void InitializeStats()
        {
            // Initial health without equipment
            _maxHealth = baseMaxHealth;
            currentHealth = _maxHealth;
            GameEvents.TriggerPlayerHealthChanged(currentHealth, _maxHealth);
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            currentHealth = Mathf.Max(0, currentHealth);
            
            GameEvents.TriggerPlayerHealthChanged(currentHealth, _maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void UpdateMaxHealth(float newMax)
        {
            float healthPercent = currentHealth / _maxHealth;
            _maxHealth = newMax;
            currentHealth = _maxHealth * healthPercent;
            GameEvents.TriggerPlayerHealthChanged(currentHealth, _maxHealth);
        }

        private void Die()
        {
            Debug.Log("Player died!");
            // Handle death state
        }
    }
}
