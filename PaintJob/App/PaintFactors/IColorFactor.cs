using System.Collections.Generic;
using Sandbox.Game.Entities;
using VRageMath;

namespace PaintJob.App.PaintFactors
{
    public interface IColorFactor : ICleanable
    {
        Dictionary<Vector3I, Vector3> Apply(MyCubeGrid grid, Dictionary<Vector3I, Vector3> currentColors, Vector3[] palette);
    }

    public interface ICleanable
    {
        void Clean();
    }

}