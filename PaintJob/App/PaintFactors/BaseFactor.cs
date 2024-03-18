using System.Collections.Generic;
using PaintJob.App.Systems;
using Sandbox.Game.Entities;
using VRageMath;

namespace PaintJob.App.PaintFactors
{
    public abstract class BaseFactor: IColorFactor
    {
        public virtual void Clean()
        {
            
        }
        public abstract Dictionary<Vector3I, Vector3> Apply(MyCubeGrid grid, Dictionary<Vector3I, Vector3> currentColors, Vector3[] palette);
    }
}