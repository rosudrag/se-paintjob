using PaintJob.App.PaintFactors;
using Sandbox.Game.Entities;

namespace PaintJob.App.PaintAlgorithms
{
    public abstract class PaintAlgorithm : ICleanable
    {
        public abstract void Clean();

        protected abstract void Apply(MyCubeGrid grid);
        protected abstract void GeneratePalette(MyCubeGrid grid);

        public void Run(MyCubeGrid grid)
        {
            GeneratePalette(grid);
            Apply(grid);
        }

        public abstract void RunTest(MyCubeGrid targetGrid, string[] args);
    }
}