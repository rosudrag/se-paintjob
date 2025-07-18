using System;
using System.Collections.Generic;
using System.Linq;
using PaintJob.App.Analysis;
using PaintJob.App.Extensions;
using PaintJob.App.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms
{
    public class MilitaryPaintJob : PaintAlgorithm
    {
        private Dictionary<Vector3I, int> _colorResult = new Dictionary<Vector3I, int>();
        private Vector3[] _colors;
        
        // Military color palette indices
        private const int DarkGreen = 0;      // Primary hull color
        private const int OliveGreen = 1;     // Secondary hull color  
        private const int DarkGrey = 2;       // Weapons and utility
        private const int Black = 3;          // Joints and recesses
        private const int DarkBrown = 4;      // Camouflage accent
        private const int SandBrown = 5;      // Desert variant
        private const int Warning = 6;        // Hazard stripes (yellow/orange)
        private const int NavRed = 7;         // Port navigation
        private const int NavGreen = 8;       // Starboard navigation
        private const int InteriorGrey = 9;  // Interior spaces
        private const int FunctionalDark = 10; // Dark functional blocks
        private const int FunctionalLight = 11; // Light functional blocks
        
        // Analysis systems
        private readonly ShipGeometryAnalyzer _geometryAnalyzer = new ShipGeometryAnalyzer();
        private readonly BlockSpatialAnalyzer _spatialAnalyzer = new BlockSpatialAnalyzer();
        private readonly SurfaceAnalyzer _surfaceAnalyzer = new SurfaceAnalyzer();
        private readonly FunctionalClusterAnalyzer _functionalAnalyzer = new FunctionalClusterAnalyzer();
        private readonly SpatialOrientationAnalyzer _orientationAnalyzer = new SpatialOrientationAnalyzer();
        private readonly PatternGenerator _patternGenerator = new PatternGenerator();

        public override void Clean()
        {
            _colorResult.Clear();
        }

        protected override void Apply(MyCubeGrid grid)
        {
            // Analyze the ship
            var geometry = _geometryAnalyzer.AnalyzeGrid(grid);
            var spatial = _spatialAnalyzer.AnalyzeBlockSpatialData(grid);
            var surfaces = _surfaceAnalyzer.AnalyzeGrid(grid);
            var functional = _functionalAnalyzer.AnalyzeGrid(grid);
            var orientation = _orientationAnalyzer.AnalyzeGrid(grid);
            
            var blocks = grid.GetBlocks();
            
            // Phase 1: Base coloring by layer and function
            ApplyBaseColors(grid, blocks, spatial, functional);
            
            // Phase 2: Exterior camouflage pattern
            ApplyCamouflagePattern(grid, spatial, surfaces, geometry);
            
            // Phase 3: Functional system coloring
            ApplyFunctionalSystemColors(functional);
            
            // Phase 4: Navigation and warning markings
            ApplyNavigationMarkings(grid, orientation, functional);
            
            // Phase 5: Apply the colors to the grid
            foreach (var block in blocks)
            {
                if (_colorResult.TryGetValue(block.Position, out var colorIndex))
                {
                    grid.ColorBlocks(block.Min, block.Max, _colors[colorIndex], false);
                }
            }
        }

        private void ApplyBaseColors(
            MyCubeGrid grid, 
            HashSet<MySlimBlock> blocks,
            Dictionary<MySlimBlock, BlockSpatialAnalyzer.BlockSpatialInfo> spatial,
            FunctionalClusterAnalyzer.ClusterAnalysis functional)
        {
            foreach (var block in blocks)
            {
                if (!spatial.TryGetValue(block, out var spatialInfo))
                    continue;
                
                // Determine base color by layer
                var baseColor = DarkGreen;
                
                if (spatialInfo.PrimaryLayer == BlockSpatialAnalyzer.LayerType.Interior)
                {
                    baseColor = InteriorGrey;
                }
                else if (spatialInfo.PrimaryLayer == BlockSpatialAnalyzer.LayerType.Guts)
                {
                    baseColor = Black;
                }
                else if (spatialInfo.PrimaryLayer == BlockSpatialAnalyzer.LayerType.Exterior)
                {
                    // Check if it's a functional block
                    if (functional.BlockToCluster.TryGetValue(block, out var clusterId))
                    {
                        var cluster = functional.Clusters.First(c => c.Id == clusterId);
                        
                        // Weapons get dark grey
                        if (cluster.SystemType == FunctionalClusterAnalyzer.FunctionalSystemType.OffensiveWeapons ||
                            cluster.SystemType == FunctionalClusterAnalyzer.FunctionalSystemType.DefensiveWeapons)
                        {
                            baseColor = DarkGrey;
                        }
                        // Sensors and comms get dark colors for stealth
                        else if (cluster.SystemType == FunctionalClusterAnalyzer.FunctionalSystemType.Sensors ||
                                 cluster.SystemType == FunctionalClusterAnalyzer.FunctionalSystemType.Communication)
                        {
                            baseColor = Black;
                        }
                    }
                }
                
                // Apply to all positions occupied by the block
                for (var x = block.Min.X; x <= block.Max.X; x++)
                {
                    for (var y = block.Min.Y; y <= block.Max.Y; y++)
                    {
                        for (var z = block.Min.Z; z <= block.Max.Z; z++)
                        {
                            _colorResult[new Vector3I(x, y, z)] = baseColor;
                        }
                    }
                }
            }
        }

        private void ApplyCamouflagePattern(
            MyCubeGrid grid,
            Dictionary<MySlimBlock, BlockSpatialAnalyzer.BlockSpatialInfo> spatial,
            SurfaceAnalyzer.SurfaceAnalysis surfaces,
            ShipGeometryAnalyzer.GeometryAnalysis geometry)
        {
            // Get exterior positions
            var exteriorPositions = _spatialAnalyzer.GetBlocksByLayer(spatial, BlockSpatialAnalyzer.LayerType.Exterior);
            
            // Determine camouflage type based on ship profile
            PatternGenerator.PatternType patternType;
            float scale;
            
            switch (geometry.Profile)
            {
                case ShipGeometryAnalyzer.ShipProfile.Wedge:
                case ShipGeometryAnalyzer.ShipProfile.Flat:
                    // Angular ships get geometric camo
                    patternType = PatternGenerator.PatternType.Hexagonal;
                    scale = 3.0f;
                    break;
                    
                case ShipGeometryAnalyzer.ShipProfile.Cylindrical:
                case ShipGeometryAnalyzer.ShipProfile.Elongated:
                    // Smooth ships get organic camo
                    patternType = PatternGenerator.PatternType.Noise;
                    scale = 2.0f;
                    break;
                    
                default:
                    // Default digital camo
                    patternType = PatternGenerator.PatternType.Checkerboard;
                    scale = 2.0f;
                    break;
            }
            
            // Generate camouflage pattern
            var camoParams = new PatternGenerator.PatternParameters
            {
                Type = patternType,
                Origin = geometry.GeometricCenter,
                Scale = scale,
                ColorIndices = new[] { DarkGreen, OliveGreen, DarkBrown },
                Frequency = 1.5f
            };
            
            var camoPattern = _patternGenerator.GeneratePattern(
                grid,
                exteriorPositions.Select(p => grid.GetCubeBlock(p)).Where(b => b != null).ToHashSet(),
                camoParams
            );
            
            // Apply camouflage only to armor blocks
            foreach (var kvp in camoPattern)
            {
                var block = grid.GetCubeBlock(kvp.Key);
                if (block != null && block.FatBlock == null && exteriorPositions.Contains(kvp.Key))
                {
                    _colorResult[kvp.Key] = kvp.Value;
                }
            }
            
            // Add wear and weathering to edges
            foreach (var edge in surfaces.Edges)
            {
                foreach (var pos in edge.Positions)
                {
                    if (_colorResult.ContainsKey(pos))
                    {
                        // Darken edges for worn look
                        _colorResult[pos] = Black;
                    }
                }
            }
        }

        private void ApplyFunctionalSystemColors(FunctionalClusterAnalyzer.ClusterAnalysis functional)
        {
            foreach (var cluster in functional.Clusters)
            {
                var color = DetermineClusterColor(cluster);
                
                foreach (var block in cluster.Blocks)
                {
                    // Only color if not already colored by more important system
                    for (var x = block.Min.X; x <= block.Max.X; x++)
                    {
                        for (var y = block.Min.Y; y <= block.Max.Y; y++)
                        {
                            for (var z = block.Min.Z; z <= block.Max.Z; z++)
                            {
                                var pos = new Vector3I(x, y, z);
                                
                                // Weapons and critical systems override camo
                                if (cluster.Importance > 0.7f || 
                                    cluster.SystemType == FunctionalClusterAnalyzer.FunctionalSystemType.OffensiveWeapons ||
                                    cluster.SystemType == FunctionalClusterAnalyzer.FunctionalSystemType.DefensiveWeapons)
                                {
                                    _colorResult[pos] = color;
                                }
                            }
                        }
                    }
                }
            }
        }

        private int DetermineClusterColor(FunctionalClusterAnalyzer.FunctionalCluster cluster)
        {
            switch (cluster.SystemType)
            {
                case FunctionalClusterAnalyzer.FunctionalSystemType.OffensiveWeapons:
                case FunctionalClusterAnalyzer.FunctionalSystemType.DefensiveWeapons:
                    return DarkGrey;
                    
                case FunctionalClusterAnalyzer.FunctionalSystemType.PowerGeneration:
                case FunctionalClusterAnalyzer.FunctionalSystemType.PowerStorage:
                    return cluster.IsPrimary ? Warning : FunctionalDark;
                    
                case FunctionalClusterAnalyzer.FunctionalSystemType.MainThrusters:
                case FunctionalClusterAnalyzer.FunctionalSystemType.ManeuveringThrusters:
                    return Black;
                    
                case FunctionalClusterAnalyzer.FunctionalSystemType.CommandAndControl:
                    return InteriorGrey;
                    
                case FunctionalClusterAnalyzer.FunctionalSystemType.CargoStorage:
                case FunctionalClusterAnalyzer.FunctionalSystemType.FluidStorage:
                    return SandBrown;
                    
                case FunctionalClusterAnalyzer.FunctionalSystemType.Medical:
                    return InteriorGrey; // Would be white with red cross in reality
                    
                default:
                    return FunctionalDark;
            }
        }

        private void ApplyNavigationMarkings(
            MyCubeGrid grid,
            SpatialOrientationAnalyzer.OrientationAnalysis orientation,
            FunctionalClusterAnalyzer.ClusterAnalysis functional)
        {
            // Navigation lights
            var lightClusters = functional.Clusters.Where(c => 
                c.SystemType == FunctionalClusterAnalyzer.FunctionalSystemType.Lighting);
            
            foreach (var cluster in lightClusters)
            {
                foreach (var block in cluster.Blocks)
                {
                    if (_orientationAnalyzer.IsPortSide(block, orientation))
                    {
                        _colorResult[block.Position] = NavRed;
                    }
                    else if (_orientationAnalyzer.IsStarboardSide(block, orientation))
                    {
                        _colorResult[block.Position] = NavGreen;
                    }
                }
            }
            
            // Hazard stripes on dangerous areas
            var dangerousClusters = functional.Clusters.Where(c =>
                c.SystemType == FunctionalClusterAnalyzer.FunctionalSystemType.OffensiveWeapons ||
                c.SystemType == FunctionalClusterAnalyzer.FunctionalSystemType.MainThrusters);
            
            foreach (var cluster in dangerousClusters)
            {
                // Add warning stripes around weapons and thrusters
                var positions = cluster.Blocks.SelectMany(b =>
                {
                    var posList = new List<Vector3I>();
                    for (var x = b.Min.X; x <= b.Max.X; x++)
                        for (var y = b.Min.Y; y <= b.Max.Y; y++)
                            for (var z = b.Min.Z; z <= b.Max.Z; z++)
                                posList.Add(new Vector3I(x, y, z));
                    return posList;
                }).ToHashSet();
                
                // Find adjacent blocks for warning stripes
                foreach (var pos in positions)
                {
                    var directions = new[]
                    {
                        Vector3I.Up, Vector3I.Down, Vector3I.Left,
                        Vector3I.Right, Vector3I.Forward, Vector3I.Backward
                    };
                    
                    foreach (var dir in directions)
                    {
                        var adjacentPos = pos + dir;
                        var adjacentBlock = grid.GetCubeBlock(adjacentPos);
                        
                        if (adjacentBlock != null && adjacentBlock.FatBlock == null)
                        {
                            // Apply warning color to adjacent armor blocks
                            if (!positions.Contains(adjacentPos))
                            {
                                _colorResult[adjacentPos] = Warning;
                            }
                        }
                    }
                }
            }
        }

        protected override void GeneratePalette(MyCubeGrid grid)
        {
            // Use ColorSchemeGenerator to create dynamic military palette
            var colorGenerator = new ColorSchemeGenerator();
            var generatedColors = colorGenerator.GenerateFactionPalette("military");
            
            // Map generated colors to our specific needs
            var militaryColors = new List<Vector3>();
            
            // Core military colors (from generated palette)
            if (generatedColors.Length >= 5)
            {
                militaryColors.Add(generatedColors[0]); // 0: Dark Green (primary)
                militaryColors.Add(generatedColors[1]); // 1: Olive Green (secondary)
                militaryColors.Add(ColorMaskExtensions.CreateGreyMask(30)); // 2: Dark Grey
                militaryColors.Add(ColorMaskExtensions.CreateGreyMask(5));  // 3: Black
                militaryColors.Add(generatedColors[2]); // 4: Dark Brown (camo)
                militaryColors.Add(generatedColors[3]); // 5: Sand Brown (camo)
            }
            else
            {
                // Fallback if generation fails
                militaryColors.Add(ColorMaskExtensions.CreateColorMask(120, 80, 30)); // Dark Green
                militaryColors.Add(ColorMaskExtensions.CreateColorMask(80, 60, 40));  // Olive Green
                militaryColors.Add(ColorMaskExtensions.CreateGreyMask(30)); // Dark Grey
                militaryColors.Add(ColorMaskExtensions.CreateGreyMask(5));  // Black
                militaryColors.Add(ColorMaskExtensions.CreateColorMask(30, 70, 25));  // Dark Brown
                militaryColors.Add(ColorMaskExtensions.CreateColorMask(40, 35, 76));  // Sand Brown
            }
            
            // Special purpose colors (always the same)
            militaryColors.Add(ColorMaskExtensions.CreateColorMask(33, 100, 100)); // 6: Warning Orange
            militaryColors.Add(ColorMaskExtensions.CreateColorMask(0, 100, 80));   // 7: Navigation Red
            militaryColors.Add(ColorMaskExtensions.CreateColorMask(120, 100, 80)); // 8: Navigation Green
            militaryColors.Add(ColorMaskExtensions.CreateGreyMask(50));            // 9: Interior Grey
            militaryColors.Add(ColorMaskExtensions.CreateGreyMask(15));            // 10: Functional Dark
            militaryColors.Add(ColorMaskExtensions.CreateGreyMask(40));            // 11: Functional Light
            
            // Add remaining generated colors or variations
            if (generatedColors.Length > 5)
            {
                for (var i = 5; i < generatedColors.Length && militaryColors.Count < 14; i++)
                {
                    militaryColors.Add(generatedColors[i]);
                }
            }
            
            // Fill to 14 colors if needed
            while (militaryColors.Count < 14)
            {
                // Add variations of existing colors
                militaryColors.Add(ColorMaskExtensions.CreateColorMask(120, 90, 15)); // Very Dark Green
                militaryColors.Add(ColorMaskExtensions.CreateColorMask(90, 40, 50));  // Light Olive
            }
            
            _colors = militaryColors.Take(14).ToArray();
        }
        
        public override void RunTest(MyCubeGrid grid, string[] args)
        {
            GeneratePalette(grid);
            
            if (args.Length == 0)
            {
                // Test all colors
                var blocks = grid.GetBlocks().ToList();
                for (var i = 0; i < blocks.Count && i < _colors.Length; i++)
                {
                    var block = blocks[i];
                    grid.ColorBlocks(block.Min, block.Max, _colors[i % _colors.Length], false);
                }
            }
            else
            {
                // Test specific color
                var colorNumber = int.Parse(args[0]);
                var color = _colors[Math.Min(colorNumber, _colors.Length - 1)];
                
                var blocks = grid.GetBlocks();
                foreach (var block in blocks)
                {
                    grid.ColorBlocks(block.Min, block.Max, color, false);
                }
            }
        }
    }
}