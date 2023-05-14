using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace ClientPlugin.PaintFactors
{
    public class LightColorFactor : IColorFactor
    {
        public bool AppliesTo(MySlimBlock block)
        {
            return block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_LightingBlock);
        }

        public Color GetColor(MySlimBlock block, IList<Color> colors, Vector3I gridSize, Vector3I relativePos, MyCubeGrid grid)
        {
            return colors[colors.Count - 1];
        }
        public void Clean()
        {

        }
    }
}