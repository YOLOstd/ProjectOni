using System.Collections.Generic;
using UnityEngine;
using ProjectOni.Data;

namespace ProjectOni.Core
{
    /// <summary>
    /// A single stat with a base value and a layered modifier stack.
    /// Uses a dirty flag so the expensive PoE math only runs when something changes.
    /// </summary>
    [System.Serializable]
    public class Stat
    {
        public float baseValue;

        private readonly List<StatMod> _modifiers = new();
        private bool  _isDirty     = true;
        private float _cachedValue;

        public Stat(float baseValue) => this.baseValue = baseValue;

        /// <summary>Final computed value. Cached until a modifier is added/removed.</summary>
        public float Value
        {
            get
            {
                if (_isDirty)
                {
                    _cachedValue = CalculateFinalValue();
                    _isDirty = false;
                }
                return _cachedValue;
            }
        }

        public void AddModifier(StatMod mod)
        {
            _modifiers.Add(mod);
            _isDirty = true;
        }

        /// <summary>Removes all modifiers contributed by a given source (e.g. an unequipped item).</summary>
        public void RemoveModifiersFromSource(object source)
        {
            if (_modifiers.RemoveAll(m => m.Source == source) > 0)
                _isDirty = true;
        }

        public void ClearAllModifiers()
        {
            _modifiers.Clear();
            _isDirty = true;
        }

        // (Base + Flat) * (1 + SumIncreased) * ProductMore
        private float CalculateFinalValue()
        {
            float finalValue   = baseValue;
            float sumIncreased = 0f;
            float totalMore    = 1f;

            foreach (var mod in _modifiers)
            {
                switch (mod.Type)
                {
                    case ModType.Flat:      finalValue   += mod.Value;        break;
                    case ModType.Increased: sumIncreased += mod.Value;        break;
                    case ModType.More:      totalMore    *= (1f + mod.Value); break;
                }
            }

            return finalValue * (1f + sumIncreased) * totalMore;
        }
    }
}
