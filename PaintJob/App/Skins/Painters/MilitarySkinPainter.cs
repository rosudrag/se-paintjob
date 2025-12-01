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
    /// Applies military-themed skins with tactical patterns
    /// </summary>
    public class MilitarySkinPainter : ISkinPainter
    {
        public string Name => "Military Skin Painter";
        public int Priority => 5; // Higher priority than pattern painter
        
        private readonly ISkinProvider _skinProvider;
        private readonly Random _random;
        
        public MilitarySkinPainter(ISkinProvider skinProvider, int? seed = null)
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
            
            // Categorize blocks by function
            var weaponBlocks = new List<MySlimBlock>();
            var armorBlocks = new List<MySlimBlock>();
            var functionalBlocks = new List<MySlimBlock>();
            var decorativeBlocks = new List<MySlimBlock>();
            
            foreach (var block in blocks)
            {
                var subtype = block.BlockDefinition?.Id.SubtypeId.String ?? "";
                
                if (IsWeaponBlock(subtype))
                    weaponBlocks.Add(block);
                else if (IsArmorBlock(subtype))
                    armorBlocks.Add(block);
                else if (IsFunctionalBlock(subtype))
                    functionalBlocks.Add(block);
                else
                    decorativeBlocks.Add(block);
            }
            
            // Apply skins based on block function
            ApplyCamouflageSkins(armorBlocks, skinResults, skinPalette);
            ApplyTacticalSkins(weaponBlocks, skinResults, skinPalette);
            ApplyUtilitySkins(functionalBlocks, skinResults, skinPalette);
            ApplyAccentSkins(decorativeBlocks, skinResults, skinPalette);
        }
        
        private void ApplyCamouflageSkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find camouflage skins in palette
            var camoSkins = palette.GetSkinsByCategory("military").Where(s => 
                s.String != null && (
                s.String.Contains("Camo", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Digital", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Moss", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            if (!camoSkins.Any() && palette.PrimarySkin != MyStringHash.NullOrEmpty)
                camoSkins.Add(palette.PrimarySkin);
            
            if (!camoSkins.Any())
                return;
            
            // Apply camouflage in patches
            foreach (var block in blocks)
            {
                // Create varied camouflage pattern
                var patchSize = 3;
                var patchX = block.Position.X / patchSize;
                var patchZ = block.Position.Z / patchSize;
                var patchIndex = Math.Abs(patchX + patchZ) % camoSkins.Count;
                
                skinResults[block.Position] = camoSkins[patchIndex];
            }
        }
        
        private void ApplyTacticalSkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find tactical/heavy skins
            var tacticalSkins = palette.GetSkinsByCategory("military").Where(s => 
                s.String != null && (
                s.String.Contains("Heavy", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Armor", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Carbon", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            if (!tacticalSkins.Any() && palette.SecondarySkin != MyStringHash.NullOrEmpty)
                tacticalSkins.Add(palette.SecondarySkin);
            
            if (!tacticalSkins.Any())
                return;
            
            // Apply tactical skins to weapon blocks
            foreach (var block in blocks)
            {
                var skinIndex = Math.Abs(block.Position.GetHashCode()) % tacticalSkins.Count;
                skinResults[block.Position] = tacticalSkins[skinIndex];
            }
        }
        
        private void ApplyUtilitySkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find utility/worn skins
            var utilitySkins = palette.Skins.Where(s => 
                s.String != null && (
                s.String.Contains("Rust", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Battered", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Worn", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            if (!utilitySkins.Any() && palette.DetailSkin != MyStringHash.NullOrEmpty)
                utilitySkins.Add(palette.DetailSkin);
            
            if (!utilitySkins.Any())
                return;
            
            // Apply utility skins with some randomness
            foreach (var block in blocks)
            {
                // Only apply to some blocks for variety
                if (_random.Next(100) < 60) // 60% chance
                {
                    var skinIndex = _random.Next(utilitySkins.Count);
                    skinResults[block.Position] = utilitySkins[skinIndex];
                }
            }
        }
        
        private void ApplyAccentSkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Use detail skin for accents if available
            if (palette.DetailSkin == MyStringHash.NullOrEmpty)
                return;
            
            // Apply accent skin sparingly
            foreach (var block in blocks)
            {
                if (_random.Next(100) < 20) // 20% chance for accent
                {
                    skinResults[block.Position] = palette.DetailSkin;
                }
            }
        }
        
        private bool IsWeaponBlock(string subtype)
        {
            var weaponKeywords = new[] { "Gatling", "Missile", "Rocket", "Interior", "Turret", "Fixed" };
            return weaponKeywords.Any(keyword => subtype.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        
        private bool IsArmorBlock(string subtype)
        {
            var armorKeywords = new[] { "Armor", "Heavy", "Light", "Slope", "Corner", "Inv" };
            return armorKeywords.Any(keyword => subtype.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        
        private bool IsFunctionalBlock(string subtype)
        {
            var functionalKeywords = new[] { "Cargo", "Reactor", "Battery", "Thrust", "Gyro", "Conveyor", "Connector" };
            return functionalKeywords.Any(keyword => subtype.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}