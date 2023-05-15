using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRageMath;

namespace ClientPlugin.App.PaintFactors
{
    public class BaseBlockColorFactor : IColorFactor
    {
        private readonly Dictionary<string, Color> _blockTypeToColor = new Dictionary<string, Color>();

        public bool AppliesTo(MySlimBlock block, MyCubeGrid grid)
        {
            return true; // This factor applies to all blocks
        }

        public Color GetColor(MySlimBlock block, MyCubeGrid grid, Color current, IList<Color> colors)
        {
            if (!_blockTypeToColor.Any())
            {
                Initialize(grid, colors);
            }

            var blockType = block.BlockDefinition.Id.TypeId.ToString();
            var result = _blockTypeToColor[blockType];
            return result;
        }

        public void Clean()
        {
            _blockTypeToColor.Clear();
        }

        private void Initialize(MyCubeGrid grid, IList<Color> colors)
        {
            // Assign the first color to all non-functional blocks
            var nonFunctionalBlockTypes = grid.GetBlocks()
                .Where(block => !(block.FatBlock is IMyFunctionalBlock))
                .Select(block => block.BlockDefinition.Id.TypeId.ToString())
                .Distinct();

            foreach (var blockType in nonFunctionalBlockTypes)
            {
                _blockTypeToColor[blockType] = colors[0];
            }

            var nonMainColors = colors.Skip(1).ToArray();
            var nonMainColorsCount = nonMainColors.Length;
            var functionalBlockTypes = grid.GetBlocks()
                .Where(block => block.FatBlock is IMyFunctionalBlock)
                .Select(block => block.BlockDefinition.Id.TypeId.ToString())
                .Distinct();

            // Assign the remaining colors to functional blocks, if any remain
            if (nonMainColors.Any())
            {
                var colorIndex = 0;
                foreach (var blockType in functionalBlockTypes)
                {
                    _blockTypeToColor[blockType] = nonMainColors[colorIndex++];
                    colorIndex %= nonMainColorsCount; // Cycle through colors
                }
            }
            else
            {
                foreach (var blockType in functionalBlockTypes)
                {
                    _blockTypeToColor[blockType] = colors[0];
                }
            }
        }
    }
}