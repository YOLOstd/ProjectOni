using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOni.Data
{
    [CreateAssetMenu(fileName = "New Trait Loot Table", menuName = "Project Oni/Items/Trait Loot Table")]
    public class TraitLootTable : ScriptableObject
    {
        [Serializable]
        public struct TraitEntry
        {
            public EquipmentTraitSO trait;
            public int weight;
        }

        public List<TraitEntry> possibleTraits = new List<TraitEntry>();

        public EquipmentTraitSO GetRandomTrait(System.Random random)
        {
            if (possibleTraits == null || possibleTraits.Count == 0) return null;

            int totalWeight = 0;
            foreach (var entry in possibleTraits) totalWeight += entry.weight;

            if (totalWeight <= 0) return null;

            int roll = random.Next(0, totalWeight);
            int currentWeight = 0;

            foreach (var entry in possibleTraits)
            {
                currentWeight += entry.weight;
                if (roll < currentWeight)
                {
                    return entry.trait;
                }
            }

            return null;
        }
    }
}
