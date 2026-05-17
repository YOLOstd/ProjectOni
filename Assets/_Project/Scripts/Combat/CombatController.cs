using System;
using System.Collections.Generic;
using UnityEngine;
using PurrNet;
using ProjectOni.Combat.Data;
using ProjectOni.Player;
using ProjectOni.Data;
using ProjectOni.Combat;
using ProjectOni.Core;

namespace ProjectOni.Combat
{
    [System.Serializable]
    public struct ActionSlotBinding
    {
        public ActionSlot slot;
        public EquipmentSlotDefinition equipmentSlot;
    }

    public class CombatController : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private List<ActionSlotBinding> _bindings;

        [Header("References")]
        [SerializeField] private CombatAnimator _combatAnimator;
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private StatController _statController;

        private Dictionary<ActionSlot, float> _cooldowns = new();

        protected override void OnSpawned()
        {
            base.OnSpawned();
            if (_equipmentManager == null) _equipmentManager = GetComponent<EquipmentManager>();
            if (_combatAnimator == null) _combatAnimator = GetComponentInChildren<CombatAnimator>();
            if (_statController == null) _statController = GetComponent<StatController>();
        }

        public void TriggerAction(ActionSlot slot, Vector2 direction)
        {
            if (!isOwner) return;

            var binding = _bindings.Find(b => b.slot == slot);
            if (binding.equipmentSlot == null) return;

            var item = _equipmentManager.GetItemInSlot(binding.equipmentSlot);
            if (!item.IsValid) return;

            // Try to find any attack behavior (Weapon or Spell)
            IAttackBehavior behavior = null;
            float cooldown = 0.5f;
            int skillLevel = 1;

            var weapon = item.GetTrait<WeaponTrait>();
            if (weapon != null)
            {
                behavior = weapon.attackData;
                skillLevel = weapon.skillLevel;
                if (weapon.attackData != null)
                {
                    float baseCooldown = weapon.attackData.attackCooldown;
                    float attackSpeedMultiplier = 1f;
                    if (_statController != null)
                    {
                        attackSpeedMultiplier = _statController.Get(StatType.AttackSpeed);
                        if (attackSpeedMultiplier <= 0f) attackSpeedMultiplier = 1f;
                    }
                    cooldown = baseCooldown / attackSpeedMultiplier;
                }
            }
            else
            {
                var spell = item.GetTrait<SpellTrait>();
                if (spell != null)
                {
                    behavior = spell.spellData;
                    skillLevel = spell.skillLevel;
                    if (spell.spellData != null)
                    {
                        cooldown = spell.spellData.attackCooldown;
                    }
                }
            }

            if (behavior == null) return;

            ExecuteAction(slot, behavior, cooldown, direction, skillLevel);
        }

        private void ExecuteAction(ActionSlot slot, IAttackBehavior behavior, float cooldown, Vector2 direction, int skillLevel)
        {
            if (_cooldowns.TryGetValue(slot, out float lastTime) && Time.time < lastTime + cooldown)
                return;

            _cooldowns[slot] = Time.time;

            // Execute Logic (Optimistic)
            VisualRequest request = behavior.Execute(new AttackContext
            {
                Caster = gameObject,
                TargetLayer = _targetLayer,
                Direction = direction,
                Position = transform.position,
                SkillLevel = skillLevel
            });
            Debug.Log($"[CombatController] Action executed: {slot} on {gameObject.name}");

            // Trigger Visuals for everyone
            RpcPlayVisuals(request, direction);
        }

        [ObserversRpc(runLocally: true, requireServer: false)]
        private void RpcPlayVisuals(VisualRequest request, Vector2 direction)
        {
            if (_combatAnimator != null)
            {
                _combatAnimator.PlayVisual(request, direction);
            }
        }
    }
}
