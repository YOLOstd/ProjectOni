using UnityEngine;

namespace ProjectOni.Data
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Project Oni/Items/Weapon")]
    public class WeaponData : ItemData
    {
        [Header("Combat Stats")]
        public float baseDamage;
        public float attackSpeed;

        [Header("Animation")]
        public AnimationClip attackAnimation;

        private void OnEnable()
        {
            type = ItemType.Weapon;
        }
    }
}
