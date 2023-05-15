using System.Collections.Generic;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace ClientPlugin.App.PaintFactors
{
    public interface IColorFactor : ICleanable
    {
        bool AppliesTo(MySlimBlock block, MyCubeGrid grid);
        Color GetColor(MySlimBlock block, MyCubeGrid grid, Color current, IList<Color> colors);
    }

    public interface ICleanable
    {
        void Clean();
    }

}