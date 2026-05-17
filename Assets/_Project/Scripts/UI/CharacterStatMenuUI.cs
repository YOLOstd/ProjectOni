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
            _stats = stats;
            SpawnRows();
        }

        private void OnEnable()
        {
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
            }
        }

        private void HandleStatsRecalculated(StatController stats)
        {
            if (stats != _stats) return;
            RefreshRows();
        }

        private void SpawnRows()
        {
            if (_rowContainer == null || _rowPrefab == null) return;

            // Clear existing
            foreach (Transform child in _rowContainer)
            {
                Destroy(child.gameObject);
            }
            _rows.Clear();

            // Spawn configured stats
            foreach (var type in _displayedStats)
            {
                var row = Instantiate(_rowPrefab, _rowContainer);
                row.Set(type.ToString(), _stats.Get(type));
                _rows[type] = row;
            }
        }

        private void RefreshRows()
        {
            if (_stats == null) return;

            foreach (var kvp in _rows)
            {
                kvp.Value.Set(kvp.Key.ToString(), _stats.Get(kvp.Key));
            }
        }
    }
}
