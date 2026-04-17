using UnityEngine;
using UnityEngine.InputSystem;
using ProjectOni.Data;
using ProjectOni.Combat;

namespace ProjectOni.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Attacking Settings")]
        private Animator _animator;
        private PlayerInventory _inventory;
        private PlayerStats _stats;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _inventory = GetComponent<PlayerInventory>();
            _stats = GetComponent<PlayerStats>();
        }

        // Action from PlayerInput
        public void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                PerformAttack();
            }
        }

        private void PerformAttack()
        {
            if (_inventory.currentWeapon == null) return;

            // Trigger animation
            if (_animator != null)
            {
                _animator.Play(_inventory.currentWeapon.attackAnimation != null ? _inventory.currentWeapon.attackAnimation.name : "Attack");
            }

            // Damage logic (simplified - normally triggered via Animation Event or Hitbox control)
            Debug.Log($"Attacking with {_inventory.currentWeapon.itemName} dealing {StatCalculator.CalculateFinalDamage(_stats.BaseDamage, _inventory.currentWeapon, null)} damage.");
        }
    }
}
