using UnityEngine;

namespace ProjectOni.Data
{
    [CreateAssetMenu(fileName = "New Ranged Weapon Trait", menuName = "Project Oni/Traits/Ranged Weapon")]
    public class RangedWeaponTrait : WeaponTrait
    {
        public override string GetDescription()
        {
            if (weaponSkill != null && weaponSkill.startingNode != null)
            {
                return base.GetDescription() + $"\nType: Ranged | Proj Speed: {weaponSkill.startingNode.visualData.projectileSpeed}m/s";
            }
            return base.GetDescription() + "\nType: Ranged";
        }
    }
}
