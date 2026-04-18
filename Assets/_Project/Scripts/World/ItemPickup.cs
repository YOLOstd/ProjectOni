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
             Debug.Log($"[ItemPickup] Interact method called on {gameObject.name}");
            if (_item == null) return;

            // Find player and add to inventory
            // We search for PlayerInventory on the object that interacts or just find the singleton player if one exists
            // For now, we'll find the player by tag
            var player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log($"[ItemPickup] Found player: {player != null}");
            if (player != null && player.TryGetComponent(out PlayerInventory inventory))
            {
                Debug.Log($"[ItemPickup] Found inventory on player: {inventory != null}");
                inventory.EquipItem(_item);
                Debug.Log($"[ItemPickup] Picked up: {_item.itemName}");
                
                // Fire generic item pickup event if needed
                // GameEvents.TriggerItemPickedUp(_item);
                
                Destroy(gameObject);
            }
        }

        public string GetInteractionText()
        {
            return _item != null ? $"Pick up {_item.itemName}" : "Pick up Item";
        }
    }
}
