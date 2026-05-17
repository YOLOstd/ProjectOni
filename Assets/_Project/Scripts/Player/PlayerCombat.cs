using UnityEngine;
using PurrNet;
using ProjectOni.Combat;
using ProjectOni.Managers;

namespace ProjectOni.Player
{
    public class PlayerCombat : NetworkBehaviour
    {
        private CombatController _combatController;
        private PlayerController _playerController;

        private void Awake()
        {
            _combatController = GetComponent<CombatController>();
            if (_combatController == null) _combatController = GetComponentInChildren<CombatController>();
            if (_combatController == null) _combatController = GetComponentInParent<CombatController>();

            _playerController = GetComponent<PlayerController>();
            if (_playerController == null) _playerController = GetComponentInChildren<PlayerController>();
            if (_playerController == null) _playerController = GetComponentInParent<PlayerController>();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();
            if (isOwner)
            {
                SubscribeEvents();
            }
        }

        protected override void OnDespawned(bool asServer)
        {
            base.OnDespawned(asServer);
            if (isOwner)
            {
                UnsubscribeEvents();
            }
        }

        private void OnEnable()
        {
            if (isSpawned && isOwner)
            {
                SubscribeEvents();
            }
        }

        private void OnDisable()
        {
            if (isSpawned && isOwner)
            {
                UnsubscribeEvents();
            }
        }

        private void SubscribeEvents()
        {
            var input = InputManager.Instance;
            if (input != null)
            {
                input.AttackPressed += OnAttack;
                input.SecondaryAttackPressed += OnSecondaryAttack;
                input.SpellQPressed += OnSpellQ;
                input.SpellEPressed += OnSpellE;
            }
        }

        private void UnsubscribeEvents()
        {
            var input = InputManager.Instance;
            if (input != null)
            {
                input.AttackPressed -= OnAttack;
                input.SecondaryAttackPressed -= OnSecondaryAttack;
                input.SpellQPressed -= OnSpellQ;
                input.SpellEPressed -= OnSpellE;
            }
        }

        private void OnAttack()
        {
            Debug.Log("OnAttack");
            if (_combatController != null)
            {
                _combatController.TriggerAction(ActionSlot.Primary, GetAttackDirection());
            }
        }

        private void OnSecondaryAttack()
        {
            if (_combatController != null)
            {
                _combatController.TriggerAction(ActionSlot.Secondary, GetAttackDirection());
            }
        }

        private void OnSpellQ()
        {
            if (_combatController != null)
            {
                _combatController.TriggerAction(ActionSlot.Spell1, GetAttackDirection());
            }
        }

        private void OnSpellE()
        {
            if (_combatController != null)
            {
                _combatController.TriggerAction(ActionSlot.Spell2, GetAttackDirection());
            }
        }

        private Vector2 GetAttackDirection()
        {
            if (_playerController == null) return Vector2.right;
            // Simple horizontal direction based on player facing
            return new Vector2(_playerController.FacingDir, 0);
        }
    }
}
