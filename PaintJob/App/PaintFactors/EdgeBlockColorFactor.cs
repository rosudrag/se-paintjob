using System;
using System.Collections.Generic;
using PaintJob.App.Extensions;
using PaintJob.Extensions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRageMath;

namespace PaintJob.App.PaintFactors
{
    public class EdgeBlockColorFactor : BaseFactor
    {
        public override Dictionary<Vector3I, int> Apply(MyCubeGrid grid, Dictionary<Vector3I, int> currentColors)
        {
            var result = new Dictionary<Vector3I, int>();
            result.AddRange(currentColors);
            var blocks = grid.GetBlocks();

            var random = new Random();

            foreach (var block in blocks)
            {
                if (!GridUtilities.IsEdgeBlock(block, grid))
                    continue;
                if (block.FatBlock is IMyFunctionalBlock)
                {
                    var secondaryColorRandom = random.Next(10, 14);
                    result[block.Position] = secondaryColorRandom;
                }
                else
                {
                    var secondaryColorRandom = random.Next(3, 7);
                    result[block.Position] = secondaryColorRandom;
                }

            }

            return result;
        }
    }
}