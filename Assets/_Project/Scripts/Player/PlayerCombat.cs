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
        private EquipmentManager _equipment;
        private PlayerStats _stats;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _equipment = GetComponent<EquipmentManager>();
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
            ModularEquipmentData activeWeapon = _equipment.GetActiveWeapon();
            if (activeWeapon == null) return;
    
            // Get the weapon trait to find the animation trigger
            var weaponTrait = activeWeapon.GetTrait<WeaponTrait>();
            string animName = (weaponTrait != null && !string.IsNullOrEmpty(weaponTrait.comboAnimationTrigger)) 
                ? weaponTrait.comboAnimationTrigger 
                : "Attack";

            // Trigger animation
            if (_animator != null)
            {
                _animator.Play(animName);
            }
    
            // Damage logic (simplified - normally triggered via Animation Event or Hitbox control)
            float damage = StatCalculator.CalculateFinalDamage(_stats.BaseDamage, new[] { activeWeapon });
            Debug.Log($"Attacking with {activeWeapon.itemName} dealing {damage} damage.");
        }
    }
}
