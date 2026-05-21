using UnityEngine;

namespace ProjectOni.Data
{
    [CreateAssetMenu(fileName = "New Melee Weapon Trait", menuName = "Project Oni/Traits/Melee Weapon")]
    public class MeleeWeaponTrait : WeaponTrait
    {
        public override string GetDescription()
        {
            return base.GetDescription() + "\nType: Melee";
        }
    }
}
