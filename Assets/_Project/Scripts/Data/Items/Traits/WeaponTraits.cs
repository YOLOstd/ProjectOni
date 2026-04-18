using System;
using UnityEngine;

namespace ProjectOni.Data
{
    [Serializable]
    public abstract class WeaponTrait : IEquipmentTrait
    {
        [Header("Common Weapon Properties")]
        public float attackSpeed; // e.g., 1.5 swings per second
        public float knockbackForce;
        public string comboAnimationTrigger; // Tells the animator which attack tree to use
        public string hitSoundEffect;
    }

    [Serializable]
    public class MeleeWeaponTrait : WeaponTrait
    {
        [Header("Melee Properties")]
        public float swingRadius;
        public int comboHitsAllowed;
        public GameObject hitSparkPrefab;
    }

    [Serializable]
    public class RangedWeaponTrait : WeaponTrait
    {
        [Header("Ranged Properties")]
        public GameObject projectilePrefab;
        public float maxDrawTime;
        public float projectileSpeed;
        public int ammoCost;
    }
}
