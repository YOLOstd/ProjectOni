using UnityEngine;
using UnityEngine.InputSystem;
using ProjectOni.Combat;

namespace ProjectOni.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        private CombatController _combatController;
        private PlayerController _playerController;

        private void Awake()
        {
            _combatController = GetComponent<CombatController>();
            _playerController = GetComponent<PlayerController>();
        }

        public void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                _combatController.TriggerAction(ActionSlot.Primary, GetAttackDirection());
            }
        }

        public void OnSecondaryAttack(InputValue value)
        {
            if (value.isPressed)
            {
                _combatController.TriggerAction(ActionSlot.Secondary, GetAttackDirection());
            }
        }

        public void OnSpellQ(InputValue value)
        {
            if (value.isPressed)
            {
                _combatController.TriggerAction(ActionSlot.Spell1, GetAttackDirection());
            }
        }

        public void OnSpellE(InputValue value)
        {
            if (value.isPressed)
            {
                _combatController.TriggerAction(ActionSlot.Spell2, GetAttackDirection());
            }
        }

        private Vector2 GetAttackDirection()
        {
            // Simple horizontal direction based on player facing
            return new Vector2(_playerController.FacingDir, 0);
        }
    }
}
