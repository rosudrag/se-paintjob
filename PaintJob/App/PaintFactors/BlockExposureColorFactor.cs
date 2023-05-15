using System.Collections.Generic;
using MonoMod.Utils;
using PaintJob.App.Extensions;
using Sandbox.Game.Entities;
using VRageMath;

namespace PaintJob.App.PaintFactors
{
    public class BlockExposureColorFactor : BaseFactor
    {
        private const float ExposureDarkenPercentage = 0.3f;
        private const float InteriorLightenPercentage = 0.4f;

        public override Dictionary<Vector3I, Color> Apply(MyCubeGrid grid, Dictionary<Vector3I, Color> currentColors)
        {
            var result = new Dictionary<Vector3I, Color>();
            result.AddRange(currentColors);
            var blocks = grid.GetBlocks();

            foreach (var block in blocks)
            {
                if (GridUtilities.IsExteriorBlock(block, grid))
                {
                    var current = currentColors[block.Position];
                    result[block.Position] = current.Darken(ExposureDarkenPercentage);
                }
            }

            return result;
        }
    }
}