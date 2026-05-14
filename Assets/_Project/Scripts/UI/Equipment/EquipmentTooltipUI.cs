using UnityEngine;
using TMPro;
using ProjectOni.Data;
using System.Collections.Generic;
using System.Text;

namespace ProjectOni.UI
{
    public class EquipmentTooltipUI : MonoBehaviour
    {
        public static EquipmentTooltipUI Instance { get; private set; }

        [Header("Header References")]
        [SerializeField] private GameObject contentPanel;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI baseStatsText; // Single string summary

        [Header("Containers")]
        [SerializeField] private Transform traitsContainer;

        [Header("Prefabs")]
        [SerializeField] private TraitRowUI rowPrefab;

        [Header("Settings")]
        [SerializeField] private Vector2 offset = new Vector2(15, 0);
        
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private List<TraitRowUI> _activeRows = new List<TraitRowUI>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            if (_rectTransform != null)
                _rectTransform.pivot = new Vector2(0, 0.5f);

            Hide();
        }

        public void Show(EquipmentInstance item)
        {
            if (contentPanel == null) return;
            
            UpdateContent(item);
            contentPanel.SetActive(true);
            UpdatePosition();
        }

        private void UpdateContent(EquipmentInstance item)
        {
            if (itemNameText != null) itemNameText.text = item.blueprint.itemName + $"  Lvl {item.itemLevel}";

            // Format Base Stats as a single summary string
            if (baseStatsText != null)
            {
                float dmg = 0, ats = 0, crit = 0;
                bool hasStats = false;

                if (item.blueprint.baseStats != null)
                {
                    foreach (var stat in item.blueprint.baseStats)
                    {
                        if (stat.type == StatType.Attack) { dmg = stat.value; hasStats = true; }
                        else if (stat.type == StatType.AttackSpeed) { ats = stat.value; hasStats = true; }
                        else if (stat.type == StatType.CritChance) { crit = stat.value; hasStats = true; }
                    }
                }

                if (hasStats)
                {
                    baseStatsText.gameObject.SetActive(true);
                    // Format: D: 1, ATS: 0.3, CRIT: 0.5%
                    baseStatsText.text = $"D: {dmg}, ATS: {ats}, CRIT: {crit}%";
                }
                else
                {
                    baseStatsText.gameObject.SetActive(false);
                }
            }

            ClearRows();

            // Spawn Traits as individual rows
            if (traitsContainer != null)
            {
                var traits = item.GetTraits();
                foreach (var trait in traits)
                {
                    SpawnRow(traitsContainer, trait.GetDescription());
                }
                traitsContainer.gameObject.SetActive(traits.Count > 0);
            }
        }

        private void SpawnRow(Transform container, string text)
        {
            if (rowPrefab == null) return;
            
            var row = Instantiate(rowPrefab, container);
            row.SetText(text);
            _activeRows.Add(row);
        }

        private void ClearRows()
        {
            foreach (var row in _activeRows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            _activeRows.Clear();
        }

        public void Hide()
        {
            if (contentPanel != null)
                contentPanel.SetActive(false);
        }

        private void Update()
        {
            if (contentPanel != null && contentPanel.activeSelf)
            {
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            var pointer = UnityEngine.InputSystem.Pointer.current;
            if (pointer == null) return;
            
            Vector2 mousePos = pointer.position.ReadValue();
            transform.position = mousePos + offset;
        }
    }
}
