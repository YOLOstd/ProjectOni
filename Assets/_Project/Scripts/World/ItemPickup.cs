using UnityEngine;
using ProjectOni.Core;
using ProjectOni.Data;
using ProjectOni.Player;
using PurrNet;

namespace ProjectOni.World
{
    /// <summary>
    /// Physical representation of an item in the game world that can be picked up.
    /// </summary>
    public class ItemPickup : NetworkBehaviour, IInteractable
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

        public void Interact(GameObject interactor)
        {
            if (!isServer) return;
            if (_item == null || interactor == null) return;

            bool handled = false;

            // 1. Try Auto-Equip if it's modular equipment
            var eqManager = interactor.GetComponentInChildren<EquipmentManager>();
            if (_item is EquipmentBlueprint blueprint && eqManager != null)
            {
                // Create instance on server
                var instance = new EquipmentInstance
                {
                    blueprint = blueprint,
                    itemLevel = 1,
                    seed = UnityEngine.Random.Range(0, int.MaxValue)
                };

                if (eqManager.EquipToFirstCompatibleSlot(instance))
                {
                    handled = true;
                    Debug.Log($"[ItemPickup] Server auto-equipped: {_item.itemName} to {interactor.name}");
                }
            }

            // 2. If not handled, add to bag
            var inventory = interactor.GetComponentInChildren<PlayerInventory>();
            if (!handled && inventory != null)
            {
                if (inventory.AddToBag(_item))
                {
                    handled = true;
                    Debug.Log($"[ItemPickup] Server added to bag: {_item.itemName} for {interactor.name}");
                }
            }

            if (handled)
            {
                // Network-synced destruction (server calls Despawn on networked object)
                Despawn();
            }
        }

        public string GetInteractionText()
        {
            return _item != null ? $"Pick up {_item.itemName}" : "Pick up Item";
        }
    }
}
