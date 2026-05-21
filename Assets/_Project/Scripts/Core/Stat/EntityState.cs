using System;
using System.Collections.Generic;
using UnityEngine;
using PurrNet;
using ProjectOni.Data;

namespace ProjectOni.Core
{
    /// <summary>
    /// The absolute source of truth for an entity's networked and local state data.
    /// Exposes no logic, just holds data and fires events.
    /// </summary>
    public class EntityState : NetworkBehaviour
    {
        // ─── Networked Stats (SyncVars) ───────────────────────────────────────
        public readonly SyncVar<float> CurrentHealth = new(0f, ownerAuth: true);
        public readonly SyncVar<float> MaxHealth = new(0f, ownerAuth: true);
        public readonly SyncVar<int> Level = new(1); // Server-auth by default

        // ─── Local Stats ──────────────────────────────────────────────────────
        private readonly Dictionary<StatType, float> _localStats = new();
        public event Action<StatType, float> OnLocalStatChanged;

        // ─── Local Stats API ──────────────────────────────────────────────────
        
        /// <summary>
        /// Gets the value of a local stat safely using TryGetValue.
        /// </summary>
        public float GetStat(StatType type)
        {
            if (_localStats.TryGetValue(type, out float value))
            {
                return value;
            }
            return 0f;
        }

        /// <summary>
        /// Sets a local stat and triggers the change event.
        /// </summary>
        public void SetStat(StatType type, float value)
        {
            float oldValue = GetStat(type);
            _localStats[type] = value;

            if (!Mathf.Approximately(oldValue, value))
            {
                OnLocalStatChanged?.Invoke(type, value);
            }
        }
    }
}
