using System.Collections.Generic;
using System.Linq;
using PaintJob.App.Analysis;
using PaintJob.App.PaintAlgorithms.Common.Painters;
using PaintJob.App.PaintAlgorithms.Common.Patterns;
using PaintJob.App.PaintAlgorithms.Military.Camouflage;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms.Military.Painters
{
    /// <summary>
    /// Applies camouflage patterns to exterior armor blocks.
    /// </summary>
    public class CamouflagePainter : IBlockPainter
    {
        private readonly CamouflageFactory _camouflageFactory;
        private readonly BlockSpatialAnalyzer _spatialAnalyzer;

        public string Name => "Camouflage";
        public int Priority => 200; // Higher than base

        public CamouflagePainter(CamouflageFactory camouflageFactory, BlockSpatialAnalyzer spatialAnalyzer)
        {
            _camouflageFactory = camouflageFactory;
            _spatialAnalyzer = spatialAnalyzer;
        }

        public void ApplyColors(MyCubeGrid grid, Dictionary<Vector3I, int> colorResults, PaintContext context)
        {
            // Get exterior armor blocks
            var exteriorPositions = GetExteriorArmorPositions(grid, context);
            if (!exteriorPositions.Any())
                return;

            // Select camouflage strategy based on ship profile
            var strategy = _camouflageFactory.SelectStrategyForProfile(context.Geometry.Profile);
            
            // Configure camouflage parameters
            var parameters = CreateCamouflageParameters(context.Geometry);
            
            // Generate pattern
            var militaryColorScheme = context.ColorScheme as MilitaryColorScheme;
            if (militaryColorScheme == null)
                return;
                
            var colorIndices = militaryColorScheme.CamouflageColors
                .Select(c => (int)c)
                .ToArray();
            
            var pattern = strategy.GeneratePattern(grid, exteriorPositions, colorIndices, parameters);
            
            // Apply pattern (only to armor blocks)
            foreach (var kvp in pattern)
            {
                var block = grid.GetCubeBlock(kvp.Key);
                if (block != null && IsArmorBlock(block) && exteriorPositions.Contains(kvp.Key))
                {
                    colorResults[kvp.Key] = kvp.Value;
                }
            }

            // Apply weathering to edges
            ApplyEdgeWeathering(context, colorResults);
        }

        private HashSet<Vector3I> GetExteriorArmorPositions(MyCubeGrid grid, PaintContext context)
        {
            var exteriorBlocks = _spatialAnalyzer.GetBlocksByLayer(
                context.SpatialData,
                BlockSpatialAnalyzer.LayerType.Exterior
            );

            var positions = new HashSet<Vector3I>();
            
            foreach (var pos in exteriorBlocks)
            {
                var block = grid.GetCubeBlock(pos);
                if (block != null && IsArmorBlock(block))
                {
                    positions.Add(pos);
                }
            }

            return positions;
        }

        private static bool IsArmorBlock(MySlimBlock block)
        {
            // Armor blocks have no FatBlock (functional component)
            return block.FatBlock == null;
        }

        private PatternParameters CreateCamouflageParameters(ShipGeometryAnalyzer.GeometryAnalysis geometry)
        {
            var scale = DeterminePatternScale(geometry);
            
            return new PatternParameters
            {
                Origin = geometry.GeometricCenter,
                Scale = scale,
                Frequency = 1.5f,
                Seed = geometry.GetHashCode(), // Consistent seed based on geometry
                CustomParameters = new Dictionary<string, object>
                {
                    { "turbulence", 0.2f },
                    { "smoothing", 1 }
                }
            };
        }

        private float DeterminePatternScale(ShipGeometryAnalyzer.GeometryAnalysis geometry)
        {
            // Scale pattern based on ship size and profile
            var size = geometry.BoundingBox.Size.Length();
            
            switch (geometry.Profile)
            {
                case ShipGeometryAnalyzer.ShipProfile.Wedge:
                case ShipGeometryAnalyzer.ShipProfile.Flat:
                    return size * 0.1f;
                case ShipGeometryAnalyzer.ShipProfile.Cylindrical:
                case ShipGeometryAnalyzer.ShipProfile.Elongated:
                    return size * 0.08f;
                default:
                    return size * 0.09f;
            }
        }

        private void ApplyEdgeWeathering(PaintContext context, Dictionary<Vector3I, int> colorResults)
        {
            foreach (var edge in context.Surfaces.Edges)
            {
                foreach (var pos in edge.Positions)
                {
                    if (colorResults.ContainsKey(pos))
                    {
                        // Darken edges for worn appearance
                        colorResults[pos] = (int)MilitaryColorScheme.ColorIndex.TechnicalAreas;
                    }
                }
            }
        }
    }
}