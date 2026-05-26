using System.Collections.Generic;
using UnityEngine;
using ProjectOni.Core;
using ProjectOni.Data;
using ProjectOni.Managers;

namespace ProjectOni.UI
{
    /// <summary>
    /// Displays a dynamic list of character stats.
    /// Toggles visibility when the menu toggle button (Tab) is pressed.
    /// </summary>
    public class CharacterStatMenuUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _menuPanel;

        [Header("Row Spawning")]
        [SerializeField] private StatRowUI _rowPrefab;
        [SerializeField] private Transform _rowContainer;

        [Header("Configuration")]
        [SerializeField] private StatType[] _displayedStats = 
        { 
            StatType.Health, 
            StatType.Armor, 
            StatType.Evasion,
            StatType.Strength,
            StatType.Dexterity,
            StatType.Intelligence
        };

        private StatController _stats;
        private readonly Dictionary<StatType, StatRowUI> _rows = new();

        /// <summary>
        /// Binds the menu to a specific StatController (the local player's).
        /// </summary>
        public void Bind(StatController stats)
        {
            Debug.Log($"[CharacterStatMenuUI] Bind called. stats is null? {stats == null}");
            _stats = stats;
            SpawnRows();
        }

        private void OnEnable()
        {
            Debug.Log($"[CharacterStatMenuUI] OnEnable");
            GameEvents.OnStatsRecalculated += HandleStatsRecalculated;
            SubscribeToInput();
            
            if (_menuPanel != null)
                _menuPanel.SetActive(false);
        }

        private void OnDisable()
        {
            GameEvents.OnStatsRecalculated -= HandleStatsRecalculated;
            UnsubscribeFromInput();
        }

        private void Start()
        {
            SubscribeToInput();
        }

        private void SubscribeToInput()
        {
            var input = InputManager.Instance;
            if (input != null)
            {
                input.MenuTogglePressed -= ToggleMenu;
                input.MenuTogglePressed += ToggleMenu;
            }
        }

        private void UnsubscribeFromInput()
        {
            var input = InputManager.Instance;
            if (input != null)
            {
                input.MenuTogglePressed -= ToggleMenu;
            }
        }

        private void ToggleMenu()
        {
            if (_menuPanel != null)
            {
                _menuPanel.SetActive(!_menuPanel.activeSelf);
                Debug.Log($"[CharacterStatMenuUI] Menu toggled. Active self: {_menuPanel.activeSelf}");
            }
            else
            {
                Debug.LogError("[CharacterStatMenuUI] _menuPanel is null! Cannot toggle.");
            }
        }

        private void HandleStatsRecalculated(StatController stats)
        {
            if (stats != _stats) return;
            RefreshRows();
        }

        private void SpawnRows()
        {
            Debug.Log($"[CharacterStatMenuUI] SpawnRows called. _rowContainer is null? {_rowContainer == null}, _rowPrefab is null? {_rowPrefab == null}");
            if (_rowContainer == null || _rowPrefab == null)
            {
                Debug.LogError("[CharacterStatMenuUI] Cannot spawn rows: container or prefab is null!");
                return;
            }

            // Clear existing
            foreach (Transform child in _rowContainer)
            {
                Destroy(child.gameObject);
            }
            _rows.Clear();

            if (_displayedStats == null || _displayedStats.Length == 0)
            {
                Debug.LogWarning("[CharacterStatMenuUI] _displayedStats is null or empty! Falling back to defaults.");
                _displayedStats = new[]
                {
                    StatType.Health,
                    StatType.Armor,
                    StatType.Evasion,
                    StatType.Strength,
                    StatType.Dexterity,
                    StatType.Intelligence
                };
            }

            Debug.Log($"[CharacterStatMenuUI] Spawning {_displayedStats.Length} rows...");

            // Spawn configured stats
            foreach (var type in _displayedStats)
            {
                var row = Instantiate(_rowPrefab, _rowContainer);
                if (row == null)
                {
                    Debug.LogError($"[CharacterStatMenuUI] Failed to instantiate row for {type}!");
                    continue;
                }
                int value = _stats != null ? Mathf.RoundToInt(_stats.Get(type)) : 0;
                row.Set(type.ToString(), value);
                _rows[type] = row;
            }
        }

        private void RefreshRows()
        {
            if (_stats == null)
            {
                Debug.LogWarning("[CharacterStatMenuUI] RefreshRows: _stats is null!");
                return;
            }

            Debug.Log("[CharacterStatMenuUI] RefreshRows called.");
            foreach (var kvp in _rows)
            {
                int val = Mathf.RoundToInt(_stats.Get(kvp.Key));
                Debug.Log($"[CharacterStatMenuUI] Refreshing {kvp.Key} = {val}");
                kvp.Value.Set(kvp.Key.ToString(), val);
            }
        }
    }
}
