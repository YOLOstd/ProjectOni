using UnityEngine;

namespace ProjectOni.Data
{
    [CreateAssetMenu(fileName = "New Ranged Weapon Trait", menuName = "Project Oni/Traits/Ranged Weapon")]
    public class RangedWeaponTrait : WeaponTrait
    {
        public override string GetDescription()
        {
            if (attackData is ProjectOni.Combat.Data.RangedAttackDataSO rangedData)
            {
                return base.GetDescription() + $"\nType: Ranged | Proj Speed: {rangedData.projectileSpeed}";
            }
            return base.GetDescription() + "\nType: Ranged";
        }
    }
}
