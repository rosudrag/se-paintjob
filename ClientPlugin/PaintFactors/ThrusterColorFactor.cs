using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace ClientPlugin.PaintFactors
{
    public class ThrusterColorFactor : IColorFactor
    {
        public bool AppliesTo(MySlimBlock block)
        {
            return block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Thrust);
        }

        public Color GetColor(MySlimBlock block, IList<Color> colors, Vector3I gridSize, Vector3I relativePos, MyCubeGrid grid)
        {
            return colors[0];
        }
        public void Clean()
        {

        }
    }
}