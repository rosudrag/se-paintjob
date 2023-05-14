using System.Collections.Generic;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace ClientPlugin.PaintFactors
{
    public class EdgeBlockColorFactor : IColorFactor
    {
        public bool AppliesTo(MySlimBlock block, MyCubeGrid grid)
        {
            return GridUtilities.IsEdgeBlock(block, grid);
        }

        public Color GetColor(MySlimBlock block, MyCubeGrid grid, Color current, IList<Color> colors)
        {
            return current.Lighten(0.1f);
        }

        public void Clean()
        {
        }
    }
}