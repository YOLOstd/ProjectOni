using System;
using UnityEngine;

namespace ProjectOni.Data
{
    public abstract class WeaponTrait : EquipmentTraitSO
    {
        [Header("Common Weapon Properties")]
        public float attackSpeed; // e.g., 1.5 swings per second
        public float knockbackForce;
        public string comboAnimationTrigger; // Tells the animator which attack tree to use
        public override string GetDescription()
        {
            return $"AS: {attackSpeed} | KB: {knockbackForce}";
        }
    }

    [CreateAssetMenu(fileName = "New Melee Weapon Trait", menuName = "Project Oni/Traits/Melee Weapon")]
    public class MeleeWeaponTrait : WeaponTrait
    {
        [Header("Melee Properties")]
        public float swingRadius;
        public int comboHitsAllowed;
        public GameObject hitSparkPrefab;

        public override string GetDescription()
        {
            return base.GetDescription() + $"\nRange: {swingRadius} | Combo: {comboHitsAllowed}";
        }
    }

    [CreateAssetMenu(fileName = "New Ranged Weapon Trait", menuName = "Project Oni/Traits/Ranged Weapon")]
    public class RangedWeaponTrait : WeaponTrait
    {
        [Header("Ranged Properties")]
        public GameObject projectilePrefab;
        public float maxDrawTime;
        public float projectileSpeed;
        public int ammoCost;

        public override string GetDescription()
        {
            return base.GetDescription() + $"\nProj Speed: {projectileSpeed} | Ammo: {ammoCost}";
        }
    }
}
