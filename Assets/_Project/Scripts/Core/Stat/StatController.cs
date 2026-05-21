using System;
using System.Collections.Generic;
using UnityEngine;
using PurrNet;
using ProjectOni.Player;
using ProjectOni.Data;

namespace ProjectOni.Core
{
    /// <summary>
    /// The stat brain. Uses a topological sort (DAG) to resolve complex 
    /// attribute-to-stat conversions in the correct order, detecting cycles.
    /// </summary>
    public class StatController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private BaseStatsSO _baseStats;
        [SerializeField] private StatSettingsSO _globalSettings;

        private readonly Dictionary<StatType, Stat> _stats = new();
        private readonly List<AttributeConversion> _allConversions = new();
        
        private EquipmentManager _equipment;
        private EntityState      _entityState;

        public float Get(StatType type) =>
            _stats.TryGetValue(type, out var s) ? s.Value : 0f;

        public event Action OnRecalculated;

        private void Awake()
        {
            _equipment   = GetComponent<EquipmentManager>();
        }

        public void Initialize(EntityState entityState)
        {
            _entityState = entityState;
            if (_entityState != null)
            {
                _entityState.Level.onChangedWithOld += OnLevelChanged;
            }
            Recalculate();
        }

        private void OnDestroy()
        {
            if (_entityState != null)
            {
                _entityState.Level.onChangedWithOld -= OnLevelChanged;
            }
        }

        private void OnLevelChanged(int oldVal, int newVal)
        {
            Recalculate();
        }

        private void OnEnable()
        {
            GameEvents.OnEquipmentSlotChanged += OnEquipmentSlotChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnEquipmentSlotChanged -= OnEquipmentSlotChanged;
        }

        private void OnEquipmentSlotChanged(EquipmentManager manager,
            EquipmentSlotDefinition slot, EquipmentInstance item)
        {
            if (manager != _equipment) return;
            if (!_equipment.isOwner) return;

            Recalculate();
        }

        /// <summary>
        /// Main calculation pipeline.
        /// 1. Gather all conversions.
        /// 2. Get safe calculation order from solver.
        /// 3. Calculate each stat in order.
        /// </summary>
        public void Recalculate()
        {
            if (_baseStats == null || _globalSettings == null) return;

            _stats.Clear();
            _allConversions.Clear();

            // 0. Gather all conversions (Global + BaseStats + Equipment)
            GatherAllConversions();

            // 1. Get safe calculation order
            var order = StatDependencySolver.GetCalculationOrder(_allConversions);

            // 2. Calculate each stat in the determined order
            foreach (var type in order)
            {
                CalculateStat(type);
            }

            // 3. Write networked health stats (owner only, dirty-checked)
            if (_entityState != null && _entityState.isOwner)
            {
                float newMax = Get(StatType.Health);
                if (!Mathf.Approximately(_entityState.MaxHealth.value, newMax))
                {
                    float pct = _entityState.MaxHealth.value > 0f
                        ? _entityState.CurrentHealth.value / _entityState.MaxHealth.value
                        : 1f;

                    _entityState.MaxHealth.value     = newMax;
                    _entityState.CurrentHealth.value = Mathf.Round(newMax * pct);
                }
            }

            // 4. Flush all local (non-networked) stats to EntityState
            if (_entityState != null)
            {
                foreach (var kvp in _stats)
                {
                    if (kvp.Key == StatType.Health) continue; // Health is networked, skip
                    _entityState.SetStat(kvp.Key, kvp.Value.Value);
                }
            }

            OnRecalculated?.Invoke();
            GameEvents.TriggerStatsRecalculated(this);
        }

        private void GatherAllConversions()
        {
            // Global
            _allConversions.AddRange(_globalSettings.globalConversions);
            

            // Equipment traits
            if (_equipment != null)
            {
                foreach (var item in _equipment.GetAllEquipped())
                {
                    if (!item.IsValid) continue;
                    foreach (var trait in item.GetTraits())
                    {
                        if (trait is AttributeConversionTrait act)
                            _allConversions.AddRange(act.conversions);
                    }
                }
            }
        }

        private void CalculateStat(StatType type)
        {
            int level = _entityState != null ? _entityState.Level.value : 1;

            // Create the stat with base value
            float baseVal = GetBaseValueForStat(type, level);
            Stat stat = new Stat(baseVal);
            _stats[type] = stat;

            // Apply Equipment Modifiers (Flat/Increased from items)
            ApplyEquipmentModifiersToStat(type, stat);

            // Apply Conversions that target this stat
            foreach (var conv in _allConversions)
            {
                if (conv.targetStat != type) continue;
                
                float sourceValue = Get(conv.sourceStat);
                if (sourceValue > 0)
                {
                    stat.AddModifier(CreateModFromConversion(conv, sourceValue));
                }
            }
        }

        private float GetBaseValueForStat(StatType type, int level)
        {
            // Find base value in character SO
            foreach (var bs in _baseStats.stats)
            {
                if (bs.type == type)
                    return bs.value + (bs.growthPerLevel * level);
            }
            return 0f;
        }

        private void ApplyEquipmentModifiersToStat(StatType type, Stat stat)
        {
            if (_equipment == null) return;

            foreach (var item in _equipment.GetAllEquipped())
            {
                if (!item.IsValid) continue;

                // Blueprint base stats
                foreach (var bs in item.blueprint.baseStats)
                {
                    if (bs.type == type)
                    {
                        var modType = bs.isMultiplier ? ModType.Increased : ModType.Flat;
                        stat.AddModifier(new StatMod(modType, bs.value, item));
                    }
                }

                // Trait modifiers
                foreach (var trait in item.GetTraits())
                {
                    if (trait is StatModifierTrait smt)
                    {
                        foreach (var mod in smt.modifiers)
                        {
                            if (mod.type == type)
                            {
                                var modType = mod.isMultiplier ? ModType.Increased : ModType.Flat;
                                stat.AddModifier(new StatMod(modType, mod.value, item));
                            }
                        }
                    }
                }
            }
        }

        private StatMod CreateModFromConversion(AttributeConversion conv, float sourceValue)
        {
            float bonus = 0f;

            if (conv.mode == ConversionMode.Stepped)
            {
                if (conv.stepSize > 0)
                {
                    int steps = Mathf.FloorToInt(sourceValue / conv.stepSize);
                    bonus = steps * conv.valuePerStep;
                }
            }
            else // Ratio mode
            {
                bonus = sourceValue * conv.conversionRate;
            }

            // Handle percentages (Increased/More use 0.01 per 1%)
            if (conv.targetModType != ModType.Flat)
                bonus /= 100f;

            return new StatMod(conv.targetModType, bonus, "Conversion");
        }
    }
}
