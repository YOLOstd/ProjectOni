using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ProjectOni.Core;
using ProjectOni.Data;

namespace ProjectOni.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [System.Serializable]
        public struct SlotMapping
        {
            public EquipmentSlotDefinition definition;
            public Image targetIcon;
        }

        [Header("Dynamic Mappings")]
        [SerializeField] private List<SlotMapping> slotMappings;

        [Header("Fallbacks")]
        [SerializeField] private Sprite emptyIcon;

        private void OnEnable()
        {
            GameEvents.OnEquipmentSlotChanged += HandleSlotChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnEquipmentSlotChanged -= HandleSlotChanged;
        }

        private void HandleSlotChanged(EquipmentSlotDefinition slot, ModularEquipmentData item)
        {
            foreach (var mapping in slotMappings)
            {
                if (mapping.definition == slot)
                {
                    UpdateIcon(mapping.targetIcon, item != null ? item.icon : null);
                }
            }
        }

        private void UpdateIcon(Image icon, Sprite sprite)
        {
            if (icon == null) return;
            icon.sprite = sprite != null ? sprite : emptyIcon;
            icon.enabled = icon.sprite != null;
        }
    }
}
