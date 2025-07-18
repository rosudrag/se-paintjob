using System.Collections.Generic;
using System.Linq;
using PaintJob.App.PaintAlgorithms.Common.Painters;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms.Military.Painters
{
    /// <summary>
    /// Applies special coloring to letter/alphabet blocks for ship designations and names.
    /// </summary>
    public class LetterBlockPainter : IBlockPainter
    {
        public string Name => "Letter Block";
        public int Priority => 350; // Higher than camouflage but lower than navigation

        public void ApplyColors(MyCubeGrid grid, Dictionary<Vector3I, int> colorResults, PaintContext context)
        {
            var letterBlocks = new HashSet<Vector3I>();
            
            // Find all letter blocks
            foreach (var block in context.Blocks)
            {
                if (IsLetterBlock(block))
                {
                    letterBlocks.Add(block.Position);
                    colorResults[block.Position] = (int)MilitaryColorScheme.ColorIndex.LetterBlock;
                }
            }
            
            // Apply background color to blocks surrounding letter blocks
            if (letterBlocks.Any())
            {
                ApplyLetterBlockBackground(grid, letterBlocks, colorResults);
            }
        }
        
        private bool IsLetterBlock(MySlimBlock block)
        {
            if (block.BlockDefinition?.Id == null)
                return false;
            
            var subtypeName = block.BlockDefinition.Id.SubtypeName.ToLower();
            var typeName = block.BlockDefinition.Id.TypeId.ToString().ToLower();
            
            // Check for letter blocks - they typically have names like:
            // - LetterA, LetterB, etc.
            // - SmallBlockLetter*
            // - LargeBlockLetter*
            // - Symbol blocks
            // - Number blocks
            return subtypeName.Contains("letter") || 
                   subtypeName.Contains("symbol") ||
                   subtypeName.Contains("number") ||
                   (typeName.Contains("text") && !typeName.Contains("panel")) || // Text blocks but not LCD panels
                   IsAlphabetBlock(subtypeName);
        }
        
        private bool IsAlphabetBlock(string subtypeName)
        {
            // Check if it's a single letter block (A-Z, 0-9)
            if (subtypeName.Length >= 2)
            {
                var lastChar = subtypeName[subtypeName.Length - 1];
                var secondLastChar = subtypeName.Length > 1 ? subtypeName[subtypeName.Length - 2] : ' ';
                
                // Check patterns like "BlockA" or "Block_A" or "BlockLetterA"
                if (char.IsLetter(lastChar) && lastChar >= 'A' && lastChar <= 'Z')
                {
                    if (secondLastChar == '_' || secondLastChar == '-' || 
                        !char.IsLetter(secondLastChar) || subtypeName.EndsWith("Letter" + lastChar))
                    {
                        return true;
                    }
                }
                
                // Check for numbers
                if (char.IsDigit(lastChar) && (secondLastChar == '_' || secondLastChar == '-' || 
                    !char.IsLetterOrDigit(secondLastChar)))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private void ApplyLetterBlockBackground(MyCubeGrid grid, HashSet<Vector3I> letterBlocks, Dictionary<Vector3I, int> colorResults)
        {
            var backgroundPositions = new HashSet<Vector3I>();
            var directions = new[]
            {
                // Direct adjacents
                Vector3I.Up, Vector3I.Down,
                Vector3I.Left, Vector3I.Right,
                Vector3I.Forward, Vector3I.Backward,
                // Diagonals on same plane (for better framing)
                new Vector3I(1, 1, 0), new Vector3I(1, -1, 0),
                new Vector3I(-1, 1, 0), new Vector3I(-1, -1, 0),
                new Vector3I(1, 0, 1), new Vector3I(1, 0, -1),
                new Vector3I(-1, 0, 1), new Vector3I(-1, 0, -1),
                new Vector3I(0, 1, 1), new Vector3I(0, 1, -1),
                new Vector3I(0, -1, 1), new Vector3I(0, -1, -1)
            };
            
            foreach (var letterPos in letterBlocks)
            {
                foreach (var dir in directions)
                {
                    var adjacentPos = letterPos + dir;
                    
                    // Skip if it's another letter block
                    if (letterBlocks.Contains(adjacentPos))
                        continue;
                    
                    var adjacentBlock = grid.GetCubeBlock(adjacentPos);
                    
                    // Only apply to existing blocks that don't have special colors
                    if (adjacentBlock != null && (!colorResults.ContainsKey(adjacentPos) || 
                        colorResults[adjacentPos] == (int)MilitaryColorScheme.ColorIndex.PrimaryHull ||
                        colorResults[adjacentPos] == (int)MilitaryColorScheme.ColorIndex.SecondaryHull))
                    {
                        backgroundPositions.Add(adjacentPos);
                    }
                }
            }
            
            // Apply background color
            foreach (var pos in backgroundPositions)
            {
                colorResults[pos] = (int)MilitaryColorScheme.ColorIndex.LetterBlockBackground;
            }
        }
    }
}