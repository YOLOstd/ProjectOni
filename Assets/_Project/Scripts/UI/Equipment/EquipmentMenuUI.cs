using UnityEngine;
using ProjectOni.Core;
using ProjectOni.Data;
using ProjectOni.Player;
using System.Collections.Generic;

namespace ProjectOni.UI
{
    /// <summary>
    /// Manages the Equipment Menu UI.
    /// Responds to slot change events and handles menu toggling.
    /// </summary>
    public class EquipmentMenuUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private InputReader inputReader;

        private List<EquipmentSlotUI> _slots = new List<EquipmentSlotUI>();

        private void Awake()
        {
            // Find all slot components in children
            _slots.AddRange(GetComponentsInChildren<EquipmentSlotUI>(true));
            
            // Ensure menu is closed on start
            if (menuPanel != null) menuPanel.SetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnEquipmentSlotChanged += HandleSlotChanged;
            
            if (inputReader != null)
            {
                inputReader.MenuTogglePressed += ToggleMenu;
            }
            else
            {
                Debug.LogWarning("EquipmentMenuUI: Input Reader is not assigned!");
            }
        }

        private void OnDisable()
        {
            GameEvents.OnEquipmentSlotChanged -= HandleSlotChanged;
            
            if (inputReader != null)
            {
                inputReader.MenuTogglePressed -= ToggleMenu;
            }
        }

        public void ToggleMenu()
        {
            if (menuPanel == null)
            {
                Debug.LogWarning("EquipmentMenuUI: Menu Panel is not assigned!");
                return;
            }
            
            bool isOpening = !menuPanel.activeSelf;
            Debug.Log($"EquipmentMenuUI: Toggling menu. Opening: {isOpening}");
            menuPanel.SetActive(isOpening);
            
            // Handle game state when menu is open
            if (isOpening)
            {
                // Optional: Pause game or unlock cursor
                // Time.timeScale = 0f; 
                Debug.Log("Equipment Menu Opened");
            }
            else
            {
                // Time.timeScale = 1f;
                Debug.Log("Equipment Menu Closed");
            }
        }

        private void HandleSlotChanged(EquipmentSlot slot, ModularEquipmentData item)
        {
            foreach (var slotUI in _slots)
            {
                if (slotUI.SlotType == slot)
                {
                    slotUI.SetItem(item);
                }
            }
        }
    }
}
