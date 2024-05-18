using System;
using System.Collections.Generic;
using System.Linq;
using PaintJob.App.Extensions;
using PaintJob.Extensions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRageMath;

namespace PaintJob.App.PaintFactors
{
    public class EdgeBlockColorFactor : BaseFactor
    {
        public override Dictionary<Vector3I, Vector3> Apply(MyCubeGrid grid, Dictionary<Vector3I, Vector3> currentColors, Vector3[] palette)
        {
            var nonFuncBlockColors = palette.Skip(3).Take(4).ToArray();
            var funcBlockColors = palette.Skip(10).ToArray();

            var result = new Dictionary<Vector3I, Vector3>();
            result.AddRange(currentColors);
            var blocks = grid.GetBlocks();

            var random = new Random();

            foreach (var block in blocks)
            {
                if (!GridUtilities.IsEdgeBlock(block, grid))
                    continue;
                if (block.FatBlock is IMyFunctionalBlock)
                {
                    var secondaryColorRandom = random.Next(funcBlockColors.Length);
                    var secondaryColor = funcBlockColors[secondaryColorRandom];
                    result[block.Position] = secondaryColor;
                }
                else
                {
                    var secondaryColorRandom = random.Next(nonFuncBlockColors.Length);
                    var secondaryColor = nonFuncBlockColors[secondaryColorRandom];
                    result[block.Position] = secondaryColor;
                }

            }

            return result;
        }
    }
}