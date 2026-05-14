using System.Collections.Generic;
using UnityEngine;

namespace ProjectOni.Data
{
    [CreateAssetMenu(fileName = "New Equipment Blueprint", menuName = "Project Oni/Items/Equipment Blueprint")]
    public class EquipmentBlueprint : ItemData
    {
        public ItemCategoryTag category; // What this item is (Weapon, Ring, etc.)
        
        [Header("Archetype Data")]
        public List<BaseStat> baseStats = new List<BaseStat>();
        public TraitLootTable traitLootTable;

        [Header("Visuals")]
        public GameObject visualPrefab;
    }
}
