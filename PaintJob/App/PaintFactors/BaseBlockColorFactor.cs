using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRageMath;

namespace PaintJob.App.PaintFactors
{
    public class BaseBlockColorFactor : BaseFactor
    {
        private readonly Dictionary<string, Color> _blockTypeToColor = new Dictionary<string, Color>();

        public override Dictionary<Vector3I, Color> Apply(MyCubeGrid grid, Dictionary<Vector3I, Color> currentColors)
        {
            Initialize(grid);
            var result = new Dictionary<Vector3I, Color>();
            var blocks = grid.GetBlocks();

            foreach (var block in blocks)
            {
                var blockType = block.BlockDefinition.Id.TypeId.ToString();
                result[block.Position] = _blockTypeToColor[blockType];
            }

            return result;
        }

        public override void Clean()
        {
            _blockTypeToColor.Clear();
        }

        private void Initialize(MyCubeGrid grid)
        {
            var colors = _stateSystem.GetColors().ToArray();

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