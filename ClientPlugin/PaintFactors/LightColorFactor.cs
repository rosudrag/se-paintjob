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
            var type = block.BlockDefinition.Id.TypeId;
            return type == typeof(MyObjectBuilder_LightingBlock) ||
                   type == typeof(MyObjectBuilder_InteriorLight) ||
                   type == typeof(MyObjectBuilder_ReflectorLight) ||
                   type == typeof(MyObjectBuilder_SignalLight);
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