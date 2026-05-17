using UnityEngine;

namespace ProjectOni.Data
{
    [CreateAssetMenu(fileName = "New Melee Weapon Trait", menuName = "Project Oni/Traits/Melee Weapon")]
    public class MeleeWeaponTrait : WeaponTrait
    {
        public override string GetDescription()
        {
            if (attackData is ProjectOni.Combat.Data.MeleeAttackDataSO meleeData)
            {
                return base.GetDescription() + $"\nType: Melee | Range: {meleeData.swingRadius}m";
            }
            return base.GetDescription() + "\nType: Melee";
        }
    }
}
