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
        private Dictionary<Vector3I, int> _colorResult = new Dictionary<Vector3I, int>(); //position to index in palette
        private readonly List<IColorFactor> _colorFactors = new List<IColorFactor>
        {
            new BaseBlockColorFactor(),
            new BlockExposureColorFactor(),
            new EdgeBlockColorFactor(),
            // new LightBlockColorFactor()
        };

        private Vector3[] _colors;

        public override void Clean()
        {
            foreach (var colorFactor in _colorFactors)
            {
                colorFactor.Clean();
            }
            _colorResult.Clear();
        }

        protected override void Apply(MyCubeGrid grid)
        {
            // Compute the color changes and store them in the cache
            foreach (var factor in _colorFactors)
            {
                _colorResult = factor.Apply(grid, _colorResult);
            }

            var blocks = grid.GetBlocks();

            foreach (var block in blocks)
            {
                if (!_colorResult.TryGetValue(block.Position, out var colorIndex))
                    continue;
                
                // Color all positions occupied by multi-block structures
                grid.ColorBlocks(block.Min, block.Max, _colors[colorIndex], false);
            }
        }

        protected override void GeneratePalette(MyCubeGrid grid)
        {
            _colors = MyPlayer.ColorSlots.ToArray();
        }
        
    }
}