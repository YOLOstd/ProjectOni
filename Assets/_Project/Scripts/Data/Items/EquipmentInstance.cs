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
            if (_cachedTraits == null && blueprint != null)
            {
                ReconstructTraits();
            }
            return _cachedTraits ?? new List<IEquipmentTrait>();
        }

        public void ReconstructTraits()
        {
            _cachedTraits = new List<IEquipmentTrait>();
            if (blueprint == null) return;

            // 1. If this is a weapon blueprint, inject its dedicated weapon trait first
            if (blueprint is WeaponBlueprint weaponBlueprint && weaponBlueprint.weaponTrait != null)
            {
                _cachedTraits.Add(weaponBlueprint.weaponTrait);
            }

            // 2. Roll random traits from the loot table if it is defined
            if (blueprint.traitLootTable != null)
            {
                var random = new System.Random(seed);
                for (int i = 0; i < 5; i++)
                {
                    var trait = blueprint.traitLootTable.GetRandomTrait(random);
                    if (trait != null)
                    {
                        _cachedTraits.Add(trait);
                    }
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
