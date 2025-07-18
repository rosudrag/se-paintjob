using System.Collections.Generic;
using PaintJob.App.Analysis;
using PaintJob.App.PaintAlgorithms.Common.Painters;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms.Military.Painters
{
    /// <summary>
    /// Applies colors to functional systems (weapons, thrusters, etc).
    /// </summary>
    public class FunctionalSystemPainter : IBlockPainter
    {
        public string Name => "Functional Systems";
        public int Priority => 300; // Higher than camouflage

        public void ApplyColors(MyCubeGrid grid, Dictionary<Vector3I, int> colorResults, PaintContext context)
        {
            foreach (var cluster in context.Functional.Clusters)
            {
                // Only paint high-importance or critical systems
                if (!ShouldPaintCluster(cluster))
                    continue;

                var militaryColorScheme = context.ColorScheme as MilitaryColorScheme;
                if (militaryColorScheme == null)
                    continue;
                    
                var colorIndex = militaryColorScheme.GetSystemColor(cluster.SystemType, cluster.IsPrimary);
                
                foreach (var block in cluster.Blocks)
                {
                    ApplyColorToBlock(block, (int)colorIndex, colorResults);
                }
            }
        }

        private bool ShouldPaintCluster(FunctionalClusterAnalyzer.FunctionalCluster cluster)
        {
            // High importance clusters always get painted
            if (cluster.Importance > 0.7f)
                return true;

            // Critical system types always get painted
            return IsCriticalSystem(cluster.SystemType);
        }

        private bool IsCriticalSystem(FunctionalClusterAnalyzer.FunctionalSystemType systemType)
        {
            return systemType == FunctionalClusterAnalyzer.FunctionalSystemType.OffensiveWeapons ||
                   systemType == FunctionalClusterAnalyzer.FunctionalSystemType.DefensiveWeapons ||
                   systemType == FunctionalClusterAnalyzer.FunctionalSystemType.PowerGeneration ||
                   systemType == FunctionalClusterAnalyzer.FunctionalSystemType.MainThrusters;
        }

        private void ApplyColorToBlock(MySlimBlock block, int colorIndex, Dictionary<Vector3I, int> colorResults)
        {
            for (var x = block.Min.X; x <= block.Max.X; x++)
            {
                for (var y = block.Min.Y; y <= block.Max.Y; y++)
                {
                    for (var z = block.Min.Z; z <= block.Max.Z; z++)
                    {
                        colorResults[new Vector3I(x, y, z)] = colorIndex;
                    }
                }
            }
        }
    }
}