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
        private CombatAnimator _combatAnimator;

        private void Awake()
        {
            _combatAnimator = GetComponent<CombatAnimator>();
            if (_combatAnimator == null) _combatAnimator = GetComponentInChildren<CombatAnimator>();
            if (_combatAnimator == null) _combatAnimator = GetComponentInParent<CombatAnimator>();

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
                Debug.Log($"[PlayerCombat] OnSpawned. isOwner=true. _combatAnimator found: {_combatAnimator != null}");
                if (_combatAnimator != null)
                {
                    _combatAnimator.OnHitDetected += HandlePlayerHitTarget;
                }
            }
        }

        protected override void OnDespawned(bool asServer)
        {
            base.OnDespawned(asServer);
            if (isOwner)
            {
                UnsubscribeEvents();
                if (_combatAnimator != null)
                {
                    _combatAnimator.OnHitDetected -= HandlePlayerHitTarget;
                }
            }
        }


        private void SubscribeEvents()
        {
            var input = InputManager.Instance;
            if (input != null)
            {
                input.AttackPressed += OnAttackPressed;
                input.AttackReleased += OnAttackReleased;
                
                input.SecondaryAttackPressed += OnSecondaryAttackPressed;
                input.SecondaryAttackReleased += OnSecondaryAttackReleased;
                
                input.SpellQPressed += OnSpellQPressed;
                input.SpellQReleased += OnSpellQReleased;
                
                input.SpellEPressed += OnSpellEPressed;
                input.SpellEReleased += OnSpellEReleased;
            }
        }

        private void UnsubscribeEvents()
        {
            var input = InputManager.Instance;
            if (input != null)
            {
                input.AttackPressed -= OnAttackPressed;
                input.AttackReleased -= OnAttackReleased;
                
                input.SecondaryAttackPressed -= OnSecondaryAttackPressed;
                input.SecondaryAttackReleased -= OnSecondaryAttackReleased;
                
                input.SpellQPressed -= OnSpellQPressed;
                input.SpellQReleased -= OnSpellQReleased;
                
                input.SpellEPressed -= OnSpellEPressed;
                input.SpellEReleased -= OnSpellEReleased;
            }
        }

        private void OnAttackPressed() => OnInputDown(ActionSlot.Primary);
        private void OnAttackReleased() => OnInputUp(ActionSlot.Primary);

        private void OnSecondaryAttackPressed() => OnInputDown(ActionSlot.Secondary);
        private void OnSecondaryAttackReleased() => OnInputUp(ActionSlot.Secondary);

        private void OnSpellQPressed() => OnInputDown(ActionSlot.Spell1);
        private void OnSpellQReleased() => OnInputUp(ActionSlot.Spell1);

        private void OnSpellEPressed() => OnInputDown(ActionSlot.Spell2);
        private void OnSpellEReleased() => OnInputUp(ActionSlot.Spell2);

        private void OnInputDown(ActionSlot slot)
        {
            if (_combatController != null)
            {
                _combatController.OnInputDown(slot, GetAttackDirection());
            }
        }

        private void OnInputUp(ActionSlot slot)
        {
            if (_combatController != null)
            {
                _combatController.OnInputUp(slot);
            }
        }

        private Vector2 GetAttackDirection()
        {
            if (_playerController == null) return Vector2.right;
            // Simple horizontal direction based on player facing
            return new Vector2(_playerController.FacingDir, 0);
        }

        private void HandlePlayerHitTarget(Hurtbox enemyHurtbox, float damage)
        {
            Debug.Log($"[PlayerCombat] HandlePlayerHitTarget triggered: isOwner={isOwner}, enemyHurtbox={enemyHurtbox?.gameObject.name}, damage={damage}");
            if (!isOwner) return; // Only process hit on attacking player's owning client

            // Verify target is an enemy by getting its EnemyCombat component
            var enemyCombat = enemyHurtbox.GetComponentInParent<ProjectOni.Enemies.EnemyCombat>();
            Debug.Log($"[PlayerCombat] Resolved EnemyCombat: {enemyCombat != null}");
            if (enemyCombat != null)
            {
                // Send ServerRpc to notify Host to apply damage (Favor the Shooter)
                Debug.Log($"[PlayerCombat] Sending ServerApplyDamage RPC to Host for {damage} damage.");
                enemyCombat.ServerApplyDamage(damage);
            }
        }
    }
}
