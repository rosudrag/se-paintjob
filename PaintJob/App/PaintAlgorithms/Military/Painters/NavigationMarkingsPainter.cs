using System.Collections.Generic;
using System.Linq;
using PaintJob.App.Analysis;
using PaintJob.App.PaintAlgorithms.Common.Painters;
using Sandbox.Game.Entities;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms.Military.Painters
{
    /// <summary>
    /// Applies navigation lights and hazard markings.
    /// </summary>
    public class NavigationMarkingsPainter : IBlockPainter
    {
        private readonly SpatialOrientationAnalyzer _orientationAnalyzer;

        public string Name => "Navigation Markings";
        public int Priority => 400; // Highest priority

        public NavigationMarkingsPainter(SpatialOrientationAnalyzer orientationAnalyzer)
        {
            _orientationAnalyzer = orientationAnalyzer;
        }

        public void ApplyColors(MyCubeGrid grid, Dictionary<Vector3I, int> colorResults, PaintContext context)
        {
            ApplyNavigationLights(context, colorResults);
            ApplyHazardMarkings(grid, context, colorResults);
        }

        private void ApplyNavigationLights(PaintContext context, Dictionary<Vector3I, int> colorResults)
        {
            var lightClusters = context.Functional.Clusters
                .Where(c => c.SystemType == FunctionalClusterAnalyzer.FunctionalSystemType.Lighting);

            foreach (var cluster in lightClusters)
            {
                foreach (var block in cluster.Blocks)
                {
                    MilitaryColorScheme.ColorIndex navColor;
                    
                    if (_orientationAnalyzer.IsPortSide(block, context.Orientation))
                    {
                        navColor = MilitaryColorScheme.ColorIndex.NavigationPort;
                    }
                    else if (_orientationAnalyzer.IsStarboardSide(block, context.Orientation))
                    {
                        navColor = MilitaryColorScheme.ColorIndex.NavigationStarboard;
                    }
                    else
                    {
                        continue; // Skip non-navigation lights
                    }

                    colorResults[block.Position] = (int)navColor;
                }
            }
        }

        private void ApplyHazardMarkings(MyCubeGrid grid, PaintContext context, Dictionary<Vector3I, int> colorResults)
        {
            var dangerousClusters = context.Functional.Clusters
                .Where(c => IsDangerousSystem(c.SystemType));

            var warningColor = (int)MilitaryColorScheme.ColorIndex.HazardWarning;
            var processedPositions = new HashSet<Vector3I>();

            foreach (var cluster in dangerousClusters)
            {
                // Get all positions occupied by dangerous blocks
                var dangerousPositions = GetClusterPositions(cluster);
                
                // Find adjacent armor blocks for warning stripes
                foreach (var pos in dangerousPositions)
                {
                    ApplyWarningToAdjacentBlocks(grid, pos, dangerousPositions, warningColor, colorResults, processedPositions);
                }
            }
        }

        private bool IsDangerousSystem(FunctionalClusterAnalyzer.FunctionalSystemType systemType)
        {
            return systemType == FunctionalClusterAnalyzer.FunctionalSystemType.OffensiveWeapons ||
                   systemType == FunctionalClusterAnalyzer.FunctionalSystemType.MainThrusters ||
                   systemType == FunctionalClusterAnalyzer.FunctionalSystemType.PowerGeneration;
        }

        private HashSet<Vector3I> GetClusterPositions(FunctionalClusterAnalyzer.FunctionalCluster cluster)
        {
            var positions = new HashSet<Vector3I>();
            
            foreach (var block in cluster.Blocks)
            {
                for (var x = block.Min.X; x <= block.Max.X; x++)
                {
                    for (var y = block.Min.Y; y <= block.Max.Y; y++)
                    {
                        for (var z = block.Min.Z; z <= block.Max.Z; z++)
                        {
                            positions.Add(new Vector3I(x, y, z));
                        }
                    }
                }
            }

            return positions;
        }

        private void ApplyWarningToAdjacentBlocks(
            MyCubeGrid grid,
            Vector3I pos,
            HashSet<Vector3I> dangerousPositions,
            int warningColor,
            Dictionary<Vector3I, int> colorResults,
            HashSet<Vector3I> processedPositions)
        {
            var directions = new[]
            {
                Vector3I.Up, Vector3I.Down,
                Vector3I.Left, Vector3I.Right,
                Vector3I.Forward, Vector3I.Backward
            };

            foreach (var dir in directions)
            {
                var adjacentPos = pos + dir;
                
                // Skip if already processed or is a dangerous position
                if (processedPositions.Contains(adjacentPos) || dangerousPositions.Contains(adjacentPos))
                    continue;

                var adjacentBlock = grid.GetCubeBlock(adjacentPos);
                
                // Only apply to armor blocks
                if (adjacentBlock != null && adjacentBlock.FatBlock == null)
                {
                    colorResults[adjacentPos] = warningColor;
                    processedPositions.Add(adjacentPos);
                }
            }
        }
    }
}