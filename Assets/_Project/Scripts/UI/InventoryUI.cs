using UnityEngine;
using UnityEngine.UI;
using ProjectOni.Core;
using ProjectOni.Data;

namespace ProjectOni.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Equipment Icons")]
        [SerializeField] private Image weaponIcon;
        [SerializeField] private Image chestIcon;
        [SerializeField] private Image ringOneIcon;
        [SerializeField] private Image ringTwoIcon;

        [Header("Fallbacks")]
        [SerializeField] private Sprite emptyIcon;

        private void OnEnable()
        {
            GameEvents.OnItemEquipped += UpdateInventoryUI;
        }

        private void OnDisable()
        {
            GameEvents.OnItemEquipped -= UpdateInventoryUI;
        }

        private void UpdateInventoryUI(ItemData item)
        {
            // Simple logic matching EquiptItem in PlayerInventory
            switch (item.type)
            {
                case ItemType.Weapon:
                    UpdateIcon(weaponIcon, item.icon);
                    break;
                case ItemType.Chest:
                    UpdateIcon(chestIcon, item.icon);
                    break;
                case ItemType.Ring:
                    // Usually more logic to find which ring was swapped
                    UpdateIcon(ringOneIcon, item.icon);
                    break;
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
