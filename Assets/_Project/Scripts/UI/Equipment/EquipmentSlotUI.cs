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
        [SerializeField] private EquipmentSlot slotType;
        [SerializeField] private Image iconImage;
        [SerializeField] private Sprite emptySprite;

        public EquipmentSlot SlotType => slotType;

        private void Awake()
        {
            if (iconImage == null) iconImage = GetComponentInChildren<Image>();
        }

        public void SetItem(ModularEquipmentData item)
        {
            if (item != null && item.icon != null)
            {
                iconImage.sprite = item.icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.sprite = emptySprite;
                // If there's no empty sprite, hidden the image component to avoid white squares
                iconImage.enabled = emptySprite != null;
            }
        }
    }
}
