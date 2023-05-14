using ClientPlugin.PaintFactors;
using Sandbox.Game.Entities;

namespace ClientPlugin.PaintAlgorithms
{
    public abstract class PaintAlgorithm : ICleanable
    {
        protected readonly IPaintJobStateSystem StateSystem;

        protected PaintAlgorithm(IPaintJobStateSystem stateSystem)
        {
            StateSystem = stateSystem;
        }
        public abstract void Clean();

        public abstract void Apply(MyCubeGrid grid);
    }
}