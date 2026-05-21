using System.Collections.Generic;
using UnityEngine;

namespace ProjectOni.Data
{
    /// <summary>
    /// Global attribute conversion rules, shared across all entities.
    /// Defines how primary attributes (Str/Dex/Int) convert into other stats.
    /// </summary>
    [CreateAssetMenu(fileName = "StatSettings", menuName = "Project Oni/Stats/Stat Settings")]
    public class StatSettingsSO : ScriptableObject
    {
        [Header("Global Conversions")]
        public List<AttributeConversion> globalConversions = new();
    }
}
