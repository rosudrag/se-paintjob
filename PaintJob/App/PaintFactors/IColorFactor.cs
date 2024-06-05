using System.Collections.Generic;
using Sandbox.Game.Entities;
using VRageMath;

namespace PaintJob.App.PaintFactors
{
    public interface IColorFactor : ICleanable
    {
        Dictionary<Vector3I, int> Apply(MyCubeGrid grid, Dictionary<Vector3I, int> currentColors);
    }

    public interface ICleanable
    {
        void Clean();
    }

}