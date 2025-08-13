using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms
{
    public class RacingPaintJob : PaintAlgorithm
    {
        private readonly Dictionary<Vector3I, int> _colorResults;
        private Vector3[] _colorPalette;
        private string _variant = "formula1";

        public RacingPaintJob()
        {
            _colorResults = new Dictionary<Vector3I, int>();
        }

        public void SetVariant(string variant)
        {
            _variant = variant?.ToLower() ?? "formula1";
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
                var blocks = grid.GetBlocks();
                var bounds = CalculateGridBounds(blocks);
                
                // Initialize painters
                var gradientPainter = new Common.Painters.GradientPainter(unchecked((int)grid.EntityId));
                var patternPainter = new Common.Painters.PatternPainter(unchecked((int)grid.EntityId));
                
                // Apply base gradient for sleek look
                gradientPainter.ApplyGradient(
                    blocks, 
                    _colorResults, 
                    Common.Painters.GradientPainter.GradientType.Linear,
                    0, // Base color
                    1, // Slightly lighter base
                    direction: Vector3.Forward
                );
                
                // Apply racing stripes with improved pattern
                if (_variant == "formula1" || _variant == "rally")
                {
                    patternPainter.ApplyPattern(
                        blocks,
                        _colorResults,
                        Common.Painters.PatternPainter.PatternType.Stripes,
                        2, // Stripe color
                        3, // Alt stripe color
                        scale: 1.5f,
                        direction: Vector3.Forward
                    );
                }
                else if (_variant == "street")
                {
                    // Street racing gets zigzag pattern
                    patternPainter.ApplyPattern(
                        blocks,
                        _colorResults,
                        Common.Painters.PatternPainter.PatternType.Zigzag,
                        2, // Accent color
                        3, // Secondary accent
                        scale: 2f
                    );
                }
                
                // Apply checkered flag pattern on specific areas
                ApplyCheckeredPattern(blocks, bounds, patternPainter);
                
                // Apply functional accents
                ApplyFunctionalAccents(blocks);
                
                // Apply racing numbers with better visibility
                ApplyRacingNumbers(blocks, bounds);
                
                // Add speed blur effect on sides
                ApplySpeedBlurEffect(blocks, bounds, gradientPainter);
                
                // Apply final colors to grid
                foreach (var block in blocks)
                {
                    if (_colorResults.TryGetValue(block.Position, out var colorIndex))
                    {
                        if (colorIndex >= 0 && colorIndex < _colorPalette.Length)
                        {
                            grid.ColorBlocks(block.Min, block.Max, _colorPalette[colorIndex], false);
                        }
                    }
                }
                
                LogInfo($"Applied racing paint job variant '{_variant}' to grid");
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply racing paint job: {ex.Message}");
                throw;
            }
        }

        private void ApplyCheckeredPattern(HashSet<MySlimBlock> blocks, BoundingBox bounds, Common.Painters.PatternPainter patternPainter)
        {
            // Apply checkered flag pattern on front/rear sections
            var frontBlocks = blocks.Where(b => b.Position.Z <= bounds.Min.Z + 3).ToHashSet();
            var rearBlocks = blocks.Where(b => b.Position.Z >= bounds.Max.Z - 3).ToHashSet();
            
            if (frontBlocks.Count > 0)
            {
                patternPainter.ApplyPattern(
                    frontBlocks,
                    _colorResults,
                    Common.Painters.PatternPainter.PatternType.Checkerboard,
                    7, // White
                    0, // Black
                    scale: 0.8f
                );
            }
            
            if (rearBlocks.Count > 0)
            {
                patternPainter.ApplyPattern(
                    rearBlocks,
                    _colorResults,
                    Common.Painters.PatternPainter.PatternType.Checkerboard,
                    7, // White
                    0, // Black
                    scale: 0.8f
                );
            }
        }

        private void ApplySpeedBlurEffect(HashSet<MySlimBlock> blocks, BoundingBox bounds, Common.Painters.GradientPainter gradientPainter)
        {
            // Create speed blur effect on sides using gradient
            var leftBlocks = blocks.Where(b => b.Position.X <= bounds.Min.X + 2).ToHashSet();
            var rightBlocks = blocks.Where(b => b.Position.X >= bounds.Max.X - 2).ToHashSet();
            
            if (leftBlocks.Count > 0)
            {
                gradientPainter.ApplyGradient(
                    leftBlocks,
                    _colorResults,
                    Common.Painters.GradientPainter.GradientType.Linear,
                    1, // Light
                    4, // Hot color (speed effect)
                    direction: Vector3.Backward
                );
            }
            
            if (rightBlocks.Count > 0)
            {
                gradientPainter.ApplyGradient(
                    rightBlocks,
                    _colorResults,
                    Common.Painters.GradientPainter.GradientType.Linear,
                    1, // Light
                    4, // Hot color (speed effect)
                    direction: Vector3.Backward
                );
            }
        }

        private void ApplyRacingStripes(HashSet<MySlimBlock> blocks, BoundingBox bounds)
        {
            var centerZ = (bounds.Min.Z + bounds.Max.Z) / 2f;
            var stripeWidth = (bounds.Max.Z - bounds.Min.Z) * 0.15f; // 15% width stripes
            
            foreach (var block in blocks)
            {
                var pos = block.Position;
                var distFromCenter = Math.Abs(pos.Z - centerZ);
                
                if (distFromCenter <= stripeWidth)
                {
                    // Center stripe
                    _colorResults[pos] = 2; // Accent color
                }
                else if (distFromCenter <= stripeWidth * 2)
                {
                    // Side stripes
                    _colorResults[pos] = 3; // Secondary accent
                }
                else
                {
                    // Base color
                    _colorResults[pos] = 0; // Primary color
                }
            }
        }

        private void ApplyFunctionalAccents(HashSet<MySlimBlock> blocks)
        {
            foreach (var block in blocks)
            {
                var blockDef = block.BlockDefinition.Id.SubtypeName;
                
                // Thrusters get hot colors
                if (blockDef.Contains("Thrust"))
                {
                    _colorResults[block.Position] = 4; // Hot color
                }
                // Weapons get aggressive colors
                else if (blockDef.Contains("Gatling") || blockDef.Contains("Missile") || blockDef.Contains("Rocket"))
                {
                    _colorResults[block.Position] = 5; // Weapon color
                }
                // Cockpits get canopy tint
                else if (blockDef.Contains("Cockpit"))
                {
                    _colorResults[block.Position] = 6; // Canopy tint
                }
            }
        }

        private void ApplyRacingNumbers(HashSet<MySlimBlock> blocks, BoundingBox bounds)
        {
            // Simulate racing numbers on the sides by creating patterns
            var sideBlocks = blocks.Where(b => 
                Math.Abs(b.Position.X - bounds.Min.X) < 2 || 
                Math.Abs(b.Position.X - bounds.Max.X) < 2).ToList();
            
            // Create a simple "number" pattern in the middle section
            var centerY = (bounds.Min.Y + bounds.Max.Y) / 2f;
            var centerZ = (bounds.Min.Z + bounds.Max.Z) / 2f;
            
            foreach (var block in sideBlocks)
            {
                var distY = Math.Abs(block.Position.Y - centerY);
                var distZ = Math.Abs(block.Position.Z - centerZ);
                
                // Create a rectangular area for the "number"
                if (distY < 3 && distZ < 2)
                {
                    _colorResults[block.Position] = 7; // Number color (high contrast)
                }
            }
        }

        private BoundingBox CalculateGridBounds(HashSet<MySlimBlock> blocks)
        {
            if (blocks.Count == 0)
                return new BoundingBox();
            
            var firstPos = blocks.First().Position;
            var min = new Vector3(firstPos);
            var max = new Vector3(firstPos);
            
            foreach (var block in blocks)
            {
                min = Vector3.Min(min, block.Position);
                max = Vector3.Max(max, block.Position);
            }
            
            return new BoundingBox(min, max);
        }

        protected override void GeneratePalette(MyCubeGrid grid)
        {
            var seed = unchecked((int)grid.EntityId);
            var colorGenerator = new Utils.ColorSchemeGenerator(seed);
            
            // Generate racing-specific palette based on variant
            _colorPalette = colorGenerator.GenerateRacingPalette(_variant);
            
            if (_colorPalette == null || _colorPalette.Length == 0)
            {
                throw new InvalidOperationException("Failed to generate racing color palette");
            }
            
            LogInfo($"Generated racing palette with {_colorPalette.Length} colors for variant '{_variant}'");
        }

        private void LogInfo(string message)
        {
            MyAPIGateway.Utilities.ShowMessage("RacingPaintJob", message);
        }

        private void LogError(string message)
        {
            MyAPIGateway.Utilities.ShowMessage("RacingPaintJob", $"ERROR: {message}");
        }
    }
}