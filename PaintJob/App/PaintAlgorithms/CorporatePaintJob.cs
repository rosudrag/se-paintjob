using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms
{
    public class CorporatePaintJob : PaintAlgorithm
    {
        private readonly Dictionary<Vector3I, int> _colorResults;
        private Vector3[] _colorPalette;
        private string _variant = "mining_corp";
        private Random _random;

        public CorporatePaintJob()
        {
            _colorResults = new Dictionary<Vector3I, int>();
        }

        public void SetVariant(string variant)
        {
            _variant = variant?.ToLower() ?? "mining_corp";
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
                _random = new Random(unchecked((int)grid.EntityId));
                
                // Apply clean base colors
                ApplyBaseColors(blocks);
                
                // Apply department color coding based on function
                ApplyDepartmentColors(blocks);
                
                // Apply corporate branding areas
                ApplyCorporateBranding(blocks);
                
                // Apply safety markings
                ApplySafetyMarkings(blocks);
                
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
                
                LogInfo($"Applied corporate paint job variant '{_variant}' to grid");
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply corporate paint job: {ex.Message}");
                throw;
            }
        }

        private void ApplyBaseColors(HashSet<MySlimBlock> blocks)
        {
            foreach (var block in blocks)
            {
                // Clean, professional base color
                _colorResults[block.Position] = 0; // Primary corporate color
            }
        }

        private void ApplyDepartmentColors(HashSet<MySlimBlock> blocks)
        {
            foreach (var block in blocks)
            {
                var blockDef = block.BlockDefinition.Id.SubtypeName;
                
                // Engineering department - orange/yellow
                if (blockDef.Contains("Reactor") || blockDef.Contains("Battery") || 
                    blockDef.Contains("Solar") || blockDef.Contains("Generator"))
                {
                    _colorResults[block.Position] = 1; // Engineering color
                }
                // Logistics department - blue
                else if (blockDef.Contains("Cargo") || blockDef.Contains("Container") || 
                         blockDef.Contains("Connector") || blockDef.Contains("Conveyor"))
                {
                    _colorResults[block.Position] = 2; // Logistics color
                }
                // Operations department - green
                else if (blockDef.Contains("Drill") || blockDef.Contains("Grinder") || 
                         blockDef.Contains("Welder") || blockDef.Contains("Refinery") ||
                         blockDef.Contains("Assembler"))
                {
                    _colorResults[block.Position] = 3; // Operations color
                }
                // Command department - white/silver
                else if (blockDef.Contains("Cockpit") || blockDef.Contains("Control") || 
                         blockDef.Contains("Remote") || blockDef.Contains("Antenna"))
                {
                    _colorResults[block.Position] = 4; // Command color
                }
                // Security department - red accents
                else if (blockDef.Contains("Gatling") || blockDef.Contains("Missile") || 
                         blockDef.Contains("Rocket") || blockDef.Contains("Turret"))
                {
                    _colorResults[block.Position] = 5; // Security color
                }
                // Medical/Life Support - white with green cross
                else if (blockDef.Contains("Medical") || blockDef.Contains("Oxygen") || 
                         blockDef.Contains("Cryo") || blockDef.Contains("Survival"))
                {
                    _colorResults[block.Position] = 6; // Medical color
                }
            }
        }

        private void ApplyCorporateBranding(HashSet<MySlimBlock> blocks)
        {
            var gradientPainter = new Common.Painters.GradientPainter(unchecked((int)_random.Next()));
            var patternPainter = new Common.Painters.PatternPainter(unchecked((int)_random.Next()));
            
            // Find large flat surfaces for logo placement
            var bounds = CalculateGridBounds(blocks);
            
            // Apply corporate stripe pattern along hull
            patternPainter.ApplyPattern(
                blocks,
                _colorResults,
                Common.Painters.PatternPainter.PatternType.Stripes,
                8, // Corporate stripe primary
                0, // Base color
                scale: 3f,
                direction: Vector3.Right
            );
            
            // Apply gradient for professional sheen
            gradientPainter.ApplyGradient(
                blocks,
                _colorResults,
                Common.Painters.GradientPainter.GradientType.Linear,
                0, // Base
                7, // Slightly lighter for sheen
                direction: Vector3.Up
            );
            
            // Identify front section for corporate logo
            var frontBlocks = blocks.Where(b => b.Position.Z <= bounds.Min.Z + 5).ToHashSet();
            if (frontBlocks.Count > 0)
            {
                // Apply hexagon pattern for tech company feel
                if (_variant == "research_lab" || _variant == "security_firm")
                {
                    patternPainter.ApplyPattern(
                        frontBlocks,
                        _colorResults,
                        Common.Painters.PatternPainter.PatternType.Hexagons,
                        7, // Logo primary
                        8, // Logo secondary
                        scale: 1.5f
                    );
                }
                // Apply dots for transport/logistics
                else if (_variant == "transport_co")
                {
                    patternPainter.ApplyPattern(
                        frontBlocks,
                        _colorResults,
                        Common.Painters.PatternPainter.PatternType.Dots,
                        7, // Logo dots
                        0, // Background
                        scale: 2f
                    );
                }
            }
            
            // Add corporate ID stripes on sides
            var sideBlocks = blocks.Where(b => 
                Math.Abs(b.Position.X - bounds.Min.X) < 2 || 
                Math.Abs(b.Position.X - bounds.Max.X) < 2).ToHashSet();
                
            if (sideBlocks.Count > 0)
            {
                patternPainter.ApplyPattern(
                    sideBlocks,
                    _colorResults,
                    Common.Painters.PatternPainter.PatternType.Stripes,
                    8, // Corporate ID color
                    0, // Base
                    scale: 1f,
                    direction: Vector3.Forward
                );
            }
        }

        private void ApplySafetyMarkings(HashSet<MySlimBlock> blocks)
        {
            foreach (var block in blocks)
            {
                var blockDef = block.BlockDefinition.Id.SubtypeName;
                
                // Hazard markings for dangerous areas
                if (blockDef.Contains("Warhead") || blockDef.Contains("Explosive"))
                {
                    _colorResults[block.Position] = 9; // Hazard yellow/black
                }
                // Emergency equipment - bright red
                else if (blockDef.Contains("Door") && block.Position.Y < 0) // Assume lower doors are emergency
                {
                    _colorResults[block.Position] = 10; // Emergency red
                }
                // Navigation lights
                else if (blockDef.Contains("Light"))
                {
                    // Port/starboard logic
                    var bounds = CalculateGridBounds(blocks);
                    if (block.Position.X < (bounds.Min.X + bounds.Max.X) / 2f)
                    {
                        _colorResults[block.Position] = 11; // Port (red tint)
                    }
                    else
                    {
                        _colorResults[block.Position] = 12; // Starboard (green tint)
                    }
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
            
            // Generate corporate-specific palette based on company type
            _colorPalette = colorGenerator.GenerateCorporatePalette(_variant);
            
            if (_colorPalette == null || _colorPalette.Length == 0)
            {
                throw new InvalidOperationException("Failed to generate corporate color palette");
            }
            
            LogInfo($"Generated corporate palette with {_colorPalette.Length} colors for variant '{_variant}'");
        }

        private void LogInfo(string message)
        {
            MyAPIGateway.Utilities.ShowMessage("CorporatePaintJob", message);
        }

        private void LogError(string message)
        {
            MyAPIGateway.Utilities.ShowMessage("CorporatePaintJob", $"ERROR: {message}");
        }
    }
}