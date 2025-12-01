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
    /// Applies alien-themed skins with organic, crystalline, and energy aesthetics
    /// </summary>
    public class AlienSkinPainter : ISkinPainter
    {
        public string Name => "Alien Skin Painter";
        public int Priority => 5;
        
        private readonly ISkinProvider _skinProvider;
        private readonly Random _random;
        
        public AlienSkinPainter(ISkinProvider skinProvider, int? seed = null)
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
            
            // Categorize blocks for alien theme
            var carapaceBlocks = new List<MySlimBlock>();
            var organBlocks = new List<MySlimBlock>();
            var sensoryBlocks = new List<MySlimBlock>();
            var coreBlocks = new List<MySlimBlock>();
            
            foreach (var block in blocks)
            {
                var subtype = block.BlockDefinition?.Id.SubtypeId.String ?? "";
                
                if (IsCoreBlock(subtype))
                    coreBlocks.Add(block);
                else if (IsSensoryBlock(subtype))
                    sensoryBlocks.Add(block);
                else if (IsOrganBlock(subtype))
                    organBlocks.Add(block);
                else
                    carapaceBlocks.Add(block);
            }
            
            // Apply skins based on alien theme
            ApplyCarapaceSkins(carapaceBlocks, skinResults, skinPalette);
            ApplyOrganicSkins(organBlocks, skinResults, skinPalette);
            ApplySensorySkins(sensoryBlocks, skinResults, skinPalette);
            ApplyCoreSkins(coreBlocks, skinResults, skinPalette);
        }
        
        private void ApplyCarapaceSkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find organic/alien carapace skins
            var carapaceSkins = palette.Skins.Where(s => 
                s.String != null && (
                s.String.Contains("Moss", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Organic", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Frozen", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Alien", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            // If no organic skins, use sci-fi skins
            if (!carapaceSkins.Any())
            {
                carapaceSkins = palette.Skins.Where(s => 
                    s.String != null && s.String.Contains("Sci", StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            
            if (!carapaceSkins.Any() && palette.PrimarySkin != MyStringHash.NullOrEmpty)
                carapaceSkins.Add(palette.PrimarySkin);
            
            if (!carapaceSkins.Any())
                return;
            
            // Apply organic carapace with variation
            foreach (var block in blocks)
            {
                if (_random.NextDouble() < 0.8) // Most get carapace
                {
                    var skinIndex = GetOrganicPattern(block.Position, carapaceSkins.Count);
                    skinResults[block.Position] = carapaceSkins[skinIndex];
                }
            }
        }
        
        private void ApplyOrganicSkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find biomechanical/organic skins
            var organicSkins = palette.Skins.Where(s => 
                s.String != null && (
                s.String.Contains("Wood", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Moss", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Corroded", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            if (!organicSkins.Any() && palette.SecondarySkin != MyStringHash.NullOrEmpty)
                organicSkins.Add(palette.SecondarySkin);
            
            if (!organicSkins.Any())
                return;
            
            // Apply to internal organs/systems
            foreach (var block in blocks)
            {
                var skinIndex = _random.Next(organicSkins.Count);
                skinResults[block.Position] = organicSkins[skinIndex];
            }
        }
        
        private void ApplySensorySkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find glowing/energy skins for sensory organs
            var sensorySkins = palette.Skins.Where(s => 
                s.String != null && (
                s.String.Contains("Neon", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Glow", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Energy", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Disco", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            if (!sensorySkins.Any())
            {
                // Use any colorful skins as fallback
                sensorySkins = palette.Skins.Where(s => 
                    s.String != null && (
                    s.String.Contains("Glamour", StringComparison.OrdinalIgnoreCase) ||
                    s.String.Contains("Golden", StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            
            if (!sensorySkins.Any())
                return;
            
            // Apply bioluminescent effects to sensors
            foreach (var block in blocks)
            {
                var skinIndex = _random.Next(sensorySkins.Count);
                skinResults[block.Position] = sensorySkins[skinIndex];
            }
        }
        
        private void ApplyCoreSkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find exotic/energy core skins
            var coreSkins = palette.Skins.Where(s => 
                s.String != null && (
                s.String.Contains("Sci", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Digital", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Energy", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            // If no energy skins, use metallic/tech skins
            if (!coreSkins.Any())
            {
                coreSkins = palette.Skins.Where(s => 
                    s.String != null && (
                    s.String.Contains("Silver", StringComparison.OrdinalIgnoreCase) ||
                    s.String.Contains("Chrome", StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            
            if (!coreSkins.Any() && palette.DetailSkin != MyStringHash.NullOrEmpty)
                coreSkins.Add(palette.DetailSkin);
            
            if (!coreSkins.Any())
                return;
            
            // Apply exotic core materials
            foreach (var block in blocks)
            {
                var skinIndex = _random.Next(coreSkins.Count);
                skinResults[block.Position] = coreSkins[skinIndex];
            }
        }
        
        private int GetOrganicPattern(Vector3I position, int maxIndex)
        {
            // Create organic pattern based on position
            float noise = (float)(Math.Sin(position.X * 0.5f) * 
                                 Math.Cos(position.Y * 0.5f) * 
                                 Math.Sin(position.Z * 0.5f) + 1.0f) * 0.5f;
            
            return (int)(noise * maxIndex) % maxIndex;
        }
        
        private bool IsCoreBlock(string subtype)
        {
            var coreKeywords = new[] { "Reactor", "Battery", "Solar", "Generator" };
            return coreKeywords.Any(keyword => subtype.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        
        private bool IsSensoryBlock(string subtype)
        {
            var sensoryKeywords = new[] { "Sensor", "Antenna", "Camera", "Detector", "Light" };
            return sensoryKeywords.Any(keyword => subtype.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        
        private bool IsOrganBlock(string subtype)
        {
            var organKeywords = new[] { "Conveyor", "Connector", "Piston", "Rotor", "Hinge" };
            return organKeywords.Any(keyword => subtype.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}