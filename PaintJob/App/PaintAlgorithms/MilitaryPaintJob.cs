using System;
using System.Collections.Generic;
using System.Linq;
using PaintJob.App.PaintAlgorithms.Common;
using PaintJob.App.PaintAlgorithms.Common.Painters;
using PaintJob.App.PaintAlgorithms.Common.Patterns;
using PaintJob.App.PaintAlgorithms.Military;
using PaintJob.App.PaintAlgorithms.Military.Analyzers;
using PaintJob.App.PaintAlgorithms.Military.Camouflage;
using PaintJob.App.PaintAlgorithms.Military.Painters;
using PaintJob.App.Utils;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms
{
    /// <summary>
    /// Military-themed paint job with camouflage patterns and functional coloring.
    /// </summary>
    public class MilitaryPaintJob : PaintAlgorithm
    {
        private readonly IAnalyzerFactory _analyzerFactory;
        private readonly CamouflageFactory _camouflageFactory;
        private readonly List<IBlockPainter> _painters;
        private readonly MilitaryColorScheme _colorScheme;
        private readonly Dictionary<Vector3I, int> _colorResults;
        
        private Vector3[] _colorPalette;

        public MilitaryPaintJob()
        {
            _analyzerFactory = new MilitaryAnalyzerFactory();
            _camouflageFactory = new CamouflageFactory();
            _colorScheme = new MilitaryColorScheme();
            _colorResults = new Dictionary<Vector3I, int>();
            _painters = InitializeDefaultPainters();
        }
        
        public override void Clean()
        {
            _colorResults.Clear();
            _colorPalette = null;
        }

        protected override void Apply(MyCubeGrid grid)
        {
            try
            {
                ValidateGrid(grid);
                
                // Perform analysis
                var context = PerformAnalysis(grid);
                
                // Apply painters in priority order
                ApplyPainters(grid, context);
                
                // Apply final colors to grid
                ApplyColorsToGrid(grid);
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply military paint job: {ex.Message}");
                throw new InvalidOperationException("Military paint job application failed", ex);
            }
        }

        private void ValidateGrid(MyCubeGrid grid)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            
            if (grid.Physics == null)
            {
                LogWarning("Grid has no physics, paint job may not apply correctly");
            }
            
            var blocks = grid.GetBlocks();
            if (!blocks.Any())
            {
                throw new InvalidOperationException("Grid has no blocks to paint");
            }
        }

        private PaintContext PerformAnalysis(MyCubeGrid grid)
        {
            try
            {
                LogInfo("Performing grid analysis...");
                
                var geometryAnalyzer = _analyzerFactory.CreateGeometryAnalyzer();
                var spatialAnalyzer = _analyzerFactory.CreateSpatialAnalyzer();
                var surfaceAnalyzer = _analyzerFactory.CreateSurfaceAnalyzer();
                var functionalAnalyzer = _analyzerFactory.CreateFunctionalAnalyzer();
                var orientationAnalyzer = _analyzerFactory.CreateOrientationAnalyzer();

                var context = new PaintContext
                {
                    ColorScheme = _colorScheme,
                    Geometry = geometryAnalyzer.AnalyzeGrid(grid),
                    SpatialData = spatialAnalyzer.AnalyzeBlockSpatialData(grid),
                    Surfaces = surfaceAnalyzer.AnalyzeGrid(grid),
                    Functional = functionalAnalyzer.AnalyzeGrid(grid),
                    Orientation = orientationAnalyzer.AnalyzeGrid(grid),
                    Blocks = grid.GetBlocks()
                };

                LogInfo($"Analysis complete. Found {context.Blocks.Count} blocks, {context.Functional.Clusters.Count} functional clusters");
                return context;
            }
            catch (Exception ex)
            {
                LogError($"Analysis failed: {ex.Message}");
                throw new InvalidOperationException("Failed to analyze grid", ex);
            }
        }

        private void ApplyPainters(MyCubeGrid grid, PaintContext context)
        {
            // Sort painters by priority
            var sortedPainters = _painters.OrderBy(p => p.Priority).ToList();
            
            foreach (var painter in sortedPainters)
            {
                try
                {
                    LogInfo($"Applying {painter.Name} painter...");
                    painter.ApplyColors(grid, _colorResults, context);
                }
                catch (Exception ex)
                {
                    LogError($"Painter {painter.Name} failed: {ex.Message}");
                    // Continue with other painters
                }
            }
        }

        private void ApplyColorsToGrid(MyCubeGrid grid)
        {
            if (_colorPalette == null || _colorPalette.Length == 0)
            {
                throw new InvalidOperationException("Color palette not initialized");
            }

            var appliedCount = 0;
            var blocks = grid.GetBlocks();
            
            foreach (var block in blocks)
            {
                if (_colorResults.TryGetValue(block.Position, out var colorIndex))
                {
                    if (colorIndex >= 0 && colorIndex < _colorPalette.Length)
                    {
                        grid.ColorBlocks(block.Min, block.Max, _colorPalette[colorIndex], false);
                        appliedCount++;
                    }
                    else
                    {
                        LogWarning($"Invalid color index {colorIndex} for block at {block.Position}");
                    }
                }
            }
            
            LogInfo($"Applied colors to {appliedCount} blocks");
        }

        protected override void GeneratePalette(MyCubeGrid grid)
        {
            try
            {
                // Use ColorSchemeGenerator for dynamic palette
                var colorGenerator = new ColorSchemeGenerator();
                var generatedColors = colorGenerator.GenerateFactionPalette("military");
                
                // Initialize color scheme with generated colors
                _colorScheme.InitializePalette(generatedColors);
                _colorPalette = _colorScheme.ColorPalette;
                
                if (_colorPalette == null || _colorPalette.Length == 0)
                {
                    throw new InvalidOperationException("Failed to generate color palette");
                }
                
                LogInfo($"Generated palette with {_colorPalette.Length} colors");
            }
            catch (Exception ex)
            {
                LogError($"Palette generation failed: {ex.Message}");
                // Use fallback palette
                _colorScheme.InitializePalette(null);
                _colorPalette = _colorScheme.ColorPalette;
            }
        }

        public override void RunTest(MyCubeGrid grid, string[] args)
        {
            GeneratePalette(grid);
            
            if (args.Length == 0)
            {
                // Test all colors
                TestAllColors(grid);
            }
            else if (int.TryParse(args[0], out var colorNumber))
            {
                // Test specific color
                TestSpecificColor(grid, colorNumber);
            }
            else if (args[0].Equals("pattern", StringComparison.OrdinalIgnoreCase))
            {
                // Test camouflage pattern
                TestCamouflagePattern(grid, args.Skip(1).ToArray());
            }
        }

        private void TestAllColors(MyCubeGrid grid)
        {
            var blocks = grid.GetBlocks().ToList();
            for (var i = 0; i < blocks.Count && i < _colorPalette.Length; i++)
            {
                var block = blocks[i];
                grid.ColorBlocks(block.Min, block.Max, _colorPalette[i % _colorPalette.Length], false);
            }
        }

        private void TestSpecificColor(MyCubeGrid grid, int colorNumber)
        {
            var colorIndex = Math.Min(Math.Max(0, colorNumber), _colorPalette.Length - 1);
            var color = _colorPalette[colorIndex];
            
            foreach (var block in grid.GetBlocks())
            {
                grid.ColorBlocks(block.Min, block.Max, color, false);
            }
        }

        private void TestCamouflagePattern(MyCubeGrid grid, string[] patternArgs)
        {
            var patternName = patternArgs.Length > 0 ? patternArgs[0] : "digital";
            
            try
            {
                var strategy = _camouflageFactory.GetStrategy(patternName);
                var parameters = new PatternParameters
                {
                    Origin = grid.WorldMatrix.Translation,
                    Scale = 2.0f,
                    Frequency = 1.5f
                };
                
                var positions = grid.GetBlocks().Select(b => b.Position).ToList();
                var colorIndices = new[] { 0, 1, 4 }; // Primary, secondary, accent
                
                var pattern = strategy.GeneratePattern(grid, positions, colorIndices, parameters);
                
                foreach (var kvp in pattern)
                {
                    var block = grid.GetCubeBlock(kvp.Key);
                    if (block != null)
                    {
                        grid.ColorBlocks(block.Min, block.Max, _colorPalette[kvp.Value], false);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Pattern test failed: {ex.Message}");
            }
        }

        private List<IBlockPainter> InitializeDefaultPainters()
        {
            var spatialAnalyzer = _analyzerFactory.CreateSpatialAnalyzer();
            var orientationAnalyzer = _analyzerFactory.CreateOrientationAnalyzer();
            
            return new List<IBlockPainter>
            {
                new BaseColorPainter(),
                new CamouflagePainter(_camouflageFactory, spatialAnalyzer),
                new FunctionalSystemPainter(),
                new NavigationMarkingsPainter(orientationAnalyzer)
            };
        }

        private void LogInfo(string message)
        {
            MyAPIGateway.Utilities.ShowMessage("MilitaryPaintJob", message);
        }

        private void LogWarning(string message)
        {
            MyAPIGateway.Utilities.ShowMessage("MilitaryPaintJob", $"WARNING: {message}");
        }

        private void LogError(string message)
        {
            MyAPIGateway.Utilities.ShowMessage("MilitaryPaintJob", $"ERROR: {message}");
        }
    }
}