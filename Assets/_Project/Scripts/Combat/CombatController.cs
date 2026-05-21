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

    public struct BufferedInput
    {
        public ActionSlot Slot;
        public float Timestamp;
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
        
        private float _globalLockEndTime;
        private float _antiGravityEndTime;
        private BufferedInput? _bufferedInput;
        private const float BufferWindow = 0.3f;
        private Coroutine _activeHitboxRoutine;
        
        private PlayerController _playerController;
        private DodgeController _dodgeController;

        public bool IsGlobalLocked => Time.time < _globalLockEndTime;
        public bool IsAntiGravityActive => Time.time < _antiGravityEndTime;

        public void CancelGlobalLock()
        {
            _globalLockEndTime = 0f;
            _antiGravityEndTime = 0f;
            _bufferedInput = null;
            
            if (_activeHitboxRoutine != null)
            {
                StopCoroutine(_activeHitboxRoutine);
                _activeHitboxRoutine = null;
            }

            RpcCancelVisuals();
        }

        [ObserversRpc(runLocally: true, requireServer: false)]
        private void RpcCancelVisuals()
        {
            if (_combatAnimator != null)
                _combatAnimator.CancelVisuals();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();
            if (_equipmentManager == null) _equipmentManager = GetComponent<EquipmentManager>();
            if (_combatAnimator == null) _combatAnimator = GetComponentInChildren<CombatAnimator>();
            if (_statController == null) _statController = GetComponent<StatController>();
            _playerController = GetComponent<PlayerController>();
            if (_playerController == null) _playerController = GetComponentInParent<PlayerController>();
            _dodgeController = GetComponent<DodgeController>();
            if (_dodgeController == null) _dodgeController = GetComponentInParent<DodgeController>();
            if (_dodgeController == null) _dodgeController = GetComponentInChildren<DodgeController>();
        }

        private void Update()
        {
            if (!isOwner) return;

            if (_bufferedInput.HasValue)
            {
                bool isBlockedByDodge = _dodgeController != null && _dodgeController.IsDodging;

                if (isBlockedByDodge)
                {
                    // Artificially keep the buffered input fresh for the entire duration of the dodge.
                    // Once the dodge ends, the buffer considers the input to be 0 milliseconds old.
                    var buffered = _bufferedInput.Value;
                    buffered.Timestamp = Time.time;
                    _bufferedInput = buffered;
                }

                // Only release the buffer if both global lock has ended AND the player is no longer dodging
                if (Time.time >= _globalLockEndTime && !isBlockedByDodge)
                {
                    var buffered = _bufferedInput.Value;
                    if (Time.time - buffered.Timestamp <= BufferWindow)
                    {
                        Vector2 latestDirection = _playerController != null 
                            ? new Vector2(_playerController.FacingDir, 0) 
                            : Vector2.right;

                        _bufferedInput = null;
                        TriggerAction(buffered.Slot, latestDirection);
                    }
                    else
                    {
                        // Expired
                        _bufferedInput = null;
                    }
                }
            }
        }

        public void TriggerAction(ActionSlot slot, Vector2 direction)
        {
            if (!isOwner) return;

            // If we are actively dodging/invincible, queue this action to be executed once the dodge ends
            if (_dodgeController != null && _dodgeController.IsInvincible)
            {
                _bufferedInput = new BufferedInput { Slot = slot, Timestamp = Time.time };
                return;
            }

            if (Time.time < _globalLockEndTime)
            {
                if (Time.time >= _globalLockEndTime - BufferWindow)
                {
                    _bufferedInput = new BufferedInput { Slot = slot, Timestamp = Time.time };
                }
                return; // Blocked by global lock
            }

            var binding = _bindings.Find(b => b.slot == slot);
            if (binding.equipmentSlot == null) return;

            var item = _equipmentManager.GetItemInSlot(binding.equipmentSlot);
            if (!item.IsValid) return;

            IAttackBehavior behavior = null;
            float cooldown = 0.5f;
            int skillLevel = 1;

            float attackSpeedMultiplier = 1f;
            float castSpeedMultiplier = 1f;

            if (_statController != null)
            {
                attackSpeedMultiplier = _statController.Get(StatType.AttackSpeed);
                if (attackSpeedMultiplier <= 0f) attackSpeedMultiplier = 0.1f;
                
                castSpeedMultiplier = _statController.Get(StatType.CastSpeed);
                if (castSpeedMultiplier <= 0f) castSpeedMultiplier = 0.1f;
            }

            var weapon = item.GetTrait<WeaponTrait>();
            if (weapon != null)
            {
                behavior = weapon.attackData;
                skillLevel = weapon.skillLevel;
                if (weapon.attackData != null)
                {
                    cooldown = weapon.attackData.attackCooldown / attackSpeedMultiplier;
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
                        cooldown = spell.spellData.attackCooldown / castSpeedMultiplier;
                    }
                }
            }

            if (behavior == null) return;

            ExecuteAction(slot, behavior, cooldown, direction, skillLevel, attackSpeedMultiplier, castSpeedMultiplier);
        }

        private void ExecuteAction(ActionSlot slot, IAttackBehavior behavior, float cooldown, Vector2 direction, int skillLevel, float attackSpdMult, float castSpdMult)
        {
            if (_cooldowns.TryGetValue(slot, out float lastTime) && Time.time < lastTime + cooldown)
                return;

            _cooldowns[slot] = Time.time;

            // Execute Logic
            AttackResult result = behavior.Execute(new AttackContext
            {
                Caster = gameObject,
                TargetLayer = _targetLayer,
                Direction = direction,
                Position = transform.position,
                SkillLevel = skillLevel,
                AttackSpeedMultiplier = attackSpdMult,
                CastSpeedMultiplier = castSpdMult
            });

            if (!result.Success) return;

            _globalLockEndTime = Time.time + result.GlobalLockTime;
            _antiGravityEndTime = Time.time + result.AntiGravityTime;

            if (_playerController != null)
            {
                _playerController.OnAirborneAttackInitiated();
                if (result.LungeForce > 0)
                {
                    _playerController.ApplyAttackLunge(result.LungeForce);
                }
            }

            Debug.Log($"[CombatController] Action executed: {slot} on {gameObject.name}. LockTime: {result.GlobalLockTime}, AntiGravityTime: {result.AntiGravityTime}");

            // Build a network-safe hint from the result. Asset refs (prefab, audio) are excluded —
            // each client resolves them locally from their own copy of the ScriptableObject.
            var hint = new NetworkedVisualHint
            {
                animationTrigger = result.Visuals.animationTrigger,
                damage           = result.Visuals.damage,
                spawnOffset      = result.Visuals.spawnOffset,
                projectileSpeed  = result.Visuals.projectileSpeed,
                lifetime         = result.Visuals.lifetime,
                hitboxStartTime  = result.Visuals.hitboxStartTime,
                hitboxDuration   = result.Visuals.hitboxDuration
            };

            // Single-step RPC: runLocally gives zero-latency on the caller,
            // requireServer: false lets PurrNet relay to all other clients via the server.
            RpcPlayVisuals(slot, direction, hint);
        }

        [ObserversRpc(runLocally: true, requireServer: false)]
        private void RpcPlayVisuals(ActionSlot slot, Vector2 direction, NetworkedVisualHint hint)
        {
            if (_combatAnimator == null) return;
            var (prefab, sfx, hitVfx) = GetLocalVisualRefs(slot);
            var request = new VisualRequest
            {
                animationTrigger = hint.animationTrigger,
                sfx              = sfx,
                projectilePrefab = prefab,
                projectileSpeed  = hint.projectileSpeed,
                damage           = hint.damage,
                spawnOffset      = hint.spawnOffset,
                hitVFXPrefab     = hitVfx,
                lifetime         = hint.lifetime,
                hitboxStartTime  = hint.hitboxStartTime,
                hitboxDuration   = hint.hitboxDuration
            };
            _combatAnimator.PlayVisual(request, direction);
        }

        /// <summary>
        /// Resolves local Unity asset references for the given slot from the currently equipped item.
        /// Called on every client independently — ScriptableObject data is identical on all machines.
        /// </summary>
        private (GameObject prefab, AudioClip sfx, GameObject hitVfx) GetLocalVisualRefs(ActionSlot slot)
        {
            var binding = _bindings.Find(b => b.slot == slot);
            if (binding.equipmentSlot == null) return (null, null, null);

            var item = _equipmentManager.GetItemInSlot(binding.equipmentSlot);
            if (!item.IsValid) return (null, null, null);

            var weapon = item.GetTrait<WeaponTrait>();
            if (weapon?.attackData != null) return weapon.attackData.GetVisualRefs();

            var spell = item.GetTrait<SpellTrait>();
            if (spell?.spellData != null) return spell.spellData.GetVisualRefs();

            return (null, null, null);
        }
    }
}
