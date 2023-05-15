using System.Collections.Generic;
using PaintJob.App.PaintFactors;
using Sandbox.Game.Entities;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms
{
    public class RudimentaryPaintJob : PaintAlgorithm
    {
        private Dictionary<Vector3I, Color> _colorCache;
        private readonly List<IColorFactor> _colorFactors;

        public RudimentaryPaintJob()
        {
            _colorFactors = new List<IColorFactor>
            {
                new BaseBlockColorFactor(),
                new BlockExposureColorFactor(),
                new EdgeBlockColorFactor(),
                new LightBlockColorFactor()
            };
            _colorCache = new Dictionary<Vector3I, Color>();
        }

        public override void Clean()
        {
            foreach (var colorFactor in _colorFactors)
            {
                colorFactor.Clean();
            }
            _colorCache.Clear();
        }

        public override void Apply(MyCubeGrid grid)
        {
            // Compute the color changes and store them in the cache
            foreach (var factor in _colorFactors)
            {
                _colorCache = factor.Apply(grid, _colorCache);
            }

            var blocks = grid.GetBlocks();
            foreach (var block in blocks)
            {
                if (!_colorCache.TryGetValue(block.Position, out var value))
                    continue;
                var color = value.ColorToHSVDX11();
                var original = block.ColorMaskHSV;
                if (original != color)
                {
                    grid.SkinBlocks(block.Position, block.Position, color, null, false);
                }
            }
        }
    }
}