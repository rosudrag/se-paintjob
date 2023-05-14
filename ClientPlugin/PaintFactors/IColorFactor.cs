using System.Collections.Generic;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace ClientPlugin.PaintFactors
{
    public interface IColorFactor : ICleanable
    {
        bool AppliesTo(MySlimBlock block);
        Color GetColor(MySlimBlock block, IList<Color> colors, Vector3I gridSize, Vector3I relativePos, MyCubeGrid grid);
    }

    public interface ICleanable
    {
        void Clean();
    }

}