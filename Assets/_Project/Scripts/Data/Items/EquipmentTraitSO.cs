using UnityEngine;

namespace ProjectOni.Data
{
    /// <summary>
    /// Base ScriptableObject for all equipment traits.
    /// This allows us to store trait references in a list while maintaining the interface.
    /// </summary>
    public abstract class EquipmentTraitSO : ScriptableObject, IEquipmentTrait 
    {
        public string traitName;

        public abstract string GetDescription();
    }
}
