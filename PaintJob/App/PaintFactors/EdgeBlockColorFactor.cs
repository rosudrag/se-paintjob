using System.Collections.Generic;
using MonoMod.Utils;
using PaintJob.App.Extensions;
using Sandbox.Game.Entities;
using VRageMath;

namespace PaintJob.App.PaintFactors
{
    public class EdgeBlockColorFactor : BaseFactor
    {
        public override Dictionary<Vector3I, Color> Apply(MyCubeGrid grid, Dictionary<Vector3I, Color> currentColors)
        {
            var result = new Dictionary<Vector3I, Color>();
            result.AddRange(currentColors);
            
            var blocks = grid.GetBlocks();
            foreach (var block in blocks)
            {
                if (GridUtilities.IsEdgeBlock(block, grid))
                {
                    var current = result[block.Position];
                    result[block.Position] = current.Lighten(0.5f);
                }
            }

            return result;
        }
    }
}