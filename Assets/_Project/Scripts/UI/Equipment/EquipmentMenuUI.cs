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
            
            // Safety check for duplicate slot assignments in UI
            HashSet<EquipmentSlotDefinition> assignedDefinitions = new HashSet<EquipmentSlotDefinition>();
            foreach (var slot in _slots)
            {
                if (slot.SlotDefinition == null) continue;
                if (!assignedDefinitions.Add(slot.SlotDefinition))
                {
                    Debug.LogWarning($"[EquipmentMenuUI] Multiple UI slots are assigned to the same definition: {slot.SlotDefinition.slotName}. This will cause display issues!");
                }
            }

            // Ensure menu is closed on start
            menuPanel?.SetActive(false);
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
                return;
            }

            
            bool isOpening = !menuPanel.activeSelf;
            menuPanel.SetActive(isOpening);
            
        }

        private void HandleSlotChanged(EquipmentSlotDefinition slot, EquipmentInstance item)
        {
            bool foundSlot = false;
            foreach (var slotUI in _slots)
            {
                if (slotUI.SlotDefinition == slot)
                {
                    Debug.Log($"[EquipmentMenuUI] Updating UI slot {slot.slotName} with {item.blueprint?.itemName ?? "Empty"}");
                    slotUI.SetItem(item);
                    foundSlot = true;
                }
            }

            if (!foundSlot && item.IsValid)
            {
                Debug.LogWarning($"[EquipmentMenuUI] Received update for slot {slot.slotName}, but no UI slot is configured for it!");
            }
        }
    }
}
