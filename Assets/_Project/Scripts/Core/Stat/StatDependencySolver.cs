using System.Collections.Generic;
using UnityEngine;
using ProjectOni.Data;

namespace ProjectOni.Core
{
    public static class StatDependencySolver
    {
        /// <summary>
        /// Returns a list of stats in the exact order they MUST be calculated based on active conversions.
        /// Uses topological sort with cycle detection.
        /// </summary>
        public static List<StatType> GetCalculationOrder(List<AttributeConversion> activeConversions)
        {
            var graph = new Dictionary<StatType, HashSet<StatType>>();
            
            // 1. Initialize the graph for every stat in the game
            foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
            {
                graph[stat] = new HashSet<StatType>();
            }

            // 2. Build the edges (Who depends on who?)
            // Graph direction — Dependencies, not Flow: graph[targetStat].Add(sourceStat)
            foreach (var conv in activeConversions)
            {
                // The Target relies on the Source being calculated first
                graph[conv.targetStat].Add(conv.sourceStat);
            }

            var sortedOrder = new List<StatType>();
            var visited = new HashSet<StatType>();
            var inProgress = new HashSet<StatType>(); // Tracks current path to catch loops

            // 3. Recursive sorting function (Post-order DFS)
            void Visit(StatType node)
            {
                if (visited.Contains(node)) return;

                // CYCLE DETECTION (DESIGNER ERROR)
                if (inProgress.Contains(node))
                {
                    Debug.LogError($"[Stat System] DESIGNER ERROR: Cyclic dependency detected at '{node}'. " +
                                   "Two or more items are creating an impossible conversion loop. " +
                                   $"Stat '{node}' will use its unmodified base value. Redesign the offending items.");
                    return; 
                }

                inProgress.Add(node);

                // Dig down into dependencies first
                foreach (var dependency in graph[node])
                {
                    Visit(dependency);
                }

                inProgress.Remove(node);
                visited.Add(node);
                
                // Only add to the final list when all dependencies below it are resolved
                sortedOrder.Add(node); 
            }

            // 4. Run the sort for every stat
            foreach (var node in graph.Keys)
            {
                Visit(node);
            }

            return sortedOrder;
        }
    }
}
