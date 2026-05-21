using System.Collections.Generic;
using UnityEngine;

namespace ProjectOni.Data
{
    [CreateAssetMenu(fileName = "New Attribute Conversion Trait", menuName = "Project Oni/Traits/Attribute Conversion")]
    public class AttributeConversionTrait : EquipmentTraitSO
    {
        public List<AttributeConversion> conversions = new();

        public override string GetDescription()
        {
            if (conversions == null || conversions.Count == 0) return "No conversions.";
            
            var lines = new List<string>();
            foreach (var conv in conversions)
            {
                string modStr = conv.targetModType switch {
                    ModType.Flat => "Flat",
                    ModType.Increased => "% Increased",
                    ModType.More => "% More",
                    _ => ""
                };

                if (conv.mode == ConversionMode.Stepped)
                {
                    lines.Add($"Every {conv.stepSize} {conv.sourceStat} gives {conv.valuePerStep} {modStr} {conv.targetStat}");
                }
                else
                {
                    lines.Add($"Gain {conv.conversionRate * 100}% of {conv.sourceStat} as {modStr} {conv.targetStat}");
                }
            }
            return string.Join("\n", lines);
        }
    }
}
