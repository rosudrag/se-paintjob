using System.Collections.Generic;
using PaintJob.App.Systems;
using Sandbox.Game.Entities;
using VRageMath;

namespace PaintJob.App.PaintFactors
{
    public abstract class BaseFactor: IColorFactor
    {
        protected IPaintJobStateSystem _stateSystem => IoC.Resolve<IPaintJobStateSystem>();
        public virtual void Clean()
        {
            
        }
        public abstract Dictionary<Vector3I, Color> Apply(MyCubeGrid grid, Dictionary<Vector3I, Color> currentColors);
    }
}