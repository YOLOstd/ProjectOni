using UnityEngine;

namespace ProjectOni.Data
{
    [CreateAssetMenu(fileName = "NewEquipment", menuName = "Project Oni/Items/Equipment")]
    public class EquipmentData : ItemData
    {
        [Header("Stat Modifiers")]
        public float healthBonus;
        public float damageBonus;
        public float defenseBonus;
    }
}
