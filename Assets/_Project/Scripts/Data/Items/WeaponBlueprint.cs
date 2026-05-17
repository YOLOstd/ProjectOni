using UnityEngine;
using PurrNet;

namespace ProjectOni.Data
{
    [RegisterNetworkType(typeof(WeaponBlueprint))]
    [CreateAssetMenu(fileName = "New Weapon Blueprint", menuName = "Project Oni/Items/Weapon Blueprint")]
    public class WeaponBlueprint : EquipmentBlueprint
    {
        [Header("Slotted Weapon Skill")]
        [Tooltip("The fixed weapon trait (melee or ranged) assigned directly to this weapon.")]
        public WeaponTrait weaponTrait;
    }
}
