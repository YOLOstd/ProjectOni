using UnityEngine;
using ProjectOni.Core;
using ProjectOni.Data;
using ProjectOni.Player;

namespace ProjectOni.World
{
    /// <summary>
    /// Physical representation of an item in the game world that can be picked up.
    /// </summary>
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData _item;
        [SerializeField] private SpriteRenderer _iconRenderer;

        private void OnValidate()
        {
            if (_item != null && _iconRenderer != null)
            {
                _iconRenderer.sprite = _item.icon;
                gameObject.name = $"Pickup_{_item.itemName}";
            }
        }

        public void Initialize(ItemData item)
        {
            _item = item;
            if (_iconRenderer != null) _iconRenderer.sprite = _item.icon;
            gameObject.name = $"Pickup_{_item.itemName}";
        }

        public void Interact()
        {
            Debug.Log("INTERACT");
            if (_item == null) return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            bool handled = false;

            // 1. Try Auto-Equip if it's modular equipment
            var eqManager = player.GetComponentInChildren<EquipmentManager>();
            if (_item is EquipmentBlueprint blueprint && eqManager != null && blueprint.category != null)
            {
                // Create a temporary instance with a random seed for now
                var instance = new EquipmentInstance
                {
                    blueprint = blueprint,
                    itemLevel = 1, // Default level
                    seed = UnityEngine.Random.Range(0, int.MaxValue)
                };

                // Auto-equip ONLY if there is a compatible empty slot
                if (eqManager.EquipToFirstCompatibleSlot(instance))
                {
                    handled = true;
                    Debug.Log($"[ItemPickup] Auto-equipped: {_item.itemName}");
                }
            }

            // 2. If not handled, add to bag
            var inventory = player.GetComponentInChildren<PlayerInventory>();
            if (!handled && inventory != null)
            {
                if (inventory.AddToBag(_item))
                {
                    handled = true;
                    Debug.Log($"[ItemPickup] Added to bag: {_item.itemName}");
                }
            }

            if (handled)
            {
                Destroy(gameObject);
            }
        }

        public string GetInteractionText()
        {
            return _item != null ? $"Pick up {_item.itemName}" : "Pick up Item";
        }
    }
}
