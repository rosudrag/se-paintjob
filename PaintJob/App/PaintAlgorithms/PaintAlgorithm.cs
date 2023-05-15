using PaintJob.App.PaintFactors;
using PaintJob.App.Systems;
using Sandbox.Game.Entities;

namespace PaintJob.App.PaintAlgorithms
{
    public abstract class PaintAlgorithm : ICleanable
    {
        public abstract void Clean();

        public abstract void Apply(MyCubeGrid grid);
    }
}