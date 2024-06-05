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
    public class BlockExposureColorFactor : BaseFactor
    {
        public override Dictionary<Vector3I, int> Apply(MyCubeGrid grid, Dictionary<Vector3I, int> currentColors)
        {
            var result = new Dictionary<Vector3I, int>();
            result.AddRange(currentColors);
            var blocks = grid.GetBlocks();

            var random = new Random();

            foreach (var block in blocks.Where(block => GridUtilities.IsExteriorBlock(block, grid)))
            {
                if (block.FatBlock is IMyFunctionalBlock)
                {
                    var secondaryColorRandom = random.Next(7, 10);
                    result[block.Position] = secondaryColorRandom;
                }
                else
                {
                    var secondaryColorRandom = random.Next(0, 3);
                    result[block.Position] = secondaryColorRandom;
                }
            }

            return result;
        }
    }
}