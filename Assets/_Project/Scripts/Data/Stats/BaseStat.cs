using System;

namespace ProjectOni.Data
{
    [Serializable]
    public struct BaseStat
    {
        public StatType type;
        public float value;
        public float growthPerLevel;
        public bool isMultiplier;
    }
}
