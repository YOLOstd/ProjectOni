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

        private List<EquipmentSlotUI> _slots = new List<EquipmentSlotUI>();

        private void Awake()
        {
            // Find all slot components in children
            _slots.AddRange(GetComponentsInChildren<EquipmentSlotUI>(true));
            
            // CRITICAL: If this script is on the panel itself, it can't toggle itself back ON.
            if (menuPanel != null && menuPanel == gameObject)
            {
                Debug.LogError("EquipmentMenuUI: Script is attached to the same GameObject as MenuPanel! " +
                                 "This will prevent the menu from opening after it is closed. " +
                                 "Please move this script to a parent object that stays active.");
            }

            // Ensure menu is closed on start
            if (menuPanel != null) menuPanel.SetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnEquipmentSlotChanged += HandleSlotChanged;
        }

        private void Start()
        {
            // Subscribe in Start to ensure InputManager.Instance is initialized
            SubscribeToInput();
        }

        private void SubscribeToInput()
        {
            var input = ProjectOni.Managers.InputManager.Instance;
            if (input != null)
            {
                // Unsubscribe first to avoid double subscription
                input.MenuTogglePressed -= ToggleMenu;
                input.MenuTogglePressed += ToggleMenu;
                Debug.Log("EquipmentMenuUI: Successfully subscribed to MenuTogglePressed.");
            }
            else
            {
                Debug.LogWarning("EquipmentMenuUI: InputManager instance not found in Start!");
            }
        }

        private void OnDisable()
        {
            GameEvents.OnEquipmentSlotChanged -= HandleSlotChanged;
            
            var input = ProjectOni.Managers.InputManager.Instance;
            if (input != null)
            {
                input.MenuTogglePressed -= ToggleMenu;
            }
        }

        public void ToggleMenu()
        {
            if (menuPanel == null)
            {
                Debug.LogWarning("EquipmentMenuUI: Menu Panel is not assigned!");
                return;
            }

            // Warning: If this script is ON the menuPanel, it will disable itself!
            if (menuPanel == gameObject)
            {
                Debug.LogWarning("EquipmentMenuUI: Script is attached to the same GameObject as MenuPanel. " +
                                 "Closing the menu will disable this script, preventing it from opening again!");
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

        private void HandleSlotChanged(EquipmentSlotDefinition slot, ModularEquipmentData item)
        {
            foreach (var slotUI in _slots)
            {
                if (slotUI.SlotDefinition == slot)
                {
                    slotUI.SetItem(item);
                }
            }
        }
    }
}
