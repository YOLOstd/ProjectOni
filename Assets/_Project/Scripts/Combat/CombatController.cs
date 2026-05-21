using System;
using System.Collections.Generic;
using UnityEngine;
using PurrNet;
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

        [Header("Combo State")]
        [SerializeField] private AttackNode _currentAttackNode;
        [SerializeField] private float _comboResetDelay = 0.5f;
        private float _currentAttackTimer;
        private float _currentGlobalLockDuration;
        private bool _isHoldingInput;
        private ActionSlot _activeSlot;

        private struct BufferedComboInput
        {
            public ActionSlot slot;
            public Vector2 direction;
            public bool isValid;
        }
        private BufferedComboInput _comboBuffer;

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

            ResetCombo();
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

        private void ResetCombo()
        {
            _currentAttackNode = null;
            _currentAttackTimer = 0f;
            _isHoldingInput = false;
            _comboBuffer.isValid = false;
        }

        private void Update()
        {
            if (!isOwner) return;

            // 1. Handle Dodge & Lock buffering for neutral combo start
            if (_bufferedInput.HasValue)
            {
                bool isBlockedByDodge = _dodgeController != null && _dodgeController.IsDodging;

                if (isBlockedByDodge)
                {
                    var buffered = _bufferedInput.Value;
                    buffered.Timestamp = Time.time;
                    _bufferedInput = buffered;
                }

                if (Time.time >= _globalLockEndTime && !isBlockedByDodge)
                {
                    var buffered = _bufferedInput.Value;
                    if (Time.time - buffered.Timestamp <= BufferWindow)
                    {
                        Vector2 latestDirection = GetFacingDirection();
                        _bufferedInput = null;
                        OnInputDown(buffered.Slot, latestDirection);
                    }
                    else
                    {
                        _bufferedInput = null;
                    }
                }
            }

            // 2. Combo State Machine Update Loop
            if (_currentAttackNode != null)
            {
                _currentAttackTimer += Time.deltaTime;

                // 2a. Transition when global lock duration is reached
                if (_currentAttackTimer >= _currentGlobalLockDuration)
                {
                    if (_isHoldingInput || _comboBuffer.isValid)
                    {
                        if (_currentAttackNode.normalNextNode != null)
                        {
                            Vector2 nextDirection = _comboBuffer.isValid ? _comboBuffer.direction : GetFacingDirection();
                            AttackNode nextNode = _currentAttackNode.normalNextNode;
                            
                            _comboBuffer.isValid = false;
                            TransitionToAttack(nextNode, nextDirection);
                        }
                        else
                        {
                            // End of combo, loop back to starting node if player holds or buffers input
                            Vector2 nextDirection = _comboBuffer.isValid ? _comboBuffer.direction : GetFacingDirection();
                            ActionSlot targetSlot = _comboBuffer.isValid ? _comboBuffer.slot : _activeSlot;
                            
                            ResetCombo();
                            
                            AttackNode startingNode = GetStartingNodeForSlot(targetSlot);
                            if (startingNode != null)
                            {
                                _activeSlot = targetSlot;
                                _isHoldingInput = true;
                                TransitionToAttack(startingNode, nextDirection);
                            }
                        }
                    }
                }

                // 2b. Automatically reset back to idle if we exceed the global reset timeout
                if (_currentAttackNode != null && _currentAttackTimer >= _currentGlobalLockDuration + _comboResetDelay)
                {
                    ResetCombo();
                }
            }
        }

        public void OnInputDown(ActionSlot slot, Vector2 direction)
        {
            if (!isOwner) return;

            if (_dodgeController != null && _dodgeController.IsInvincible)
            {
                return;
            }

            bool isGlobalLocked = Time.time < _globalLockEndTime;

            if (_currentAttackNode == null)
            {
                if (isGlobalLocked)
                {
                    if (Time.time >= _globalLockEndTime - BufferWindow)
                    {
                        _bufferedInput = new BufferedInput { Slot = slot, Timestamp = Time.time };
                    }
                    return;
                }

                AttackNode startingNode = GetStartingNodeForSlot(slot);
                if (startingNode != null)
                {
                    _activeSlot = slot;
                    _isHoldingInput = true;
                    TransitionToAttack(startingNode, direction);
                }
            }
            else
            {
                if (isGlobalLocked)
                {
                    // Global locked: only buffer same-slot inputs or attempt rhythm checks
                    if (slot == _activeSlot)
                    {
                        _isHoldingInput = true;
                        bool activatedPerfect = EvaluateRhythmAction(direction, isClick: true);
                        if (!activatedPerfect)
                        {
                            _comboBuffer = new BufferedComboInput
                            {
                                slot = slot,
                                direction = direction,
                                isValid = true
                            };
                        }
                    }
                }
                else
                {
                    // Global lock has expired! We can transition or switch slots immediately
                    if (slot == _activeSlot)
                    {
                        _isHoldingInput = true;
                        bool activatedPerfect = EvaluateRhythmAction(direction, isClick: true);
                        if (!activatedPerfect)
                        {
                            if (_currentAttackNode.normalNextNode != null)
                            {
                                TransitionToAttack(_currentAttackNode.normalNextNode, direction);
                            }
                            else
                            {
                                AttackNode startingNode = GetStartingNodeForSlot(slot);
                                if (startingNode != null)
                                {
                                    ResetCombo();
                                    _activeSlot = slot;
                                    _isHoldingInput = true;
                                    TransitionToAttack(startingNode, direction);
                                }
                            }
                        }
                    }
                    else
                    {
                        // We are no longer global locked, so we can transition to a different slot immediately
                        AttackNode startingNode = GetStartingNodeForSlot(slot);
                        if (startingNode != null)
                        {
                            ResetCombo();
                            _activeSlot = slot;
                            _isHoldingInput = true;
                            TransitionToAttack(startingNode, direction);
                        }
                    }
                }
            }
        }

        public void OnInputUp(ActionSlot slot)
        {
            if (!isOwner) return;

            if (slot == _activeSlot)
            {
                _isHoldingInput = false;

                if (_currentAttackNode != null)
                {
                    Vector2 direction = GetFacingDirection();
                    EvaluateRhythmAction(direction, isClick: false);
                }
            }
        }

        private bool EvaluateRhythmAction(Vector2 direction, bool isClick)
        {
            if (_currentAttackNode == null) return false;

            if (_currentAttackTimer >= _currentAttackNode.perfectWindowStart && 
                _currentAttackTimer <= _currentAttackNode.perfectWindowEnd)
            {
                if (_currentAttackNode.perfectNextNode != null)
                {
                    TransitionToAttack(_currentAttackNode.perfectNextNode, direction);
                    return true;
                }
            }

            return false;
        }

        private void TransitionToAttack(AttackNode nextNode, Vector2 direction)
        {
            if (nextNode == null) return;

            float attackSpeedMultiplier = 1f;
            float castSpeedMultiplier = 1f;

            if (_statController != null)
            {
                attackSpeedMultiplier = _statController.Get(StatType.AttackSpeed);
                if (attackSpeedMultiplier <= 0f) attackSpeedMultiplier = 0.1f;

                castSpeedMultiplier = _statController.Get(StatType.CastSpeed);
                if (castSpeedMultiplier <= 0f) castSpeedMultiplier = 0.1f;
            }

            float statMultiplier = (_activeSlot == ActionSlot.Primary || _activeSlot == ActionSlot.Secondary) 
                ? attackSpeedMultiplier 
                : castSpeedMultiplier;

            int skillLevel = GetSkillLevelForSlot(_activeSlot);

            AttackResult result = nextNode.Execute(new AttackContext
            {
                Caster = gameObject,
                TargetLayer = _targetLayer,
                Direction = direction,
                Position = transform.position,
                SkillLevel = skillLevel,
                AttackSpeedMultiplier = attackSpeedMultiplier,
                CastSpeedMultiplier = castSpeedMultiplier
            });

            if (!result.Success) return;

            _currentAttackNode = nextNode;
            _currentAttackTimer = 0f;
            _currentGlobalLockDuration = result.GlobalLockTime;
            _comboBuffer.isValid = false;

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

            Debug.Log($"[CombatController] Combo Transition: {nextNode.name} on {gameObject.name}. LockTime: {result.GlobalLockTime}");

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

            RpcPlayVisuals(_activeSlot, direction, hint);
        }

        [ObserversRpc(runLocally: true, requireServer: false)]
        private void RpcPlayVisuals(ActionSlot slot, Vector2 direction, NetworkedVisualHint hint)
        {
            if (_combatAnimator == null) return;
            var (prefab, sfx, hitVfx) = GetLocalVisualRefs(slot, hint.animationTrigger);
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

        private (GameObject prefab, AudioClip sfx, GameObject hitVfx) GetLocalVisualRefs(ActionSlot slot, string animationTrigger)
        {
            AttackNode rootNode = GetStartingNodeForSlot(slot);
            if (rootNode == null) return (null, null, null);

            AttackNode targetNode = FindNodeInTree(rootNode, animationTrigger);
            if (targetNode != null)
            {
                return (targetNode.visualData.projectilePrefab, targetNode.visualData.sfx, targetNode.visualData.hitVFXPrefab);
            }

            return (rootNode.visualData.projectilePrefab, rootNode.visualData.sfx, rootNode.visualData.hitVFXPrefab);
        }

        private AttackNode GetStartingNodeForSlot(ActionSlot slot)
        {
            var binding = _bindings.Find(b => b.slot == slot);
            if (binding.equipmentSlot == null) return null;

            var item = _equipmentManager.GetItemInSlot(binding.equipmentSlot);
            if (!item.IsValid) return null;

            var weapon = item.GetTrait<WeaponTrait>();
            if (weapon?.weaponSkill?.startingNode != null) return weapon.weaponSkill.startingNode;

            var spell = item.GetTrait<SpellTrait>();
            if (spell?.spellSkill?.startingNode != null) return spell.spellSkill.startingNode;

            return null;
        }

        private AttackNode FindNodeInTree(AttackNode root, string animationTrigger)
        {
            if (root == null || string.IsNullOrEmpty(animationTrigger)) return null;
            var visited = new HashSet<AttackNode>();
            return FindNodeInTreeInternal(root, animationTrigger, visited);
        }

        private AttackNode FindNodeInTreeInternal(AttackNode node, string animationTrigger, HashSet<AttackNode> visited)
        {
            if (node == null || visited.Contains(node)) return null;
            visited.Add(node);

            if (node.visualData.animationTrigger == animationTrigger)
                return node;

            var found = FindNodeInTreeInternal(node.normalNextNode, animationTrigger, visited);
            if (found != null) return found;

            return FindNodeInTreeInternal(node.perfectNextNode, animationTrigger, visited);
        }

        private int GetSkillLevelForSlot(ActionSlot slot)
        {
            var binding = _bindings.Find(b => b.slot == slot);
            if (binding.equipmentSlot == null) return 1;

            var item = _equipmentManager.GetItemInSlot(binding.equipmentSlot);
            if (!item.IsValid) return 1;

            var weapon = item.GetTrait<WeaponTrait>();
            if (weapon != null) return weapon.skillLevel;

            var spell = item.GetTrait<SpellTrait>();
            if (spell != null) return spell.skillLevel;

            return 1;
        }

        private Vector2 GetFacingDirection()
        {
            if (_playerController != null)
            {
                return new Vector2(_playerController.FacingDir, 0);
            }
            return Vector2.right;
        }
    }
}
