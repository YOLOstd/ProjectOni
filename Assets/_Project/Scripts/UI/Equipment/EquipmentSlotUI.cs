using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ProjectOni.Data;

namespace ProjectOni.UI
{
    /// <summary>
    /// Represents a single equipment slot in the UI.
    /// Listens for item changes and updates its icon.
    /// Also handles tooltips on hover.
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private EquipmentSlotDefinition slotDefinition;
        [SerializeField] private Image iconImage;
        [SerializeField] private Sprite emptySprite;

        private EquipmentInstance _currentItem;

        public EquipmentSlotDefinition SlotDefinition => slotDefinition;

        private void Awake()
        {
            if (iconImage == null) iconImage = GetComponentInChildren<Image>();
            
            // Default icon from the definition if emptySprite is not set
            if (emptySprite == null && slotDefinition != null)
            {
                emptySprite = slotDefinition.defaultIcon;
            }
        }

        public void SetItem(EquipmentInstance item)
        {
            _currentItem = item;

            // Ensure components are initialized even if Awake hasn't run yet
            if (iconImage == null) iconImage = GetComponentInChildren<Image>();
            if (iconImage == null) return;

            if (item.IsValid && item.blueprint.icon != null)
            {
                iconImage.sprite = item.blueprint.icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.sprite = emptySprite;
                // If there's no empty sprite, hide the image component to avoid white squares
                iconImage.enabled = emptySprite != null;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_currentItem.IsValid && ProjectOni.UI.EquipmentTooltipUI.Instance != null)
            {
                ProjectOni.UI.EquipmentTooltipUI.Instance.Show(_currentItem);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (ProjectOni.UI.EquipmentTooltipUI.Instance != null)
            {
                ProjectOni.UI.EquipmentTooltipUI.Instance.Hide();
            }
        }
        
        private void OnDisable()
        {
            // Ensure tooltip is hidden if the slot disappears (e.g. menu closed)
            if (ProjectOni.UI.EquipmentTooltipUI.Instance != null)
            {
                ProjectOni.UI.EquipmentTooltipUI.Instance.Hide();
            }
        }
    }
}
