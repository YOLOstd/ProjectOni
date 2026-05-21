using System;

namespace ProjectOni.Combat
{
    [Flags]
    public enum SkillTag
    {
        None = 0,
        Melee = 1 << 0,
        Ranged = 1 << 1,
        Spell = 1 << 2,
        Physical = 1 << 3,
        Magical = 1 << 4,
        Fire = 1 << 5,
        Ice = 1 << 6,
        Lightning = 1 << 7,
        Slash = 1 << 8,
        Thrust = 1 << 9
    }
}
