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
            foreach (var conv in activeConversions)
            {
                // The Target relies on the Source being calculated first
                graph[conv.targetStat].Add(conv.sourceStat);
            }


            var sortedOrder = new List<StatType>();
            var visited = new HashSet<StatType>();
            var inProgress = new HashSet<StatType>(); 
            var path = new List<StatType>();

            // 3. Recursive sorting function (Post-order DFS)
            void Visit(StatType node)
            {
                if (visited.Contains(node)) return;

                if (inProgress.Contains(node))
                {
                    int startIndex = path.IndexOf(node);
                    string cycle = string.Join(" -> ", path.GetRange(startIndex, path.Count - startIndex)) + " -> " + node;
                    
                    Debug.LogError($"[Stat System] DESIGNER ERROR: Cyclic dependency detected! Chain: {cycle}. " +
                                   "Conversions cannot loop. This stat will use its base value only.");
                    return; 
                }

                inProgress.Add(node);
                path.Add(node);

                foreach (var dependency in graph[node])
                {
                    Visit(dependency);
                }

                path.RemoveAt(path.Count - 1);
                inProgress.Remove(node);
                visited.Add(node);
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
