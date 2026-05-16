using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOni.Data
{
    public enum ConversionMode
    {
        Stepped,
        Ratio
    }

    [Serializable]
    public struct AttributeConversion
    {
        public StatType sourceStat;
        public StatType targetStat;
        public ModType targetModType;
        public ConversionMode mode;

        [Header("Stepped Mode")]
        public int stepSize;
        public float valuePerStep;

        [Header("Ratio Mode")]
        public float conversionRate;
    }

    /// <summary>
    /// Per-entity ScriptableObject holding base stat values and per-level growth.
    /// Global conversion rules live in AttributeSettingsSO, not here.
    /// </summary>
    [CreateAssetMenu(fileName = "New Base Stats", menuName = "Project Oni/Stats/Base Stats")]
    public class BaseStatsSO : ScriptableObject
    {
        [Header("Starting Stats & Growth")]
        public List<BaseStat> stats = new();
    }
}
