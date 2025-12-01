using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRageMath;
using VRage.Utils;
using PaintJob.App.Analysis;
using PaintJob.App.PaintAlgorithms.Common;
using PaintJob.App.PaintAlgorithms.Common.Painters;
using PaintJob.App.PaintAlgorithms.Military;
using PaintJob.App.PaintAlgorithms.Military.Analyzers;
using PaintJob.App.PaintAlgorithms.Military.Camouflage;
using PaintJob.App.PaintAlgorithms.Military.Painters;
using PaintJob.App.Skins;
using PaintJob.App.Skins.Painters;
using PaintJob.App.Utils;

namespace PaintJob.App.PaintAlgorithms
{
    /// <summary>
    /// Enhanced military paint job that supports both colors and skins
    /// </summary>
    public class EnhancedMilitaryPaintJob : SkinAwarePaintAlgorithm
    {
        private readonly IAnalyzerFactory _analyzerFactory;
        private readonly CamouflageFactory _camouflageFactory;
        private readonly List<IBlockPainter> _painters;
        private readonly MilitaryColorScheme _colorScheme;
        private readonly Dictionary<Vector3I, int> _colorResults;
        
        private Vector3[] _colorPalette;
        private string _variant = "standard";
        private bool _useAdaptiveCamo = true;
        
        public EnhancedMilitaryPaintJob() : base()
        {
            _analyzerFactory = new MilitaryAnalyzerFactory();
            _camouflageFactory = new CamouflageFactory();
            _colorScheme = new MilitaryColorScheme();
            _colorResults = new Dictionary<Vector3I, int>();
            _painters = InitializeDefaultPainters();
            
            // Set military theme for skins
            SkinTheme = "military";
            EnableSkins = true;
        }
        
        /// <summary>
        /// Sets the military variant for color and skin generation
        /// </summary>
        public void SetVariant(string variant, bool useAdaptiveCamo = true)
        {
            _variant = variant;
            _useAdaptiveCamo = useAdaptiveCamo;
            
            // Update skin theme based on variant
            switch (variant?.ToLower())
            {
                case "stealth":
                    SkinTheme = "stealth";
                    break;
                case "desert":
                    SkinTheme = "desert";
                    break;
                case "arctic":
                    SkinTheme = "arctic";
                    break;
                case "urban":
                    SkinTheme = "urban";
                    break;
                case "jungle":
                    SkinTheme = "jungle";
                    break;
                default:
                    SkinTheme = "military";
                    break;
            }
        }
        
        protected override void InitializeSkinPainters()
        {
            // Add military-specific skin painters
            var seed = _random?.Next() ?? 0;
            _skinPainters.Clear();
            _skinPainters.Add(new MilitarySkinPainter(_skinProvider, seed));
            _skinPainters.Add(new PatternSkinPainter(seed));
        }
        
        public override void Clean()
        {
            base.Clean();
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
                
                // Apply color painters
                ApplyPainters(grid, context);
                
                // Apply both colors and skins
                ApplyColorsAndSkins(grid, _colorResults, _colorPalette);
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply enhanced military paint job: {ex.Message}");
                throw new InvalidOperationException("Enhanced military paint job application failed", ex);
            }
        }
        
        private void ValidateGrid(MyCubeGrid grid)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            
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
                LogInfo("Performing grid analysis for enhanced military paint...");
                
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
                
                LogInfo($"Analysis complete. Found {context.Blocks.Count} blocks");
                return context;
            }
            catch (Exception ex)
            {
                LogError($"Analysis failed: {ex.Message}");
                throw;
            }
        }
        
        private void ApplyPainters(MyCubeGrid grid, PaintContext context)
        {
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
                }
            }
        }
        
        protected override void GenerateColorPalette(MyCubeGrid grid)
        {
            try
            {
                var seed = unchecked((int)grid.EntityId);
                var colorGenerator = new ColorSchemeGenerator(seed);
                
                // Generate military palette for the specified variant
                var generatedColors = colorGenerator.GenerateMilitaryPalette(_variant);
                
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
                _colorScheme.InitializePalette(null);
                _colorPalette = _colorScheme.ColorPalette;
            }
        }
        
        protected override void ApplySkins(MyCubeGrid grid)
        {
            if (!EnableSkins)
                return;
            
            LogInfo("Applying military skins...");
            
            // Generate context-aware skin selections
            if (_useAdaptiveCamo)
            {
                ApplyAdaptiveCamoSkins(grid);
            }
            
            // Apply standard skin patterns
            base.ApplySkins(grid);
        }
        
        private void ApplyAdaptiveCamoSkins(MyCubeGrid grid)
        {
            var blocks = grid.GetBlocks();
            var seed = unchecked((int)grid.EntityId);
            
            foreach (var block in blocks)
            {
                // Create skin selection context
                var context = new SkinSelectionContext
                {
                    Block = block,
                    Position = block.Position,
                    BlockSubtype = block.BlockDefinition?.Id.SubtypeId.String ?? "",
                    ColorIndex = _colorResults.ContainsKey(block.Position) ? _colorResults[block.Position] : (int?)null,
                    PatternType = DeterminePatternType(block),
                    Zone = DetermineZone(block),
                    Seed = seed
                };
                
                // Select appropriate skin
                var selectedSkin = _skinProvider.SelectSkin(context);
                
                if (selectedSkin != MyStringHash.NullOrEmpty)
                {
                    _skinManager.SetBlockSkin(block.Position, selectedSkin);
                }
            }
        }
        
        private string DeterminePatternType(MySlimBlock block)
        {
            var subtype = block.BlockDefinition?.Id.SubtypeId.String ?? "";
            
            if (subtype.Contains("Armor"))
                return "camouflage";
            else if (subtype.Contains("Heavy"))
                return "rusty";
            else if (subtype.Contains("Interior"))
                return "tech";
            else
                return "default";
        }
        
        private string DetermineZone(MySlimBlock block)
        {
            var subtype = block.BlockDefinition?.Id.SubtypeId.String ?? "";
            
            if (subtype.Contains("Weapon") || subtype.Contains("Turret"))
                return "weapon";
            else if (subtype.Contains("Thrust") || subtype.Contains("Engine"))
                return "propulsion";
            else if (subtype.Contains("Armor"))
                return "hull";
            else
                return "interior";
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
                new LetterBlockPainter(),
                new NavigationMarkingsPainter(orientationAnalyzer)
            };
        }
        
        private Random _random = new Random();
        
        private void LogInfo(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("EnhancedMilitaryPaint", message);
        }
        
        private void LogWarning(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("EnhancedMilitaryPaint", $"WARNING: {message}");
        }
        
        private void LogError(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("EnhancedMilitaryPaint", $"ERROR: {message}");
        }
    }
}