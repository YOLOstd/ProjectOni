using ProjectOni.Data;

namespace ProjectOni.Core
{
    /// <summary>
    /// A single stat modifier. Tracks its Source so it can be cleanly removed
    /// when an item is unequipped (no need to keep manual references).
    /// </summary>
    /// A single stat modifier. Tracks its Source so it can be cleanly removed
    /// when an item is unequipped (no need to keep manual references).
    /// </summary>
    public class StatMod
    {
        public ModType Type   { get; }
        public float   Value  { get; }
        public object  Source { get; }

        public StatMod(ModType type, float value, object source = null)
        {
            Type   = type;
            Value  = value;
            Source = source;
        }
    }
}
