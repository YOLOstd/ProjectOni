using UnityEngine;
using ProjectOni.Data;

namespace ProjectOni.Enemies
{
    public class LootDropper : MonoBehaviour
    {
        [Header("Loot Pool")]
        [SerializeField] private ItemData[] lootPool;
        [SerializeField, Range(0, 1)] private float dropChance = 0.5f;
        [SerializeField] private World.ItemPickup itemPickupPrefab;

        public void DropLoot()
        {
            if (Random.value > dropChance || lootPool == null || lootPool.Length == 0) return;

            // Pick a random item from pool
            int index = Random.Range(0, lootPool.Length);
            ItemData droppedItem = lootPool[index];

            Debug.Log($"Dropped item: {droppedItem.itemName}");

            // Spawning logic
            if (itemPickupPrefab != null)
            {
                var pickup = Instantiate(itemPickupPrefab, transform.position, Quaternion.identity);
                pickup.Initialize(droppedItem);
            }
        }
    }
}
