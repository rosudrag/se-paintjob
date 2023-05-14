using System.Collections.Generic;
using System.Linq;
using ClientPlugin.PaintFactors;
using Sandbox.Game.Entities;
using VRageMath;

namespace ClientPlugin.PaintAlgorithms
{
    public class ThemedPaintAlgorithm : PaintAlgorithm
    {
        private readonly List<IColorFactor> _colorFactors;

        public ThemedPaintAlgorithm(IPaintJobStateSystem stateSystem) : base(stateSystem)
        {
            _colorFactors = new List<IColorFactor>
            {
                new ThrusterColorFactor(),
                new LightColorFactor(),
                new DefaultColorFactor() // Make sure this is last, as it applies to all blocks
            };
        }

        public override void Apply(MyCubeGrid grid)
        {
            var colors = StateSystem.GetColors().ToList();
            var blocks = grid.GetBlocks();
            var gridSize = grid.Max - grid.Min;

            foreach (var block in blocks)
            {
                var relativePos = block.Position - grid.Min;

                foreach (var factor in _colorFactors)
                {
                    if (factor.AppliesTo(block))
                    {
                        var color = factor.GetColor(block, colors, gridSize, relativePos, grid);
                        var colorHSV = color.ColorToHSV();
                        grid.ChangeColorAndSkin(block, colorHSV);
                        break;
                    }
                }
            }
        }
        public override void Clean()
        {
            foreach (var factor in _colorFactors)
            {
                factor.Clean();
            }
        }
    }
}