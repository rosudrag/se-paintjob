using System.Collections.Generic;
using System.Linq;
using ClientPlugin.PaintFactors;
using Sandbox.Game.Entities;
using VRageMath;

namespace ClientPlugin.PaintAlgorithms
{
    public class FadedPaint : PaintAlgorithm
    {
        private readonly Dictionary<Vector3I, Color> _colorCache;
        private readonly List<IColorFactor> _colorFactors;

        public FadedPaint(IPaintJobStateSystem stateSystem) : base(stateSystem)
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
            var colors = StateSystem.GetColors().ToList();
            var blocks = grid.GetBlocks();

            // Compute the color changes and store them in the cache
            foreach (var factor in _colorFactors)
            {
                foreach (var block in blocks)
                {
                    if (factor.AppliesTo(block, grid))
                    {
                        var currentColor = _colorCache.TryGetValue(block.Position, out var value) ? value : block.ColorMaskHSV.HSVtoColor();
                        var newColor = factor.GetColor(block, grid, currentColor, colors);
                        _colorCache[block.Position] = newColor;
                    }
                }
            }

            foreach (var block in blocks)
            {
                var color = _colorCache[block.Position].ColorToHSVDX11();
                var original = block.ColorMaskHSV;
                if (original != color)
                {
                    grid.ChangeColorAndSkin(block, color);
                }
            }
        }
    }
}