namespace ProjectOni.Data
{
    public enum StatType
    {
        // Defense
        Health,
        Armor,
        Evasion,

        // Attack
        Attack,
        AttackSpeed,
        CritChance,
        CritDamage,
        Aoe,

        // Misc
        MoveSpeed,
        DodgePower,
        DodgeCooldown,

        // Attributes
        Strength,
        Dexterity,
        Intelligence
    }

    public enum ModType
    {
        Flat,
        Increased,
        More
    }
}
