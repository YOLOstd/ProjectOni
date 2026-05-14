using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

namespace ProjectOni.Data
{
    /// <summary>
    /// A runtime instance of an item. 
    /// This struct is synced over the network.
    /// Traits are reconstructed locally using the seed.
    /// </summary>
    [Serializable]
    public struct EquipmentInstance
    {
        public EquipmentBlueprint blueprint;
        public int itemLevel;
        public int seed;

        // Non-synced cache of traits
        [NonSerialized] private List<IEquipmentTrait> _cachedTraits;

        public List<IEquipmentTrait> GetTraits()
        {
            if (_cachedTraits == null && blueprint != null && blueprint.traitLootTable != null)
            {
                ReconstructTraits();
            }
            return _cachedTraits ?? new List<IEquipmentTrait>();
        }

        public void ReconstructTraits()
        {
            if (blueprint == null || blueprint.traitLootTable == null)
            {
                _cachedTraits = new List<IEquipmentTrait>();
                return;
            }

            var random = new System.Random(seed);
            _cachedTraits = new List<IEquipmentTrait>();

            // Roll up to 5 traits (or based on some logic)
            // For now, let's say 5 traits as per the user's request
            for (int i = 0; i < 5; i++)
            {
                var trait = blueprint.traitLootTable.GetRandomTrait(random);
                if (trait != null)
                {
                    _cachedTraits.Add(trait);
                }
            }
        }

        public T GetTrait<T>() where T : class, IEquipmentTrait
        {
            foreach (var trait in GetTraits())
            {
                if (trait is T match) return match;
            }
            return null;
        }

        public bool HasTrait<T>() where T : class, IEquipmentTrait
        {
            return GetTrait<T>() != null;
        }

        public bool IsValid => blueprint != null;
    }
}
