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
    /// Applies skins in patterns across the grid
    /// </summary>
    public class PatternSkinPainter : ISkinPainter
    {
        public string Name => "Pattern Skin Painter";
        public int Priority => 10;
        
        private readonly Random _random;
        
        public PatternSkinPainter(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }
        
        public void ApplySkins(MyCubeGrid grid, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette skinPalette, object context = null)
        {
            if (grid == null || skinResults == null || skinPalette == null)
                return;
            
            var blocks = grid.GetBlocks();
            if (blocks == null || blocks.Count == 0)
                return;
            
            // Determine pattern type based on palette
            var patternType = DeterminePatternType(skinPalette);
            
            switch (patternType)
            {
                case "stripes":
                    ApplyStripedPattern(blocks, skinResults, skinPalette);
                    break;
                    
                case "checkerboard":
                    ApplyCheckerboardPattern(blocks, skinResults, skinPalette);
                    break;
                    
                case "gradient":
                    ApplyGradientPattern(blocks, skinResults, skinPalette);
                    break;
                    
                case "random":
                    ApplyRandomPattern(blocks, skinResults, skinPalette);
                    break;
                    
                default:
                    ApplyBasicPattern(blocks, skinResults, skinPalette);
                    break;
            }
        }
        
        private string DeterminePatternType(SkinPalette palette)
        {
            // Determine pattern based on number of available skins
            var skinCount = palette.Skins.Count;
            
            if (skinCount >= 3)
                return "gradient";
            else if (skinCount == 2)
                return "stripes";
            else
                return "basic";
        }
        
        private void ApplyStripedPattern(HashSet<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            if (palette.PrimarySkin == MyStringHash.NullOrEmpty && palette.SecondarySkin == MyStringHash.NullOrEmpty)
                return;
            
            foreach (var block in blocks)
            {
                // Create horizontal stripes based on Y position
                var useSecondary = (block.Position.Y % 4) >= 2;
                var skinToUse = useSecondary && palette.SecondarySkin != MyStringHash.NullOrEmpty
                    ? palette.SecondarySkin
                    : palette.PrimarySkin;
                
                if (skinToUse != MyStringHash.NullOrEmpty)
                {
                    skinResults[block.Position] = skinToUse;
                }
            }
        }
        
        private void ApplyCheckerboardPattern(HashSet<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            if (palette.PrimarySkin == MyStringHash.NullOrEmpty && palette.SecondarySkin == MyStringHash.NullOrEmpty)
                return;
            
            foreach (var block in blocks)
            {
                // Create checkerboard based on position
                var isEven = ((block.Position.X + block.Position.Y + block.Position.Z) % 2) == 0;
                var skinToUse = isEven && palette.SecondarySkin != MyStringHash.NullOrEmpty
                    ? palette.SecondarySkin
                    : palette.PrimarySkin;
                
                if (skinToUse != MyStringHash.NullOrEmpty)
                {
                    skinResults[block.Position] = skinToUse;
                }
            }
        }
        
        private void ApplyGradientPattern(HashSet<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            if (palette.Skins.Count == 0)
                return;
            
            // Find grid bounds
            var minY = blocks.Min(b => b.Position.Y);
            var maxY = blocks.Max(b => b.Position.Y);
            var range = maxY - minY + 1;
            
            if (range <= 0)
                return;
            
            foreach (var block in blocks)
            {
                // Calculate gradient position
                var relativeY = block.Position.Y - minY;
                var gradientPos = (float)relativeY / range;
                var skinIndex = (int)(gradientPos * palette.Skins.Count);
                skinIndex = Math.Min(skinIndex, palette.Skins.Count - 1);
                
                var skinToUse = palette.GetSkinByIndex(skinIndex);
                if (skinToUse != MyStringHash.NullOrEmpty)
                {
                    skinResults[block.Position] = skinToUse;
                }
            }
        }
        
        private void ApplyRandomPattern(HashSet<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            if (palette.Skins.Count == 0)
                return;
            
            foreach (var block in blocks)
            {
                // Use deterministic randomness based on block position for consistency
                var skinIndex = Math.Abs(block.Position.GetHashCode() + _random.Next()) % palette.Skins.Count;
                
                var skinToUse = palette.GetSkinByIndex(skinIndex);
                if (skinToUse != MyStringHash.NullOrEmpty)
                {
                    skinResults[block.Position] = skinToUse;
                }
            }
        }
        
        private void ApplyBasicPattern(HashSet<MySlimBlock> blocks, Dictionary<Vector3I, MyStringHash> skinResults, SkinPalette palette)
        {
            if (palette.PrimarySkin == MyStringHash.NullOrEmpty)
                return;
            
            // Apply primary skin to all blocks
            foreach (var block in blocks)
            {
                skinResults[block.Position] = palette.PrimarySkin;
            }
        }
    }
}