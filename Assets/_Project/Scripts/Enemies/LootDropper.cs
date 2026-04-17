using UnityEngine;
using ProjectOni.Data;

namespace ProjectOni.Enemies
{
    public class LootDropper : MonoBehaviour
    {
        [Header("Loot Pool")]
        [SerializeField] private ItemData[] lootPool;
        [SerializeField, Range(0, 1)] private float dropChance = 0.5f;

        public void DropLoot()
        {
            if (Random.value > dropChance || lootPool == null || lootPool.Length == 0) return;

            // Pick a random item from pool
            int index = Random.Range(0, lootPool.Length);
            ItemData droppedItem = lootPool[index];

            Debug.Log($"Dropped item: {droppedItem.itemName}");

            // Spawning logic (normally spawn a physical ItemPickup prefab)
            // Instantiate(itemPickupPrefab, transform.position, Quaternion.identity).Initialize(droppedItem);
        }
    }
}
