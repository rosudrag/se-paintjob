using System.Collections.Generic;
using PaintJob.App.Analysis;
using PaintJob.App.PaintAlgorithms.Common.Painters;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms.Military.Painters
{
    /// <summary>
    /// Applies base colors to blocks based on their layer and function.
    /// </summary>
    public class BaseColorPainter : IBlockPainter
    {
        public string Name => "Base Color";
        public int Priority => 100; // Base priority

        public void ApplyColors(MyCubeGrid grid, Dictionary<Vector3I, int> colorResults, PaintContext context)
        {
            foreach (var block in context.Blocks)
            {
                if (!context.SpatialData.TryGetValue(block, out var spatialInfo))
                    continue;

                var baseColor = DetermineBaseColor(block, spatialInfo, context);
                ApplyColorToBlock(block, baseColor, colorResults);
            }
        }

        private MilitaryColorScheme.ColorIndex DetermineBaseColor(
            MySlimBlock block,
            BlockSpatialAnalyzer.BlockSpatialInfo spatialInfo,
            PaintContext context)
        {
            // First, check layer-based coloring
            var militaryColorScheme = context.ColorScheme as MilitaryColorScheme;
            if (militaryColorScheme == null)
                return MilitaryColorScheme.ColorIndex.PrimaryHull;
                
            var layerColor = militaryColorScheme.GetLayerColor(spatialInfo.PrimaryLayer);

            // Override for functional blocks on exterior
            if (spatialInfo.PrimaryLayer == BlockSpatialAnalyzer.LayerType.Exterior)
            {
                if (context.Functional.BlockToCluster.TryGetValue(block, out var clusterId))
                {
                    var cluster = GetClusterById(context.Functional, clusterId);
                    if (cluster != null)
                    {
                        var functionalColor = militaryColorScheme.GetSystemColor(
                            cluster.SystemType,
                            cluster.IsPrimary
                        );

                        // Weapons and sensors always override base color
                        if (IsHighPrioritySystem(cluster.SystemType))
                        {
                            return functionalColor;
                        }
                    }
                }
            }

            return layerColor;
        }

        private static FunctionalClusterAnalyzer.FunctionalCluster GetClusterById(
            FunctionalClusterAnalyzer.ClusterAnalysis analysis,
            int clusterId)
        {
            foreach (var cluster in analysis.Clusters)
            {
                if (cluster.Id == clusterId)
                    return cluster;
            }
            return null;
        }

        private static bool IsHighPrioritySystem(FunctionalClusterAnalyzer.FunctionalSystemType systemType)
        {
            return systemType == FunctionalClusterAnalyzer.FunctionalSystemType.OffensiveWeapons ||
                   systemType == FunctionalClusterAnalyzer.FunctionalSystemType.DefensiveWeapons ||
                   systemType == FunctionalClusterAnalyzer.FunctionalSystemType.Sensors ||
                   systemType == FunctionalClusterAnalyzer.FunctionalSystemType.Communication;
        }

        private static void ApplyColorToBlock(MySlimBlock block, MilitaryColorScheme.ColorIndex color, Dictionary<Vector3I, int> colorResults)
        {
            var positions = GetBlockPositions(block);
            foreach (var pos in positions)
            {
                colorResults[pos] = (int)color;
            }
        }

        private static IEnumerable<Vector3I> GetBlockPositions(MySlimBlock block)
        {
            for (var x = block.Min.X; x <= block.Max.X; x++)
            {
                for (var y = block.Min.Y; y <= block.Max.Y; y++)
                {
                    for (var z = block.Min.Z; z <= block.Max.Z; z++)
                    {
                        yield return new Vector3I(x, y, z);
                    }
                }
            }
        }
    }
}