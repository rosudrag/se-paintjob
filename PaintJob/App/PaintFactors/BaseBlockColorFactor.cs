using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRageMath;

namespace PaintJob.App.PaintFactors
{
    public class BaseBlockColorFactor : BaseFactor
    {
        private readonly Dictionary<string, int> _blockTypeToColorIndex = new Dictionary<string, int>();

        public override Dictionary<Vector3I, int> Apply(MyCubeGrid grid, Dictionary<Vector3I, int> currentColors)
        {
            Initialize(grid);
            var result = new Dictionary<Vector3I, int>();
            var blocks = grid.GetBlocks();

            foreach (var block in blocks)
            {
                var blockType = block.BlockDefinition.Id.TypeId.ToString();
                if(_blockTypeToColorIndex.TryGetValue(blockType, out var colorIndex))
                    result[block.Position] = colorIndex;
            }

            return result;
        }

        public override void Clean()
        {
            _blockTypeToColorIndex.Clear();
        }

        private void Initialize(MyCubeGrid grid)
        {
            DoNonFunctionalBlocks(grid);
            DoFunctionalBlocks(grid);
        }

        private void DoFunctionalBlocks(MyCubeGrid grid)
        {
            var functionalBlockTypes = grid.GetBlocks()
                .Where(block => block.FatBlock is IMyFunctionalBlock)
                .Select(block => block.BlockDefinition.Id.TypeId.ToString())
                .Distinct();

            var colorIndex = 7;
            foreach (var blockType in functionalBlockTypes)
            {
                _blockTypeToColorIndex[blockType] = colorIndex++;
                colorIndex %= 13; // cycle through colors (skip the first 7 colors) and wrap around at 13
                if(colorIndex == 0) colorIndex = 7;
            }
        }

        // Assign the first color to all non-functional blocks
        private void DoNonFunctionalBlocks(MyCubeGrid grid)
        {
            var functionalBlockTypes = grid.GetBlocks()
                .Where(block => !(block.FatBlock is IMyFunctionalBlock))
                .Select(block => block.BlockDefinition.Id.TypeId.ToString())
                .Distinct();

            var colorIndex = 0;
            foreach (var blockType in functionalBlockTypes)
            {
                _blockTypeToColorIndex[blockType] = colorIndex++;
                colorIndex %= 6; // Cycle through colors and wrap around at 6
            }
        }
    }
}