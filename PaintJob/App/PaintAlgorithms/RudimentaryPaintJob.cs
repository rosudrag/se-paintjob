using System.Collections.Generic;
using System.Linq;
using PaintJob.App.PaintFactors;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms
{
    public class RudimentaryPaintJob : PaintAlgorithm
    {
        private Dictionary<Vector3I, Vector3> _colorCache = new Dictionary<Vector3I, Vector3>();
        private readonly List<IColorFactor> _colorFactors = new List<IColorFactor>
        {
            new BaseBlockColorFactor(),
            new BlockExposureColorFactor(),
            new EdgeBlockColorFactor(),
            new LightBlockColorFactor()
        };

        private Vector3[] _colors;

        public override void Clean()
        {
            foreach (var colorFactor in _colorFactors)
            {
                colorFactor.Clean();
            }
            _colorCache.Clear();
        }

        protected override void Apply(MyCubeGrid grid)
        {
            // Compute the color changes and store them in the cache
            foreach (var factor in _colorFactors)
            {
                _colorCache = factor.Apply(grid, _colorCache, _colors);
            }

            var blocks = grid.GetBlocks();
            foreach (var block in blocks)
            {
                if (!_colorCache.TryGetValue(block.Position, out var value))
                    continue;
                grid.ColorBlocks(block.Position, block.Position, value, false);
            }
        }

        protected override void GeneratePalette(MyCubeGrid grid)
        {
            _colors = MyPlayer.ColorSlots.ToArray();
        }
    }
}