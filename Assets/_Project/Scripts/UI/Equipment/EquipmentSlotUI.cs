using UnityEngine;
using UnityEngine.UI;
using ProjectOni.Data;

namespace ProjectOni.UI
{
    /// <summary>
    /// Represents a single equipment slot in the UI.
    /// Listens for item changes and updates its icon.
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour
    {
        [SerializeField] private EquipmentSlotDefinition slotDefinition;
        [SerializeField] private Image iconImage;
        [SerializeField] private Sprite emptySprite;

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
    }
}
