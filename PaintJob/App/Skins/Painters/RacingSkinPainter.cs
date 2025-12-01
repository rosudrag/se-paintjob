using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRage.Utils;
using VRageMath;

namespace PaintJob.App.Skins.Painters
{
    /// <summary>
    /// Applies racing-themed skins with carbon fiber and performance aesthetics
    /// </summary>
    public class RacingSkinPainter : ISkinPainter
    {
        public string Name => "Racing Skin Painter";
        public int Priority => 5;
        
        private readonly ISkinProvider _skinProvider;
        private readonly Random _random;
        
        public RacingSkinPainter(ISkinProvider skinProvider, int? seed = null)
        {
            _skinProvider = skinProvider ?? throw new ArgumentNullException(nameof(skinProvider));
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }
        
        public void ApplySkins(MyCubeGrid grid, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette skinPalette, object context = null)
        {
            if (grid == null || skinResults == null || skinPalette == null)
                return;
            
            var blocks = grid.GetBlocks();
            if (blocks == null || blocks.Count == 0)
                return;
            
            // Categorize blocks for racing theme
            var bodyBlocks = new List<MySlimBlock>();
            var aeroBlocks = new List<MySlimBlock>();
            var performanceBlocks = new List<MySlimBlock>();
            var cockpitBlocks = new List<MySlimBlock>();
            
            foreach (var block in blocks)
            {
                var subtype = block.BlockDefinition?.Id.SubtypeId.String ?? "";
                
                if (IsCockpitBlock(subtype))
                    cockpitBlocks.Add(block);
                else if (IsAeroBlock(subtype))
                    aeroBlocks.Add(block);
                else if (IsPerformanceBlock(subtype))
                    performanceBlocks.Add(block);
                else
                    bodyBlocks.Add(block);
            }
            
            // Apply skins based on racing theme
            ApplyCarbonFiberSkins(bodyBlocks, skinResults, skinPalette);
            ApplyAerodynamicSkins(aeroBlocks, skinResults, skinPalette);
            ApplyPerformanceSkins(performanceBlocks, skinResults, skinPalette);
            ApplyCockpitSkins(cockpitBlocks, skinResults, skinPalette);
        }
        
        private void ApplyCarbonFiberSkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find carbon fiber and tech skins
            var carbonSkins = palette.Skins.Where(s => 
                s.String != null && (
                s.String.Contains("Carbon", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Fiber", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Tech", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            if (!carbonSkins.Any() && palette.PrimarySkin != MyStringHash.NullOrEmpty)
                carbonSkins.Add(palette.PrimarySkin);
            
            if (!carbonSkins.Any())
                return;
            
            // Apply carbon fiber to main body
            foreach (var block in blocks)
            {
                // Most body panels get carbon fiber
                if (_random.NextDouble() < 0.7)
                {
                    var skinIndex = _random.Next(carbonSkins.Count);
                    skinResults[block.Position] = carbonSkins[skinIndex];
                }
            }
        }
        
        private void ApplyAerodynamicSkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find sleek, smooth skins for aerodynamics
            var aeroSkins = palette.Skins.Where(s => 
                s.String != null && (
                s.String.Contains("Smooth", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Glossy", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Chrome", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Silver", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            if (!aeroSkins.Any() && palette.SecondarySkin != MyStringHash.NullOrEmpty)
                aeroSkins.Add(palette.SecondarySkin);
            
            if (!aeroSkins.Any())
                return;
            
            // Apply to aerodynamic surfaces
            foreach (var block in blocks)
            {
                var skinIndex = _random.Next(aeroSkins.Count);
                skinResults[block.Position] = aeroSkins[skinIndex];
            }
        }
        
        private void ApplyPerformanceSkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find high-tech performance skins
            var performanceSkins = palette.Skins.Where(s => 
                s.String != null && (
                s.String.Contains("Neon", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Energy", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Glow", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Digital", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            if (!performanceSkins.Any())
            {
                // Fallback to any tech-looking skins
                performanceSkins = palette.Skins.Where(s => 
                    s.String != null && s.String.Contains("Sci", StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            
            if (!performanceSkins.Any())
                return;
            
            // Apply to performance parts (thrusters, reactors, etc.)
            foreach (var block in blocks)
            {
                if (_random.NextDouble() < 0.8) // High chance for performance parts
                {
                    var skinIndex = _random.Next(performanceSkins.Count);
                    skinResults[block.Position] = performanceSkins[skinIndex];
                }
            }
        }
        
        private void ApplyCockpitSkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find premium/luxury skins for cockpit
            var cockpitSkins = palette.Skins.Where(s => 
                s.String != null && (
                s.String.Contains("Glass", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Clear", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Premium", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Luxury", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            // If no luxury skins, use clean tech skins
            if (!cockpitSkins.Any())
            {
                cockpitSkins = palette.Skins.Where(s => 
                    s.String != null && (
                    s.String.Contains("Clean", StringComparison.OrdinalIgnoreCase) ||
                    s.String.Contains("Silver", StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            
            if (!cockpitSkins.Any() && palette.DetailSkin != MyStringHash.NullOrEmpty)
                cockpitSkins.Add(palette.DetailSkin);
            
            if (!cockpitSkins.Any())
                return;
            
            // Apply premium skins to cockpit area
            foreach (var block in blocks)
            {
                var skinIndex = _random.Next(cockpitSkins.Count);
                skinResults[block.Position] = cockpitSkins[skinIndex];
            }
        }
        
        private bool IsCockpitBlock(string subtype)
        {
            var cockpitKeywords = new[] { "Cockpit", "Control", "Seat", "Console" };
            return cockpitKeywords.Any(keyword => subtype.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        
        private bool IsAeroBlock(string subtype)
        {
            var aeroKeywords = new[] { "Slope", "Corner", "Wing", "Fin" };
            return aeroKeywords.Any(keyword => subtype.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        
        private bool IsPerformanceBlock(string subtype)
        {
            var performanceKeywords = new[] { "Thrust", "Reactor", "Battery", "Engine", "Gyro" };
            return performanceKeywords.Any(keyword => subtype.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}