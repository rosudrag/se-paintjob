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

        public override Dictionary<Vector3I, Vector3> Apply(MyCubeGrid grid, Dictionary<Vector3I, Vector3> currentColors, Vector3[] palette)
        {
            Initialize(grid, palette);
            var result = new Dictionary<Vector3I, Vector3>();
            var blocks = grid.GetBlocks();

            foreach (var block in blocks)
            {
                var color = palette[0];

                try
                {
                    var blockType = block.BlockDefinition.Id.TypeId.ToString();
                    result[block.Position] = _blockTypeToColor[blockType];
                }
                catch
                {
                    result[block.Position] = color;
                }

            }

            return result;
        }

        public override void Clean()
        {
            _blockTypeToColor.Clear();
        }

        private void Initialize(MyCubeGrid grid, Vector3[] colors)
        {
            DoNonFunctionalBlocks(grid, colors);
            DoFunctionalBlocks(grid, colors);
        }

        private void DoFunctionalBlocks(MyCubeGrid grid, Vector3[] colors)
        {
            var nonMainColors = colors.Skip(7).ToArray();
            var nonMainColorsCount = nonMainColors.Length;
            var functionalBlockTypes = grid.GetBlocks()
                .Where(block => block.FatBlock is IMyFunctionalBlock)
                .Select(block => block.BlockDefinition.Id.TypeId.ToString())
                .Distinct();

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

        // Assign the first color to all non-functional blocks
        private void DoNonFunctionalBlocks(MyCubeGrid grid, Vector3[] colors)
        {
            var nonMainColors = colors.Take(7).ToArray();
            var nonMainColorsCount = nonMainColors.Length;
            var functionalBlockTypes = grid.GetBlocks()
                .Where(block => !(block.FatBlock is IMyFunctionalBlock))
                .Select(block => block.BlockDefinition.Id.TypeId.ToString())
                .Distinct();

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