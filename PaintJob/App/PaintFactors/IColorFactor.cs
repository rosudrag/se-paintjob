using System.Collections.Generic;
using Sandbox.Game.Entities;
using VRageMath;

namespace PaintJob.App.PaintFactors
{
    public interface IColorFactor : ICleanable
    {
        Dictionary<Vector3I,Color> Apply(MyCubeGrid grid, Dictionary<Vector3I, Color> currentColors);
    }

    public interface ICleanable
    {
        void Clean();
    }

}