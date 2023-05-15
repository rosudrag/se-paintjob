using System.Collections.Generic;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace ClientPlugin.App.PaintFactors
{
    public class BlockExposureColorFactor : IColorFactor
    {
        private const float ExposureDarkenPercentage = 0.3f;
        private const float InteriorLightenPercentage = 0.4f;

        public bool AppliesTo(MySlimBlock block, MyCubeGrid grid)
        {
            return true; // This factor applies to all blocks
        }

        public Color GetColor(MySlimBlock block, MyCubeGrid grid, Color current, IList<Color> colors)
        {
            if (GridUtilities.IsExteriorBlock(block, grid))
            {
                return current.Darken(ExposureDarkenPercentage);
            }
            return current.Lighten(InteriorLightenPercentage);
        }

        public void Clean()
        {
            // No cleanup required for this factor
        }
    }
}