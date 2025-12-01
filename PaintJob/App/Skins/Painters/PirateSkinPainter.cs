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
    /// Applies pirate-themed skins with weathering and battle damage
    /// </summary>
    public class PirateSkinPainter : ISkinPainter
    {
        public string Name => "Pirate Skin Painter";
        public int Priority => 5; // Higher priority than pattern painter
        
        private readonly ISkinProvider _skinProvider;
        private readonly Random _random;
        
        public PirateSkinPainter(ISkinProvider skinProvider, int? seed = null)
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
            
            // Categorize blocks for pirate theming
            var exteriorBlocks = new List<MySlimBlock>();
            var weaponBlocks = new List<MySlimBlock>();
            var cargoBlocks = new List<MySlimBlock>();
            var interiorBlocks = new List<MySlimBlock>();
            
            foreach (var block in blocks)
            {
                var subtype = block.BlockDefinition?.Id.SubtypeId.String ?? "";
                
                if (IsWeaponBlock(subtype))
                    weaponBlocks.Add(block);
                else if (IsCargoBlock(subtype))
                    cargoBlocks.Add(block);
                else if (IsExteriorBlock(subtype))
                    exteriorBlocks.Add(block);
                else
                    interiorBlocks.Add(block);
            }
            
            // Apply skins based on pirate theme
            ApplyWeatheredSkins(exteriorBlocks, skinResults, skinPalette);
            ApplyBattleScarredSkins(weaponBlocks, skinResults, skinPalette);
            ApplyTreasureSkins(cargoBlocks, skinResults, skinPalette);
            ApplyMakeshiftSkins(interiorBlocks, skinResults, skinPalette);
        }
        
        private void ApplyWeatheredSkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find heavily weathered and rusty skins
            var weatheredSkins = palette.Skins.Where(s => 
                s.String != null && (
                s.String.Contains("Rust", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Corroded", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Battered", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Worn", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            // Add primary skin if no weathered skins found
            if (!weatheredSkins.Any() && palette.PrimarySkin != MyStringHash.NullOrEmpty)
                weatheredSkins.Add(palette.PrimarySkin);
            
            if (!weatheredSkins.Any())
                return;
            
            // Apply weathering with heavy variation
            foreach (var block in blocks)
            {
                // Most blocks get weathered
                if (_random.NextDouble() < 0.85)
                {
                    var skinIndex = _random.Next(weatheredSkins.Count);
                    skinResults[block.Position] = weatheredSkins[skinIndex];
                }
            }
        }
        
        private void ApplyBattleScarredSkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find battle-damaged skins
            var battleSkins = palette.Skins.Where(s => 
                s.String != null && (
                s.String.Contains("Heavy", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Battered", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Damaged", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            if (!battleSkins.Any() && palette.SecondarySkin != MyStringHash.NullOrEmpty)
                battleSkins.Add(palette.SecondarySkin);
            
            if (!battleSkins.Any())
                return;
            
            // Apply battle damage to weapons
            foreach (var block in blocks)
            {
                var skinIndex = _random.Next(battleSkins.Count);
                skinResults[block.Position] = battleSkins[skinIndex];
            }
        }
        
        private void ApplyTreasureSkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find treasure/valuable skins
            var treasureSkins = palette.Skins.Where(s => 
                s.String != null && (
                s.String.Contains("Gold", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Glamour", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Silver", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Bronze", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            if (!treasureSkins.Any())
                return;
            
            // Rarely apply treasure skins to cargo areas
            foreach (var block in blocks)
            {
                if (_random.NextDouble() < 0.25) // 25% chance for treasure
                {
                    var skinIndex = _random.Next(treasureSkins.Count);
                    skinResults[block.Position] = treasureSkins[skinIndex];
                }
            }
        }
        
        private void ApplyMakeshiftSkins(List<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            // Find makeshift/scavenged skins
            var makeshiftSkins = palette.Skins.Where(s => 
                s.String != null && (
                s.String.Contains("Wood", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Scrap", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Makeshift", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            // If no makeshift skins, use random mix (scavenged parts)
            if (!makeshiftSkins.Any() && palette.Skins.Count > 0)
            {
                // Pirates use whatever they can find
                makeshiftSkins = palette.Skins.ToList();
            }
            
            if (!makeshiftSkins.Any())
                return;
            
            // Apply random makeshift repairs
            foreach (var block in blocks)
            {
                if (_random.NextDouble() < 0.4) // 40% chance for makeshift repairs
                {
                    var skinIndex = _random.Next(makeshiftSkins.Count);
                    skinResults[block.Position] = makeshiftSkins[skinIndex];
                }
            }
        }
        
        private bool IsWeaponBlock(string subtype)
        {
            var weaponKeywords = new[] { "Gatling", "Missile", "Rocket", "Turret", "Fixed", "Cannon" };
            return weaponKeywords.Any(keyword => subtype.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        
        private bool IsCargoBlock(string subtype)
        {
            var cargoKeywords = new[] { "Cargo", "Container", "Storage" };
            return cargoKeywords.Any(keyword => subtype.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        
        private bool IsExteriorBlock(string subtype)
        {
            var exteriorKeywords = new[] { "Armor", "Heavy", "Light", "Slope", "Corner" };
            return exteriorKeywords.Any(keyword => subtype.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}