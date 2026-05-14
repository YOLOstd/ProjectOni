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
        public string hitSoundEffect;
    }

    [CreateAssetMenu(fileName = "New Melee Weapon Trait", menuName = "Project Oni/Traits/Melee Weapon")]
    public class MeleeWeaponTrait : WeaponTrait
    {
        [Header("Melee Properties")]
        public float swingRadius;
        public int comboHitsAllowed;
        public GameObject hitSparkPrefab;
    }

    [CreateAssetMenu(fileName = "New Ranged Weapon Trait", menuName = "Project Oni/Traits/Ranged Weapon")]
    public class RangedWeaponTrait : WeaponTrait
    {
        [Header("Ranged Properties")]
        public GameObject projectilePrefab;
        public float maxDrawTime;
        public float projectileSpeed;
        public int ammoCost;
    }
}
